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

        public int oClass = 0;

        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
        public float rotX = 0.0f;
        public float rotY = 0.0f;
        public float rotZ = 0.0f;
        public float scale = 1.0f;
        public float alpha = 1.0f;

        public int animationID = 0;
        public int animationDuration = 0;

        public ushort modeBits = 0x10 | 0x20 | 0x400 | 0x4000;

        public bool mpUpdateFunc = true;
        public bool collision = true;

        Dictionary<Moby, ushort> colliders = new Dictionary<Moby, ushort>();

        public Moby(LuaTable luaTable = null) : base(luaTable)
        {
            
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
