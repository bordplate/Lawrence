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

        public int animationID;

        public ushort level;

        public bool active = false;

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
            Lawrence.DistributePacket(Packet.MakeDeleteMobyPacket(UUID));

            deleted = true;
            active = false;
            level = 0;
            parent = null;
            oClass = 0;
            animationID = 0;
        }

        public void AddCollider(ushort uuid, uint collisionFlags)
        {
            if (!colliders.ContainsKey(uuid))
            {
                colliders.Add(uuid, Moby.COLLIDE_TICKS);
                Console.WriteLine($"New collision between {this.UUID} and {uuid}");
                // Notify stuff that new collision has happened

                if (collisionFlags > 0)
                {
                    // We hit someone, so we get their moby and tell them they're hurt
                    Moby victim = Environment.Shared().GetMoby(uuid);
                    if (victim != null && victim.parent != null)
                    {
                        Console.WriteLine($"Moby {UUID} hit {uuid} for 1 damage.");
                        victim.parent.SendPacket(Packet.MakeDamagePacket(1));
                    } else
                    {
                        Console.WriteLine($"Moby {UUID} hit null-moby {uuid}");
                    }
                }
            } else
            {
                colliders[uuid] = Moby.COLLIDE_TICKS;
            }
        }

        public void Tick()
        {
            // Collision debouncing
            foreach(var key in colliders.Keys)
            {
                colliders[key] -= 1;

                if (colliders[key] <= 0)
                {
                    colliders.Remove(key);
                    Console.WriteLine($"Collision ended between {this.UUID} and {key}");
                    // Notify stuff that collision has ended
                }
            }
        }
	}
}
