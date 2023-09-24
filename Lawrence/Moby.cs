using System;
using System.Collections.Generic;
using NLua;

namespace Lawrence
{
	public class Moby : Entity
	{
        /// <summary>
        /// The current level this moby is on. Levels are tied to universes such that mobys (incl. users) on the same
        /// level in different universes won't be able to see or interact with each other. 
        /// </summary>
        protected Level _level;
        
        static ushort COLLIDE_TICKS = 10;
        
        private int _oClass = 0;
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
        public virtual int animationID { get => _animationID; set { if (_animationID != value) { _animationID = value; HasChanged = true; } } }
        
        private int _animationDuration = 0;
        public int animationDuration { get => _animationDuration; set { if (_animationDuration != value) { _animationDuration = value; HasChanged = true; } } }
        
        private ushort _modeBits = 0x10 | 0x20 | 0x400 | 0x1000 | 0x4000;
        public ushort modeBits { get => _modeBits; set { if (_modeBits != value) { _modeBits = value; HasChanged = true; } } }

        public bool HasChanged { get; protected set; } = false;

        public void ResetChanged()
        {
            HasChanged = false;
        }


        public bool mpUpdateFunc = true;
        public bool collision = true;

        Dictionary<Moby, ushort> colliders = new Dictionary<Moby, ushort>();

        public Moby(LuaTable luaTable = null) : base(luaTable) {
            Game.Shared().NotificationCenter().Subscribe<PreTickNotification>(OnPreTick);
        }

        public override void Delete() {
            base.Delete();
            
            Game.Shared().NotificationCenter().Unsubscribe<PreTickNotification>(OnPreTick);
        }
        
        public Level Level() {
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

        public void SetOClass(int oClass) {
            this.oClass = oClass;
        }

        public void SetPosition(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
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
            if (!colliders.ContainsKey(moby))
            {
                colliders.Add(moby, Moby.COLLIDE_TICKS);

                OnCollision(moby);
            } else
            {
                colliders[moby] = Moby.COLLIDE_TICKS;
            }
        }

        public override void OnTick(TickNotification notification) {
            base.OnTick(notification);
            
            // Collision debouncing
            Moby[] keys = new Moby[colliders.Keys.Count];
            colliders.Keys.CopyTo(keys, 0);

            foreach(var key in keys) {
                colliders[key] -= 1;

                if (colliders[key] <= 0) {
                    colliders.Remove(key);
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
            
            float xDiff = this.x - moby.x;
            float yDiff = this.y - moby.y;
            float zDiff = this.z - moby.z;

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
            CallLuaFunction("OnCollision", new object[] { LuaEntity(), collidee.LuaEntity() });
        }

        public virtual void OnHit(Moby attacker) {
            CallLuaFunction("OnHit", new object[] { LuaEntity(), attacker.LuaEntity() });
        }

        public virtual void OnAttack(Moby attacked) {
            CallLuaFunction("OnAttack", new object[] { LuaEntity(), attacked.LuaEntity() });
        }
	}
}
