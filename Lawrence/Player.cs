using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using NLua;

namespace Lawrence
{
    /// <summary>
    /// Networked Player entity. Communicates with Client to send and receive updates in the Player's game. 
    /// </summary>
    public partial class Player : Moby {
        private readonly Client _client;
        public Player(Client client) {
            _client = client;
            _client.SetHandler(this);

            InitializeInteralLuaEntity();
        }

    }

    #region Lua
    partial class Player { 
        private void InitializeInteralLuaEntity() {
            object player = Game.Shared().State()["Player"];

            if (!(player is LuaTable)) {
                throw new Exception("Unable to create Player entity in Lua. `Player` is nil or not a LuaTable.");
            }

            if (!(((LuaTable)player)["new"] is LuaFunction)) {
                throw new Exception("Could not initialize new `Player` entity as initialize isn't a function on `Player` table");
            }

            LuaFunction initializeFunction = ((LuaFunction)((LuaTable)player)["new"]);

            object[] entity = initializeFunction.Call( new[] { player, this });

            if (entity.Length <= 0 || !(entity[0] is LuaTable)) {
                throw new Exception("Failed to initialize `Player` Lua entity. `Player` is not a Lua table");
            }

            SetLuaEntity((LuaTable)entity[0]);
        }
    }
    #endregion
    
    #region Gameplay related function

    partial class Player {
        public void LoadLevel(string level) {
            _level = _universe.GetLevelByName(level);
            _level.Add(this);

            SendPacket(Packet.MakeGoToPlanetPacket(_level.GetGameID()));
        }
    }
    #endregion

    #region Notification
    partial class Player { 
        public override void OnTick(TickNotification notification) {
            // Check if this client is still alive
            if (_client.IsDisconnected()) {
                return;
            }

            if (_client.GetInactiveSeconds() > Lawrence.CLIENT_INACTIVE_TIMEOUT_SECONDS) {
                Logger.Log($"Client {_client.ID} inactive for more than {Lawrence.CLIENT_INACTIVE_TIMEOUT_SECONDS} seconds.");

                // Notify client and delete client's mobys and their children
                _client.Disconnect();

                // TODO: Delete entity

                return;
            }
            
            // Find the nearest parent that masks visibility or use the level
            Entity visibilityGroup = this;

            do {
                if (visibilityGroup.MasksVisibility()) {
                    break;
                }

                if (visibilityGroup.Parent() == null) {
                    visibilityGroup = _level;
                    break;
                }

                visibilityGroup = visibilityGroup.Parent();
            } while (!visibilityGroup.MasksVisibility());

            foreach (Moby moby in visibilityGroup.Find<Moby>()) {
                Update(moby);
            }
            
            _client.Tick();

            base.OnTick(notification);
        }
    }
    #endregion

    #region Networking
    partial class Player {
        private Dictionary<string, ushort> _mobysTable = new Dictionary<string, ushort>();
        private (string, long)[] _mobys = new (string, long)[1024];

        public void Update(Moby moby) {
            // We don't update ourselves.
            if (moby == this) {
                return;
            }

            // 0 is invalid in this context. 0 refers to the game's hero moby and shouldn't be updated here. 
            ushort internalId = 0;

            // TODO: I think these GUID().ToString() calls might not be super performant.
            if (!_mobysTable.TryGetValue(moby.GUID().ToString(), out internalId)) {
                // Find next available ID
                for (ushort i = 0; i < _mobys.Length; i++) {
                    // Check if this is a stale moby that we should clear out to make new room
                    if (_mobys[i].Item1 != null && _mobys[i].Item2 < Game.Shared().Ticks() - 10) {
                        // If this moby hasn't been touched in 10 ticks, we tell the game to delete it and clear it out
                        SendPacket(Packet.MakeDeleteMobyPacket(i));
                        _mobys[i] = (null,0);
                    }
                    
                    if (_mobys[i].Item1 == null) {
                        internalId = i;
                        _mobys[i] = (moby.GUID().ToString(), Game.Shared().Ticks());
                        _mobysTable[moby.GUID().ToString()] = i;
                        
                        break;
                    }
                }

                if (internalId == 0) {
                    Logger.Error("A player has run out of moby space.");
                    return;
                }
            }
            
            // Update the last time we saw this moby
            _mobys[internalId].Item2 = Game.Shared().Ticks();
            
            SendPacket(Packet.MakeMobyUpdatePacket(internalId, moby));
        }
        
        /// <summary>
        /// Send a packet to the Player
        /// </summary>
        /// <important>
        /// This override must not call its base function. That could cause an infinite loop. 
        /// </important>
        /// <param name="packet"></param>
        public override void SendPacket((MPPacketHeader, byte[]) packet)
        {
            _client.SendPacket(packet);
        }
    }
    #endregion

    #region Client events
    partial class Player : IClientHandler
    {
        public void Collision(MPPacketMobyCollision collision)
        {
            ushort uuid = collision.uuid;
            ushort collidedWith = collision.collidedWith;

            //if (uuid == 0) {
            //    uuid = clientMoby.UUID;
            //}
            //
            //if (collidedWith == 0) {
            //    collidedWith = clientMoby.UUID;
            //}
            //
            //if (uuid == collidedWith) {
            //    Console.WriteLine($"Player {ID} just told us they collided with themselves.");
            //    return;
            //}
            //
            //Moby moby = null;
            //
            //if (moby != null) {
            //    moby.AddCollider(collidedWith, collision.flags);
            //} else {
            //    Console.WriteLine($"Player {ID} claims they hit null-moby {uuid}");
            //}

        }

        public void ControllerInputHeld(ControllerInput input)
        {
            throw new NotImplementedException();
        }

        public void ControllerInputReleased(ControllerInput input)
        {
            throw new NotImplementedException();
        }

        public void ControllerInputTapped(ControllerInput input)
        {
            throw new NotImplementedException();
        }

        public uint CreateMoby()
        {
            throw new NotImplementedException();
        }

        public void UpdateMoby(MPPacketMobyUpdate mobyUpdate)
        {
            // Update this moby if 0, update child moby if not 0
            if (mobyUpdate.uuid == 0) {
                this.active = (mobyUpdate.flags & MPMobyFlags.MP_MOBY_FLAG_ACTIVE) > 0;

                this.x = mobyUpdate.x;
                this.y = mobyUpdate.y;
                this.z = mobyUpdate.z;
                this.rot = mobyUpdate.rotation;
                this.animationID = mobyUpdate.animationID;
                this.animationDuration = mobyUpdate.animationDuration;
            } else {
                // Update child moby 
                throw new NotImplementedException("Player's can't spawn child mobys yet");
            }
        }
    }
    #endregion
}

