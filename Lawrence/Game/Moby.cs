using System;
using System.Collections.Generic;
using Lawrence.Game;
using NLua;

using Lawrence.Core;

namespace Lawrence.Game;

public struct Color {
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color(uint color) {
        R = (byte)(((color & 0x000000ff) >> 0) * 2);
        G = (byte)(((color & 0x0000ff00) >> 8) * 2);
        B = (byte)(((color & 0x00ff0000) >> 16) * 2);
        A = (byte)(((color & 0xff000000) >> 24) * 2);
    }

    public Color(byte r, byte g, byte b, byte a = 0xff) {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public uint ToUInt() {
        return (uint)((A/2 << 24) | (B/2 << 16) | (G/2 << 8) | R/2);
    }
}

public enum MonitoredValueType: byte {
    Attribute = 1,
    PVar = 2
}

public enum MonitoredValueDataType {
    Number,
    Float
}

public struct MonitoredValue {
    public ushort Offset;
    public ushort Size;
    public MonitoredValueType Flags;
    public MonitoredValueDataType DataType;
}

public class Moby : Entity
{
    /// <summary>
    /// The current level this moby is on. Levels are tied to universes such that mobys (incl. users) on the same
    /// level in different universes won't be able to see or interact with each other. 
    /// </summary>
    protected Level _level;
    
    static ushort COLLIDE_TICKS = 10;
    
    private int _oClass = 0;

    /// <summary>
    /// This is used when a moby is attached to a bone of another moby, like helmets, gear, backpack, etc. 
    /// </summary>
    public Moby AttachedTo;
    
    /// <summary>
    /// Which bone we are attached to that decides the position on the body.
    /// </summary>
    public byte PositionBone;
    
    /// <summary>
    /// We use this to determine which bone we follow the transformation of. This is often the same as PositionBone, but
    ///   can be different if we want to follow the rotation of one bone, but the position of another.
    /// </summary>
    public byte TransformBone;
    
    /// <summary>
    /// UID is only used for hybrid mobys. 
    /// </summary>
    public ushort UID { get; private set; }

    /// <summary>
    /// Hybrid mobys are mobys that are created in the game, but can be controlled by the server.
    /// </summary>
    public bool IsHybrid {
        get;
        private set;
    }

    /// <summary>
    /// Synced mobys come from a game and is fully controlled by the game.
    /// </summary>
    public Moby SyncOwner {
        get; 
        private set;
    }
    public int SyncSpawnId { get; set; }

    // TODO: Make these HashSets instead?
    public List<MonitoredValue> MonitoredAttributes { get; private set; } = new();
    public List<MonitoredValue> MonitoredPVars { get; private set; } = new();
    
    public int oClass { get => _oClass; set { if (_oClass != value) { _oClass = value; HasChanged = true; } } }
    
    protected float _x = 0.0f;
    public virtual float x { get => _x; set { if (_x != value) { _x = value; HasChanged = true; } } }
    
    protected float _y = 0.0f;
    public virtual float y { get => _y; set { if (_y != value) { _y = value; HasChanged = true; } } }
    
    protected float _z = 0.0f;
    public virtual float z { get => _z; set { if (_z != value) { _z = value; HasChanged = true; } } }
    
    private float _rotX = 0.0f;
    public float rotX { get => _rotX; set { if (_rotX != value) { _rotX = value; HasChanged = true; } } }
    
    private float _rotY = 0.0f;
    public float rotY { get => _rotY; set { if (_rotY != value) { _rotY = value; HasChanged = true; } } }
    
    protected float _rotZ = 0.0f;
    public virtual float rotZ { get => _rotZ; set { if (_rotZ != value) { _rotZ = value; HasChanged = true; } } }
    
    private float _scale = 1.0f;
    public float scale { get => _scale; set { if (_scale != value) { _scale = value; HasChanged = true; } } }
    
    private float _alpha = 1.0f;
    public float alpha { get => _alpha; set { if (_alpha != value) { _alpha = value; HasChanged = true; } } }

    protected int _animationID = 0;
    public virtual int AnimationId { get => _animationID; set { if (_animationID != value) { _animationID = value; HasChanged = true; } } }
    
    private int _animationDuration = 0;
    public int AnimationDuration { get => _animationDuration; set { if (_animationDuration != value) { _animationDuration = value; HasChanged = true; } } }
    
    protected ushort _modeBits = 0x10 | 0x400;
    public ushort modeBits { get => _modeBits; set { if (_modeBits != value) { _modeBits = value; HasChanged = true; } } }
    
    protected Color _color = new() {
        R = 255/2,
        G = 255/2,
        B = 255/2,
        A = 255
    };
    
    public virtual Color Color { get => _color; set { if (_color.ToUInt() != value.ToUInt()) { _color = value; HasChanged = true; } } }

    public bool HasChanged { get; protected set; }

    public void ResetChanged()
    {
        HasChanged = false;
    }


    public readonly bool MpUpdateFunc = true;
    public readonly bool CollisionEnabled = true;

    private Dictionary<Moby, ushort> _colliders = new();

    public Moby(LuaTable luaTable = null) : base(luaTable) {
        Game.Shared().NotificationCenter().Subscribe<PreTickNotification>(OnPreTick);
    }

    public override void Delete() {
        base.Delete();
        
        Game.Shared().NotificationCenter().Unsubscribe<PreTickNotification>(OnPreTick);
    }

    public void MakeHybrid(ushort uid) {
        UID = uid;
        IsHybrid = true;
    }
    
    public void MakeSynced(Moby moby) {
        SyncOwner = moby;
    }
    
    public Level Level() {
        if (_level == null) {
            _level = Universe().GetLevelByGameID(0);
        }
        
        return _level;
    }

    public void SetLevel(Level level) {
        if (this is Player) {
            throw new Exception("Do NOT use SetLevel() to set a Player's level. Instead use LoadLevel()");
        }
        
        if (_level != null) {
            _level.Remove(this);
        }
        
        _level = level;
        
        _level.Add(this);
    }

    public void SetColor(int r, int g, int b) {
        Color = new Color {
            R = (byte)r,
            G = (byte)g,
            B = (byte)b,
            A = Color.A
        };
    }

    public void SetOClass(int oClass) {
        this.oClass = oClass;
    }

    public void SetPosition(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void MonitorAttribute(ushort offset, ushort size, bool isFloat = false) {
        MonitoredAttributes.Add(new MonitoredValue {
            Offset = offset,
            Size = size,
            Flags = MonitoredValueType.Attribute,
            DataType = isFloat ? MonitoredValueDataType.Float : MonitoredValueDataType.Number
        });
    }
    
    public void MonitorPVar(ushort offset, ushort size, bool isFloat = false) {
        MonitoredPVars.Add(new MonitoredValue {
            Offset = offset,
            Size = size,
            Flags = MonitoredValueType.PVar,
            DataType = isFloat ? MonitoredValueDataType.Float : MonitoredValueDataType.Number
        });
    }

    public void OnHybridValueChanged(Player player, MonitoredValueType type, ushort offset, ushort size, byte[] oldValue,
        byte[] newValue) {
        object oldV = null;
        object newV = null;
        
        // Find data type
        foreach (MonitoredValue value in type == MonitoredValueType.Attribute ? MonitoredAttributes : MonitoredPVars) {
            if (value.Offset == offset) {
                if (value.DataType == MonitoredValueDataType.Number) {
                    oldV = BitConverter.ToUInt32(oldValue);
                    newV = BitConverter.ToUInt32(newValue);
                } else {
                    oldV = BitConverter.ToSingle(oldValue);
                    newV = BitConverter.ToSingle(newValue);
                }
            }
        }
        
        if (oldV == null || newV == null) {
            throw new Exception("Failed to find data type for monitored value for offset " + offset);
        }
        
        if (type == MonitoredValueType.Attribute) {
            CallLuaFunction("OnAttributeChange", LuaEntity(), player, offset, oldV, newV);
        } else {
            CallLuaFunction("OnPVarChange", LuaEntity(), player, offset, oldV, newV);
        }
    }
    
    public Universe Universe() {
        Entity parent = Parent();
        
        while (!(parent is Universe)) {
            if (parent == null) {
                return null;
            }
            
            parent = parent.Parent();
        }

        if (parent is Universe universe) {
            return universe;
        }
        
        return null;
    }

    public void AddCollider(Moby moby, uint collisionFlags = 0)
    {
        if (!_colliders.ContainsKey(moby))
        {
            _colliders.Add(moby, COLLIDE_TICKS);

            OnCollision(moby);
        } else
        {
            _colliders[moby] = COLLIDE_TICKS;
        }
    }

    public override void OnTick(TickNotification notification) {
        base.OnTick(notification);
        
        // Collision debouncing
        Moby[] keys = new Moby[_colliders.Keys.Count];
        _colliders.Keys.CopyTo(keys, 0);

        foreach(var key in keys) {
            _colliders[key] -= 1;

            if (_colliders[key] <= 0) {
                _colliders.Remove(key);
            } else {
                OnCollision(key);
            }
        }
    }


    public void OnPreTick(PreTickNotification notification) {
        if (Game.Shared().Ticks() % 60 == 0) {
            // Force sending updates every 60 ticks
            HasChanged = true;
        }
        else {
            ResetChanged();
        }
    }

    public float DistanceTo(object mobyObject) {
        Moby moby;
        
        if (mobyObject is LuaTable) {
            moby = (Moby)(((LuaTable)mobyObject)["_internalEntity"]);
        }
        else if (mobyObject is Moby) {
            moby = (Moby)mobyObject;
        }
        else {
            throw new Exception("Invalid object type for mobyObject in DistanceTo");
        }
        
        float xDiff = x - moby.x;
        float yDiff = y - moby.y;
        float zDiff = z - moby.z;

        return (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
    }
    
    public bool IsWithinCube(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        // Ensure x1, y1, z1 is the min point and x2, y2, z2 is the max point
        float minX = Math.Min(x1, x2);
        float minY = Math.Min(y1, y2);
        float minZ = Math.Min(z1, z2);
        float maxX = Math.Max(x1, x2);
        float maxY = Math.Max(y1, y2);
        float maxZ = Math.Max(z1, z2);

        // Check if this Moby's position is within the cube defined by the two points
        return minX <= x && x <= maxX &&
               minY <= y && y <= maxY &&
               minZ <= z && z <= maxZ;
    }

    public virtual void OnCollision(Moby collidee) {
        CallLuaFunction("OnCollision", LuaEntity(), collidee.LuaEntity());
    }

    public virtual void OnHit(Moby attacker) {
        CallLuaFunction("OnHit", LuaEntity(), attacker.LuaEntity());
    }

    public virtual void OnAttack(Moby attacked, ushort sourceOClass, float damage) {
        CallLuaFunction("OnAttack", LuaEntity(), attacked.LuaEntity(), sourceOClass, damage);
    }
}
