using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Lawrence.Game;

namespace Lawrence.Core;

public partial class Client {
    [PacketHandler(MPPacketType.MP_PACKET_ACK, RequiresHandshake = false)]
    public static void HandleAck(PacketContext context) {
        foreach (var unacked in context.Client._unacked.ToArray()) {
            if (unacked.AckCycle == context.Header.AckCycle &&
                unacked.AckIndex == context.Header.RequiresAck) {
                context.Client._unacked.Remove(unacked);
                unacked.AckCallback?.Invoke();
            }
        }
    }
    
    [PacketHandler(MPPacketType.MP_PACKET_SYN, RequiresHandshake = false)]
    public static void HandleSyn(PacketContext context) {
            var response = Packet.MakeAckPacket();
                    
            if (!context.Client._handshakeCompleted) {
                context.Client._handshakeCompleted = true;
            }
            else {
                response.Header.AckCycle = 0;
                response.Header.RequiresAck = 0;
            }

            context.Client.SendPacket(response);
    }
    
    [PacketHandler(MPPacketType.MP_PACKET_SYN_LE, RequiresHandshake = false)]
    public static void HandleSynLE(PacketContext context) {
        if (!context.Client._processedFirstPacket) {
            context.Client._endianness = Packet.Endianness.LittleEndian;
            
            Logger.Log($"Player is little endian.");
            
            HandleSyn(context);
            return;
        }
                    
        Logger.Log("Player tried to change endianness after handshake.");
                    
        context.Client.Disconnect();
    }
    
    [PacketHandler(MPPacketType.MP_PACKET_DISCONNECTED)]
    public static void HandleDisconnect(PacketContext context) {
        context.Client.Disconnect();
        
        Logger.Log("Player disconnected.");
        Lawrence.ForceDirectorySync();
    }
    
    [PacketHandler(MPPacketType.MP_PACKET_CONNECT, RequiresHandshake = false)]
    public static void HandleConnect(PacketContext context) {
        if (!context.Client.WaitingToConnect) {
            Logger.Error("Player tried to connect twice.");
            return;
        }
        
        if (context.GetPacketSize() < (uint)Marshal.SizeOf<MPPacketConnect>()) {
            // Legacy client, we tell it to fuck off with an unknown error code, since it doesn't know about
            //  the "outdated version" return code. 
            Logger.Log("Legacy client tried to connect.");
            
            MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                status = MPPacketConnectResponseStatus.ERROR_UNKNOWN
            };
            
            context.RPCRespond([connectResponse]);
            context.Client.Disconnect();
            return;
        }
        
        var connect = context.Get<MPPacketConnect>();
        string username = connect.GetUsername(context.Body);
        
        // Check that the API versions are compatible
        if (connect.Version < API_VERSION_MIN) {
            Logger.Log($"{username} tried to connect with old version {connect.Version}. Minimum version: {API_VERSION_MIN}. Latest: {API_VERSION}");
            // Client is outdated, tell them to update.
            MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                status = connect.Version == 0 ? 
                    MPPacketConnectResponseStatus.ERROR_UNKNOWN : // Legacy alpha doesn't support ERROR_OUTDATED
                    MPPacketConnectResponseStatus.ERROR_OUTDATED
            };
            
            context.RPCRespond([connectResponse]);
            context.Client.Disconnect();
        }
        
        foreach (Client c in context.Client._server.Clients()) {
            if (c != context.Client && c.GetUsername() == username) {
                if (c.GetUserid() == connect.UserId && c.GetEndpoint().Address.Equals(context.Client.GetEndpoint().Address)) {
                    c.Disconnect();
                    break;
                }
            
                MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                    status = MPPacketConnectResponseStatus.ERROR_USER_ALREADY_CONNECTED
                };
                
                context.RPCRespond([connectResponse]);
                context.Client.Disconnect();
                
                return;
            }
        }
        
        MPPacketConnectResponse responseBody = new MPPacketConnectResponse {
            status = MPPacketConnectResponseStatus.SUCCESS
        };
        
        context.Client._username = username;
        context.Client._userid = connect.UserId;
        
        context.RPCRespond([responseBody]);
        
        Game.Game.Shared().OnPlayerConnect(context.Client);
        
        context.Client.WaitingToConnect = false;
        
        Logger.Log($"New player {username} connected!");
        Lawrence.ForceDirectorySync();
        
        context.Client.DataStreamKey = (uint)Guid.NewGuid().GetHashCode();
        context.Client.SendPacket(Packet.MakeOpenDataStreamPacket(context.Client.DataStreamKey.Value));
    }

    [PacketHandler(MPPacketType.MP_PACKET_TIME_SYNC, RequiresHandshake = false)]
    public static void HandleTimeSync(PacketContext context) {
        MPPacketTimeResponse response = new MPPacketTimeResponse {
            ClientSendTime = (ulong)context.Header.TimeSent,
            ServerSendTime = Server.Time()
        };
        
        context.RPCRespond([response]);
    }

    [PacketHandler(MPPacketType.MP_PACKET_MOBY_UPDATE)]
    public static void HandleMobyUpdate(PacketContext context) {
        context.GetHandler()?.UpdateMoby(context.Get<MPPacketMobyUpdate>());
    }

    [PacketHandler(MPPacketType.MP_PACKET_MOBY_CREATE)]
    public static void HandleMobyCreate(PacketContext context) {
        var client = context.Client;
        var header = context.Header;
        var create = context.Get<MPPacketMobyCreate>();

        if (context.GetHandler() is not { } player) {
            throw new Exception("Client is not ready to create moby");
        }
        
        var newMoby = player.CreateMoby(create.OClass, create.SpawnId);
        
        if (newMoby == null) {
            Logger.Error($"Client({client.ID}) failed to create moby [oClass:{create.OClass}].");
            return;
        }
        
        newMoby.modeBits = create.ModeBits;

        if (create.Flags.HasFlag(MPMobyFlags.MP_MOBY_FLAG_ATTACHED_TO)) {
            var parent = create.ParentUuid == 0 ?
                player.Moby() :
                client.GetSyncMobyByInternalId(create.ParentUuid);
            
            if (parent != null) {
                newMoby.AttachedTo = parent;
                newMoby.PositionBone = create.PositionBone;
                newMoby.TransformBone = create.TransformBone;

                if (create.OClass == 173) {
                    newMoby.PositionBone = (byte)(create.PositionBone == 6 ? 22 : 23);
                    newMoby.TransformBone = (byte)(create.PositionBone == 6 ? 22 : 23);
                }
            } else {
                Logger.Error($"Player({client.ID}) tried to attach moby [oClass:{create.OClass}] to a parent [{create.ParentUuid}] that doesn't exist.");
            }
        }

        if (player.Moby() is { } moby) {
            newMoby.MakeSynced(moby);
        }

        var internalId = client.CreateSyncMoby(newMoby);
        
        MPPacketMobyCreateResponse mobyCreateResponse = new MPPacketMobyCreateResponse {
            Uuid = internalId
        };
        
        context.RPCRespond([mobyCreateResponse]);
    }

    [PacketHandler(MPPacketType.MP_PACKET_MOBY_CREATE_FAILURE)]
    public static void HandleMobyCreateFailure(PacketContext context) {
        var createFailure = context.Get<MPPacketMobyCreateFailure>();
        var client = context.Client;
        
        switch (createFailure.Reason) {
            case MPMobyCreateFailureReason.UNKNOWN:
                Logger.Error($"Couldn't create moby for Player({client.ID}): unknown error.");
                break;
            case MPMobyCreateFailureReason.NOT_READY: return;
            case MPMobyCreateFailureReason.ALREADY_EXISTS:
                Logger.Error($"Couldn't create moby for Player({client.ID}): already exists.");
                break;
            case MPMobyCreateFailureReason.MAX_MOBYS:
                Logger.Error($"Couldn't create moby for Player({client.ID}): out of moby space.");
                break;
            case MPMobyCreateFailureReason.UPDATE_NON_EXISTENT:
                Logger.Error($"Couldn't create moby for Player({client.ID}): tried to update a moby that doesn't exist.");
                break;
            case MPMobyCreateFailureReason.SUCCESS:
                client.SetMobyCreated(createFailure.Uuid);
                return;
        }
                
        client.ClearMoby(createFailure.Uuid);
    }

    [PacketHandler(MPPacketType.MP_PACKET_MOBY_DELETE)]
    public static void HandleMobyDelete(PacketContext context) {
        var client = context.Client;
        var delete = context.Get<MPPacketMobyDelete>();
        
        var moby = client.GetSyncMobyByInternalId((ushort)delete.Uuid);
        if (moby != null) {
            context.GetHandler()?.DeleteMoby(moby);
        } else {
            Logger.Error($"Player [{client.ID}] tried to delete a moby that doesn't exist in its moby table.");
        }
    }

    [PacketHandler(MPPacketType.MP_PACKET_MOBY_DAMAGE)]
    public static void HandleMobyDamage(PacketContext context) {
        var client = context.Client;
        var player = context.GetHandler();
        var damage = context.Get<MPPacketMobyDamage>();
        
        var collider = damage.Uuid == 0
            ? player?.Moby()
            : client.GetMobyByInternalId(damage.Uuid);
        
        var collidee = damage.CollidedWithUuid == 0
            ? player?.Moby()
            : client.GetMobyByInternalId(damage.CollidedWithUuid);
        
        if (!damage.Flags.HasFlag(MPDamageFlags.GameMoby)) {
            // This is an attack against a server-spawned moby, like another player or other entity.
            if (collider is {} _ && collidee is {} __) {
                player?.OnDamage(collider, collidee, damage.SourceOClass, damage.Damage);
            }
        }
    }

    [PacketHandler(MPPacketType.MP_PACKET_SET_STATE)]
    public static void HandleSetState(PacketContext context) {
        var client = context.Client;
        var player = context.GetHandler();
        var state = context.Get<MPPacketSetState>();

        if (state.StateType == MPStateType.MP_STATE_TYPE_GAME) {
            player?.GameStateChanged((GameState)state.Value);
        }

        if (state.StateType == MPStateType.MP_STATE_TYPE_COLLECTED_GOLD_BOLT) {
            player?.CollectedGoldBolt((int)state.Offset, (int)state.Value);
        }

        if (state.StateType == MPStateType.MP_STATE_TYPE_UNLOCK_ITEM) {
            // TODO: Clean up this bitwise magic into readable flags
            uint item = state.Value & 0xFFFF;
            bool equip = (state.Value >> 16) == 1;
            
            player?.UnlockItem((int)item, equip);
        }

        if (state.StateType == MPStateType.MP_STATE_TYPE_UNLOCK_LEVEL) {
            player?.OnUnlockLevel((int)state.Value);
        }

        if (state.StateType == MPStateType.MP_STATE_TYPE_GIVE_BOLTS) {
            player?.OnGiveBolts((int)state.Value, state.Offset);
        }
            
        if (state.StateType == MPStateType.MP_STATE_TYPE_UNLOCK_SKILLPOINT) {
            player?.OnUnlockSkillpoint((byte)state.Value);
        }

        if (state.StateType == MPStateType.MP_STATE_START_IN_LEVEL_MOVIE) {
            player?.OnStartInLevelMovie(state.Value, state.Offset);
        }

        if (state.StateType == MPStateType.MP_STATE_STANDING_ON_MOBY) {
            Moby? moby = null;
            if (state.Value > 0) {
                moby = client.GetMobyByInternalId((ushort)state.Value);
            }
            
            player?.OnPlayerStandingOnMoby(moby);
        }
    }

    [PacketHandler(MPPacketType.MP_PACKET_CONTROLLER_INPUT)]
    public static void HandleControllerInput(PacketContext context) {
        var input = context.Get<MPPacketControllerInput>();
        
        if ((input.Flags & MPControllerInputFlags.MP_CONTROLLER_FLAGS_HELD) != 0) {
            context.GetHandler()?.ControllerInputHeld((ControllerInput)input.Input);
        }
        
        if ((input.Flags & MPControllerInputFlags.MP_CONTROLLER_FLAGS_PRESSED) != 0) {
            ControllerInput pressedButtons = (ControllerInput)input.Input;
        
            context.GetHandler()?.ControllerInputTapped(pressedButtons);
        }
    }

    [PacketHandler(MPPacketType.MP_PACKET_PLAYER_RESPAWNED)]
    public static void HandlePlayerRespawned(PacketContext context) {
        var spawned = context.Get<MPPacketSpawned>();
        context.GetHandler()?.PlayerRespawned(spawned.SpawnId, spawned.LevelId);
    }

    [PacketHandler(MPPacketType.MP_PACKET_MONITORED_VALUE_CHANGED)]
    public static void HandleMonitoredValueChanged(PacketContext context) {
        var valueChanged = context.Get<MPPacketMonitoredValueChanged>();
    
        context.GetHandler()?.OnHybridMobyValueChange(
            valueChanged.Uid,
            valueChanged.Flags == 1 ? MonitoredValueType.Attribute : MonitoredValueType.PVar,
            valueChanged.Offset,
            valueChanged.Size,
            BitConverter.GetBytes(valueChanged.OldValue),
            BitConverter.GetBytes(valueChanged.NewValue)
        );
    }

    [PacketHandler(MPPacketType.MP_PACKET_MONITORED_ADDRESS_CHANGED)]
    public static void HandleMonitoredAddressChanged(PacketContext context) {
        var addressChanged = context.Get<MPPacketMonitoredAddressChanged>();
        
        context.GetHandler()?.OnMonitoredAddressChanged(
            addressChanged.Address,
            (byte)addressChanged.Size,
            BitConverter.GetBytes(addressChanged.OldValue),
            BitConverter.GetBytes(addressChanged.NewValue)
        );
    }

    [PacketHandler(MPPacketType.MP_PACKET_LEVEL_FLAG_CHANGED)]
    public static void HandleLevelFlagChanged(PacketContext context) {
        var body = context.Body.ToList();
        var flagChanged = context.Get<MPPacketLevelFlagsChanged>();
        
        for (var i = 0; i < flagChanged.Flags; i++) {
            var offset = Marshal.SizeOf<MPPacketLevelFlagsChanged>() + Marshal.SizeOf<MPPacketLevelFlag>() * i;
            if (Packet.BytesToStruct<MPPacketLevelFlag>(body.Skip(offset).ToArray(), context.Client._endianness) is not {} levelFlag) {
                throw new NetworkParsingException($"Failed to parse level flag {i} of {flagChanged.Flags}.");
            }
            
            context.GetHandler()?.OnLevelFlagChanged(
                flagChanged.Type,
                flagChanged.Level,
                levelFlag.Size,
                levelFlag.Index,
                levelFlag.Value
            );
        }
    }

    [PacketHandler(MPPacketType.MP_PACKET_UI_EVENT)]
    public static void HandleUIEvent(PacketContext context) {
        var uiEvent = context.Get<MPPacketUIEvent>();
                    
        byte[] extraData = uiEvent.GetExtraData(context.Body);
        
        context.GetHandler()?.UIEvent(uiEvent.EventType, uiEvent.ElementId, uiEvent.Data, extraData);
    }

    [PacketHandler(MPPacketType.MP_PACKET_QUERY_GAME_SERVERS, RequiresHandshake = false, RequiresDirectoryMode = true)]
    public static void HandleQueryGameServers(PacketContext context) {
        var header = context.Header;
        var client = context.Client;

        if (header.Size < Marshal.SizeOf<MPPacketQueryGameServers>()) {
            Logger.Error(
                $"(Player {client.ID}) tried to query us with outdated version.");

            header.PacketType = MPPacketType.MP_PACKET_ACK;
            header.Size = 0x2d;
            header.TimeSent = (long)Server.Time();

            // We didn't use to have a version field on the query packet, and also no way to tell a
            // querying client that it is outdated. The byte array is a query response with 1 server named
            // "Your multiplayer mod is outdated." for the old client. 
            client.SendPacket(header, [
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x59, 0x6f, 0x75, 0x72, 0x20,
                0x6d, 0x75, 0x6c, 0x74, 0x69, 0x70, 0x6c, 0x61, 0x79, 0x65, 0x72, 0x20, 0x6d, 0x6f, 0x64, 0x20, 0x69,
                0x73, 0x20, 0x6f, 0x75, 0x74, 0x64, 0x61, 0x74, 0x65, 0x64, 0x2e
            ]);

            return;
        }

        if (Lawrence.Directory()?.Servers() is { } servers) {
            client.SendPacket(Packet.MakeQueryServerResponsePacket(servers, header.RequiresAck, header.AckCycle));
        }

        client.Disconnect();
    }

    [PacketHandler(MPPacketType.MP_PACKET_REGISTER_SERVER, RequiresHandshake = false, RequiresDirectoryMode = true)]
    public static void HandleRegisterServer(PacketContext context) {
        var client = context.Client;
        var serverInfo = context.Get<MPPacketRegisterServer>();
    
        string name = serverInfo.GetName(context.Body);
    
        uint ip = serverInfo.Ip != 0 ? serverInfo.Ip : (uint)client.GetEndpoint().Address.Address;
    
        IPAddress address = new IPAddress(ip);
    
        Lawrence.Directory()?.RegisterServer(
            address.ToString(), 
            serverInfo.Port, 
            name, 
            serverInfo.MaxPlayers, 
            serverInfo.PlayerCount, 
            serverInfo.GetDescription(context.Body), 
            serverInfo.GetOwner(context.Body)
        );
        
        client.Disconnect();
    }
}