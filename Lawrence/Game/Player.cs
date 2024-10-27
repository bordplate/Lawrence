using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lawrence.Game;
using NLua;

using Lawrence.Core;
using Lawrence.Game.UI;

namespace Lawrence.Game;

public enum MonitoredAddressDataType {
    Number,
    Float
}

public struct MonitoredAddress {
    public uint Address;
    public ushort Size;
    public MonitoredAddressDataType DataType;
}

/// <summary>
/// Networked Player entity. Communicates with Client to send and receive updates in the Player's game. 
/// </summary>
public partial class Player : Moby {
    private readonly Client _client;

    public GameState GameState = 0;
    
    private List<MonitoredAddress> _monitoredAddresses = new();

    public override float x {
        get { return _x; }
        set { base.x = value; _client.SendPacket(Packet.MakeSetPositionPacket(0, value)); }
    }

    public override float y {
        get { return _y; }
        set { base.y = value; _client.SendPacket(Packet.MakeSetPositionPacket(1, value)); }
    }

    public override float z {
        get { return _z; }
        set { base.z = value; _client.SendPacket(Packet.MakeSetPositionPacket(2, value)); }
    }
    
    public override float rotZ {
        get { return _rotZ; }
        set { base.rotZ = value; _client.SendPacket(Packet.MakeSetPositionPacket(6, (float)(((Math.PI / 180) * value) - Math.PI))); }
    }

    public override Color Color {
        get { return _color; }
        set { base.Color = value; _client.SendPacket(Packet.MakeMobyUpdateExtended(0, new []{ new Packet.UpdateMobyValue(0x38, this.Color.ToUInt()) })); }
    }

    private ushort _state = 0;
    public ushort state {
        get { return _state; }
        set { _state = value; _client.SendPacket(Packet.MakeSetPlayerStatePacket(value)); }
    }

    // Some animations can cause crashes in other games, we filter those for the time being. 
    private List<int> _filteredAnimationIDs = new() {
        130  // Gold bolt collect animation
    };

    public override int AnimationId {
        get => _animationID;
        set {
            if (_animationID != value && !_filteredAnimationIDs.Contains(value)) {
                _animationID = value;
                HasChanged = true;
            }
        }
    }

    public Player(Client client) {
        _client = client;
        _client.SetHandler(this);
        
        GeneratePlayerColor();

        InitializeInternalLuaEntity();
        
        // Set Player mobys as auto damageable and targetable by default
        modeBits = (ushort)(_modeBits | 0x1000 | 0x4000);
        
        Game.Shared().NotificationCenter().Subscribe<PostTickNotification>(OnPostTick);
    }

    public override void Delete() {
        foreach (var label in _labels) {
            label?.Delete();
        }

        base.Delete();
        
        Game.Shared().NotificationCenter().Unsubscribe<PostTickNotification>(OnPostTick);
    }

    public void GeneratePlayerColor() {
        // Generate player color based on username
        string username = _client.GetUsername();
        int hash = 0;
        
        for (int i = 0; i < username.Length; i++) {
            hash = username[i] + ((hash << 5) - hash);
        }
        
        int r = (hash >> 16) & 0xFF;
        int g = (hash >> 8) & 0xFF;
        int b = hash & 0xFF;
        
        Color = new Color {
            R = (byte)r,
            G = (byte)g,
            B = (byte)b,
            A = 255
        };
    }
}

#region Lua
partial class Player { 
    /// <summary>
    /// Initializes the Lua Player entity for t
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void InitializeInternalLuaEntity() {
        object player = Game.Shared().State()["Player"];

        if (!(player is LuaTable table)) {
            throw new Exception("Unable to create Player entity in Lua. `Player` is nil or not a LuaTable.");
        }

        if (table["new"] is not LuaFunction initializeFunction) {
            throw new Exception("Could not initialize new `Player` entity as initialize isn't a function on `Player` table");
        }

        object[] entity = initializeFunction.Call(table, this);

        if (entity.Length <= 0 || entity[0] is not LuaTable entityTable) {
            throw new Exception("Failed to initialize `Player` Lua entity. `Player` is not a Lua table");
        }

        SetLuaEntity(entityTable);
    }
}
#endregion

#region Gameplay related function
partial class Player {
    public void LoadLevel(string level) {
        _level = Universe().GetLevelByName(level);
        _level.Add(this);

        SendPacket(Packet.MakeGoToLevelPacket(_level.GameID()));
        
        RegisterHybridMobys();
    }

    public void RegisterHybridMobys() {
        foreach (Moby moby in Level().GetHybridMobys()) {
            SendPacket(Packet.MakeRegisterHybridMobyPacket(moby));
        }
    }

    public void GiveItem(ushort item, bool equip = false) {
        SendPacket(Packet.MakeSetItemPacket(item, equip));
    }
    
    public void UnlockSkillpoint(byte skillpoint) {
        SendPacket(Packet.MakeUnlockSkillpointPacket(skillpoint));
    }

    public void SetRespawn(float x, float y, float z, float rotationZ) {
        SendPacket(Packet.MakeSetRespawnPacket(x, y, z, (float)(((Math.PI / 180) * rotationZ) - Math.PI)));
    }

    public void Damage(int damage) {
        SendPacket(Packet.MakeDamagePacket((uint)damage));
    }

    public string Username() {
        return _client.GetUsername() ?? "<Unknown>";
    }

    public void SetInputState(uint state) {
        SendPacket(Packet.MakeSetPlayerInputStatePacket(state));
    }

    public void LockMovement() {
        state = 114;
        SetInputState(0x9);
    }

    public void UnlockMovement() {
        state = 0;
        SetInputState(0);
    }

    public void SetSpeed(float speed) {
        SendPacket(Packet.MakeSetAddressFloatPacket(0x969e74, speed));
    }

    public void SetBolts(int bolts) {
        SendPacket(Packet.MakeGiveBoltsPacket(bolts, setBolts: true));
    }

    public void GiveBolts(int bolts) {
        SendPacket(Packet.MakeGiveBoltsPacket(bolts));
    }

    public void UnlockLevel(int level) {
        SendPacket(Packet.MakeUnlockLevelPacket(level));
    }

    public void SetGhostRatchet(uint timeoutInFrames = 150) {
        SendPacket(Packet.MakeSetAddressValuePacket(0x969EAC, timeoutInFrames));
    }
    
    public void SetLevelFlags(byte type, byte level, ushort index, uint[] value) {
        for (int i = 0; i <= value.Length; i += 8) {
            SendPacket(Packet.MakeSetLevelFlagPacket(type, level, index, value.Skip(i).Take(8).ToArray()));
        }
    }

    public void ShowErrorMessage(string message) {
        _client.ShowErrorMessage(message);
    }

    /// <summary>
    /// Sets the level flags for the current level.
    /// </summary>
    public void SetCurrentLevelFlags() {
        List<uint> levelFlags1 = new();
        List<uint> levelFlags2 = new();
        
        foreach (var flag in Level().LevelFlags1) {
            levelFlags1.Add(flag);
        }
        
        foreach (var flag in Level().LevelFlags2) {
            levelFlags2.Add(flag);
        }
        
        SetLevelFlags(1, (byte)Level().GameID(), 0, levelFlags1.ToArray());
        SetLevelFlags(2, (byte)Level().GameID(), 0, levelFlags2.ToArray());
    }

    public void MonitorAddress(uint address, byte size, bool isFloat = false) {
        _monitoredAddresses.Add(new MonitoredAddress {
            Address = address,
            Size = size,
            DataType = isFloat ? MonitoredAddressDataType.Float : MonitoredAddressDataType.Number
        });
        
        SendPacket(Packet.MakeMonitorAddressPacket(address, size, 0));
    }
    
    public void SetAddressValue(uint address, uint value, byte size) {
        SendPacket(Packet.MakeSetAddressValuePacket(address, value, size));
    }
}
#endregion

#region Notification
partial class Player { 
    public override void OnTick(TickNotification notification) {
        _client.Tick();

        if (Parent() == null) {
            return;
        }
        
        base.OnTick(notification);
    }

    public void OnPostTick(PostTickNotification notification) {
        // Check if this client is still alive
        if (_client.IsDisconnected()) {
            Delete();
            
            return;
        }

        if (_client.GetInactiveSeconds() > Lawrence.CLIENT_INACTIVE_TIMEOUT_SECONDS) {
            Logger.Log($"Client {_client.ID} inactive for more than {Lawrence.CLIENT_INACTIVE_TIMEOUT_SECONDS} seconds.");

            // Notify client and delete client's mobys and their children
            _client.Disconnect();

            return;
        }

        // Nothing here for the player if they're not in a level yet.
        if (_level == null) {
            return;
        }

        UpdateLabels();
        
        // Find the nearest parent that masks visibility or use the level to update the client on surrounding,
        //  visible mobys.
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
            if (!moby.HasChanged || (moby.IsInstanced() && !moby.HasParent(this))) {
                continue;
            }

            float mobyDistance = DistanceTo(moby);
            if (mobyDistance > 10) {
                if (Game.Shared().Ticks() % (int)(Math.Max(1, mobyDistance / 10)) != 0) {
                    continue;
                }
            }
            
            Update(moby);
        }
    }

    public override void OnDeleteEntity(DeleteEntityNotification notification) {
        if (notification.Entity is Moby moby) {
            if (_client.HasMoby(moby)) {
                _client.DeleteMoby(moby);
            }
        }

        base.OnDeleteEntity(notification);
    }
}
#endregion

#region Networking
partial class Player {
    public void Update(Moby moby) {
        // We don't update ourselves.
        if (moby == this) {
            return;
        }
        
        if (!moby.IsHybrid) {
            _client.UpdateMoby(moby);
        }
    }

    public void NotifyDelete(Moby entity) {
        _client.DeleteMoby(entity);
    }

    /// <summary>
    /// Send a packet to the Player
    /// </summary>
    /// <important>
    /// This override must not call its base function. That could cause an infinite loop. 
    /// </important>
    /// <param name="packet"></param>
    public override void SendPacket(Packet packet)
    {
        _client.SendPacket(packet);
    }

    public bool Disconnected() {
        return _client.IsDisconnected();
    }

    public void Disconnect() {
        _client.Disconnect();
    }
    
    public void ChangeMobyAttribute(ushort uid, ushort offset, ushort size, object value, bool isFloat = false) {
        uint val = !isFloat ? Convert.ToUInt32(value) : BitConverter.ToUInt32(BitConverter.GetBytes(Convert.ToSingle(value)), 0);
        
        SendPacket(Packet.MakeChangeMobyValuePacket(uid, MonitoredValueType.Attribute, offset, size, val));
    }
    
    public void ChangeMobyPVar(ushort uid, ushort offset, ushort size, object value, bool isFloat = false) {
        uint val = !isFloat ? Convert.ToUInt32(value) : BitConverter.ToUInt32(BitConverter.GetBytes(Convert.ToSingle(value)), 0);
        
        SendPacket(Packet.MakeChangeMobyValuePacket(uid, MonitoredValueType.PVar, offset, size, val));
    }
}
#endregion

#region Client events
partial class Player : IClientHandler
{
    /// <summary>
    /// Called from a player whenever a moby they manage has collided with another moby. 
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="collidee"></param>
    /// <param name="aggressive">Set when the collision is an attack.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Collision(Moby collider, Moby collidee, bool aggressive = false)
    {
        if (collider == null) {
            throw new InvalidOperationException($"Player [{_client.GetEndpoint()}]: Got null-collider");
        }

        if (collidee == null) {
            return;
        }
        
        if (collider != this) {
            throw new InvalidOperationException($"Player [{_client.GetEndpoint()}]: Illegal collider for collision update");
        }

        if (collider == collidee) {
            throw new InvalidOperationException($"$Player [{_client.GetEndpoint()}]: Collider and collidee can't be the same entity. You can't collide with yourself.");
        }

        if (!aggressive) {
            collider.AddCollider(collidee);
            collidee.AddCollider(collider);
        }
        else {
            collidee.OnHit(collider);
            collider.OnAttack(collidee);
        }
    }

    /// <summary>
    /// Called for as long as a player is holding any buttons. 
    /// </summary>
    /// <param name="input">The currently held buttons.</param>
    public void ControllerInputHeld(ControllerInput input)
    {
        CallLuaFunction("OnControllerInputHeld", LuaEntity(), (int)input);
    }

    /// <summary>
    /// Called when a player releases a button
    /// </summary>
    /// <param name="input">The currently held inputs</param>
    public void ControllerInputReleased(ControllerInput input)
    {
        CallLuaFunction("OnControllerInputReleased", LuaEntity(), (int)input);
    }

    /// <summary>
    /// Called whenever a player has pressed a button, only fires once while a player is holding a button. 
    /// </summary>
    /// <param name="input">The pressed inputs, including any other buttons that are held.</param>
    public void ControllerInputTapped(ControllerInput input)
    {
        CallLuaFunction("OnControllerInputTapped", LuaEntity(), (int)input);
    }

    /// <summary>
    /// Called when a client wants to create a moby. A client could want to do this to spawn weapons they are holding,
    ///  a projectile from a weapon, Clank as a backpack, etc.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public uint CreateMoby()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called when a client sends updates about a Moby they manage. This is typically an update about their
    ///  hero moby (typically Ratchet, but potentially Clank, Big Clank, Hologuise, and more). 
    /// </summary>
    /// <param name="mobyUpdate">Packet describing the current state of the moby.</param>
    /// <exception cref="NotImplementedException">Clients can not yet send updates about mobys other than their hero moby. </exception>
    public void UpdateMoby(MPPacketMobyUpdate mobyUpdate)
    {
        // Update this moby if 0, update child moby if not 0
        if (mobyUpdate.uuid == 0) {
            SetActive((mobyUpdate.mpFlags & MPMobyFlags.MP_MOBY_FLAG_ACTIVE) > 0);

            if (mobyUpdate.x != _x || mobyUpdate.y != _y || mobyUpdate.z != _z) {
                HasChanged = true;
            }
            
            _x = mobyUpdate.x;
            _y = mobyUpdate.y;
            _z = mobyUpdate.z;
            oClass = mobyUpdate.oClass;
            _state = mobyUpdate.state;
            rotX = (float)((180 / Math.PI) * (mobyUpdate.rotX + Math.PI));
            rotY = (float)((180 / Math.PI) * (mobyUpdate.rotY + Math.PI));
            _rotZ = (float)((180 / Math.PI) * (mobyUpdate.rotZ + Math.PI));
            scale = mobyUpdate.scale;
            alpha = mobyUpdate.alpha / 128.0f;
            AnimationId = mobyUpdate.animationID;
            AnimationDuration = mobyUpdate.animationDuration;
            
            // Wait until we have a parent
            if (Parent() == null) {
                return;
            }

            if (mobyUpdate.level != Level().GameID()) {
                Level lastLevel = _level;
                
                _level.Remove(this, false);
                
                _level = Universe().GetLevelByGameID(mobyUpdate.level);
                _level.Add(this);
                Logger.Log($"Player moved from {lastLevel.GetName()} to {_level.GetName()}");
                
                SetCurrentLevelFlags();
            }
        } else {
            // Update child moby 
            throw new NotImplementedException("Players can't spawn child mobys yet");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Moby Moby() {
        return this;
    }

    /// <summary>
    /// Called after a player has respawned. 
    /// </summary>
    public void PlayerRespawned() {
        CallLuaFunction("OnRespawned", LuaEntity());
        
        SendPacket(Packet.MakeMobyUpdateExtended(0, new []{ new Packet.UpdateMobyValue(0x38, this.Color.ToUInt()) }));
        
        RegisterHybridMobys();
    }

    /// <summary>
    /// Called when a player's game state changes. E.g. from in-game to pause menu, cutscene, in-level movie (ILM)
    ///  or similar. 
    /// </summary>
    /// <param name="gameState">Game state this Player's client has changed to</param>
    public void GameStateChanged(GameState gameState) {
        this.GameState = gameState;
        CallLuaFunction("OnGameStateChanged", LuaEntity(), (int)gameState);
    }

    public void CollectedGoldBolt(int planet, int number) {
        CallLuaFunction("OnCollectedGoldBolt", LuaEntity(), planet, number);
    }

    public void UnlockItem(int itemId, bool equip) {
        CallLuaFunction("OnUnlockItem", LuaEntity(), itemId, equip);
    }

    public void OnUnlockLevel(int level) {
        CallLuaFunction("OnUnlockLevel", LuaEntity(), level);
    }

    public void OnUnlockSkillpoint(byte skillpoint) {
        CallLuaFunction("OnUnlockSkillpoint", LuaEntity(), skillpoint);
    }

    public void OnDisconnect() {
        Logger.Log($"{Username()} disconnected.");
        Game.Shared().NotificationCenter().Post(new PlayerDisconnectedNotification(0, Username(), this));
    }

    public void OnHybridMobyValueChange(ushort uid, MonitoredValueType type, ushort offset, ushort size, byte[] oldValue, byte[] newValue) {
        foreach (var moby in Level().GetHybridMobys()) {
            if (moby.UID == uid) {
                moby.OnHybridValueChanged(this, type, offset, size, oldValue, newValue);
            }
        }
    }
    
    public void OnMonitoredAddressChanged(uint address, byte size, byte[] oldValue, byte[] newValue) {
        foreach (var monitoredAddress in _monitoredAddresses) {
            if (monitoredAddress.Address == address) {
                if (monitoredAddress.DataType == MonitoredAddressDataType.Number) {
                    CallLuaFunction("MonitoredAddressChanged", LuaEntity(), address, BitConverter.ToUInt32(oldValue, 0), BitConverter.ToUInt32(newValue, 0));
                } else {
                    CallLuaFunction("MonitoredAddressChanged", LuaEntity(), address, BitConverter.ToSingle(oldValue, 0), BitConverter.ToSingle(newValue, 0));
                }
            }
        }
    }

    public void OnLevelFlagChanged(ushort type, byte level, byte size, ushort index, uint value) {
        if (Level().GameID() != level) {
            Console.Error.WriteLine($"Player [{_client.GetEndpoint()}]: Received level flag change for level {level} but is in level {Level().GameID()}");
            return;
        }
        
        Level().OnFlagChanged(this, type, size, index, value);
        
        CallLuaFunction("OnLevelFlagChanged", LuaEntity(), type, level, size, index, value);
    }
    
    public void OnGiveBolts(int boltdiff, uint totalBolts) {
        CallLuaFunction("OnGiveBolts", LuaEntity(), boltdiff, totalBolts);
    }
}
#endregion

#region User interface
partial class Player {
    /// <summary>
    /// We keep track of the labels we've assigned a user here, to match the way we track labels in the game client.
    /// Tuple where first item is the Label object, and the second is the hash we registered last tick. We use the hash
    ///     to only send updates to the user when we know something about the Label has updated. 
    /// </summary>
    private List<Label> _labels = new ();

    /// <summary>
    /// Adds a label to the player's screen.
    /// </summary>
    /// <param name="label"></param>
    public override void AddLabel(Label label) {
        // We loop through to check that we don't have that label or a label with the same hash already.
        // This mitigates issues if someone e.g. tries to add labels every tick, which can happen and be hard to debug
        //  for beginners.
        foreach (var _label in _labels) {
            if (_label == label) {
                return;
            }
        }

        // Register first empty index with null label and force set HasChanged to guarantee it's sent to the player next tick
        for (int i = 0; i < _labels.Count; i++) {
            if (_labels[i] == null) {
                label.ForceSetChanged();
                _labels[i] = label;
                return;
            }
        }
        
        // If it didn't return above, we just allocate more space for a new label
        _labels.Add(label);
    }

    /// <summary>
    /// Removes all the labels from the player's screen.
    /// </summary>
    public void RemoveAllLabels() {
        _labels = new List<Label>();
        
        for (int i = 0; i < _labels.Count; i++) {
            RemoveLabel(_labels[i]);
        }
    }

    /// <summary>
    /// Removes the given label from the player's screen.
    /// </summary>
    /// <param name="label">Label to remove</param>
    public override void RemoveLabel(Label label) {
        for (int i = 0; i < _labels.Count; i++) {
            if (_labels[i] == null) {
                continue;
            }
            
            if (_labels[i] == label) {
                _labels[i] = null;
                
                SendPacket(Packet.MakeDeleteHUDTextPacket((ushort)i));
            }
        }
    }

    /// <summary>
    /// Sends an update to the player about labels that need updates. 
    /// </summary>
    private void UpdateLabels() {
        for (int i = 0; i < _labels.Count; i++) {
            Label label = _labels[i];

            if (label == null) {
                continue;
            }

            // Continue if the label hasn't been updated
            // But we force resend every 1 second in case a client missed our call. 
            if (!label.HasChanged && Game.Shared().Ticks() % 60 != 0) {
                continue;
            }

            SendPacket(Packet.MakeSetHUDTextPacket((ushort)i, label.Text(), label.X(), label.Y(), label.Color(), label.States()));
        }
    }
    
    /// <summary>
    /// Shows a toast message on screen for 188 ticks by default.
    /// </summary>
    /// <param name="message">Message to show to player</param>
    public void ToastMessage(string message) {
        SendPacket(Packet.MakeToastMessagePacket(message));
    }

    /// <summary>
    /// Shows a toast message on screen for specified amount of ticks
    /// </summary>
    /// <param name="message">Message to show to player</param>
    /// <param name="duration">Duration in game ticks</param>
    public void ToastMessage(string message, uint duration) {
        SendPacket(Packet.MakeToastMessagePacket(message, duration));
    }
}
#endregion
