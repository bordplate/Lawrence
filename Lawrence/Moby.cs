using System;
using System.Collections.Generic;

namespace Lawrence
{
	public class Moby
	{
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

        public ushort level = 0;
        public byte team = 0;

        public bool onlyVisibleToTeam = false;
        public bool active = false;
        public bool mpUpdateFunc = true;
        public bool collision = true;

        public Client parent;

        bool deleted = false;

        Dictionary<ushort, ushort> colliders = new Dictionary<ushort, ushort>();

        public Moby(Client parent = null)
        {
            this.parent = parent;
        }

        public bool Deleted()
        {
            return deleted;
        }

        public void Delete()
        {
            // FIXME: Make it so this doesn't delete all teams' mobys if it shouldn't
            Lawrence.DistributePacket(Packet.MakeDeleteMobyPacket(UUID));

            deleted = true;
            active = false;
            level = 0;
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

                Moby victim = Environment.Shared().GetMoby(uuid);
                Environment.Shared().OnCollision(this, victim, collisionFlags);
            } else
            {
                colliders[uuid] = Moby.COLLIDE_TICKS;
            }
        }

        public void Tick()
        {
            // Collision debouncing
            ushort[] keys = new ushort[colliders.Keys.Count];
            colliders.Keys.CopyTo(keys, 0);

            foreach(var key in keys)
            {
                colliders[key] -= 1;

                if (colliders[key] <= 0)
                {
                    colliders.Remove(key);
                    Console.WriteLine($"Collision ended between {this.UUID} and {key}");
                    // Notify stuff that collision has ended
                    Environment.Shared().OnCollisionEnd(this, Environment.Shared().GetMoby(key));
                }
            }
        }
	}
}
