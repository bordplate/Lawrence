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

        public ushort UUID = 0;

        public int oClass = 0;

        public byte state = 0;

        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
        public float rot = 0.0f;

        public int animationID = 0;
        public int animationDuration = 0;

        public bool active = false;
        public bool mpUpdateFunc = true;
        public bool collision = true;

        public Client parent;

        bool deleted = false;

        Dictionary<ushort, ushort> colliders = new Dictionary<ushort, ushort>();

        public Moby(LuaTable luaTable = null) : base(luaTable)
        {
            
        }

        public Level Level() {
            return _level;
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

        public bool Deleted()
        {
            return deleted;
        }

        public void Delete()
        {
            deleted = true;
            active = false;
            parent = null;
            oClass = 0;
            animationID = 0;
        }

        public void Damage(int damage)
        {
            if (parent != null)
            {
                parent.SendPacket(Packet.MakeDamagePacket(1));
            }
        }

        public void AddCollider(ushort uuid, uint collisionFlags)
        {
            if (!colliders.ContainsKey(uuid))
            {
                colliders.Add(uuid, Moby.COLLIDE_TICKS);
                Console.WriteLine($"New collision between {this.UUID} and {uuid}");
                // Notify stuff that new collision has happened

                // TODO: Notify other 
            } else
            {
                colliders[uuid] = Moby.COLLIDE_TICKS;
            }
        }

        public override void OnTick(TickNotification notification) {
            base.OnTick(notification);
            
            // Collision debouncing
            ushort[] keys = new ushort[colliders.Keys.Count];
            colliders.Keys.CopyTo(keys, 0);

            foreach(var key in keys) {
                colliders[key] -= 1;

                if (colliders[key] <= 0) {
                    colliders.Remove(key);
                    Console.WriteLine($"Collision ended between {this.UUID} and {key}");
                    // Notify stuff that collision has ended
                    // TODO: Notify other
                }
            }
        }
	}
}
