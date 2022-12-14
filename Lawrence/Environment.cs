using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lawrence
{
	public class Environment
	{
		static Environment SharedEnvironment;

        public List<Moby> mobys = new List<Moby>();

        public Environment()
		{

		}

		// Get shared environment singleton
		public static Environment Shared()
		{
			if (Environment.SharedEnvironment == null)
			{
				Environment.SharedEnvironment = new Environment();
			}

			return Environment.SharedEnvironment;
		}

		public List<Moby> GetMobys()
		{
			return mobys;
		}


        public Moby GetMoby(uint uuid)
        {
            if (uuid > mobys.Count)
            {
                return null;
            }

            return mobys[(int)uuid - 1];
        }


        public Moby NewMoby(Client parent = null)
        {
            Moby moby = new Moby(parent);
            // Moby UUIDs start at 1, not 0.
            moby.UUID = (ushort)(mobys.Count + 1);

            foreach (Moby m in mobys)
            {
                if (m.Deleted())
                {
                    moby.UUID = m.UUID;
                }
            }

            if (moby.UUID-1 >= mobys.Count)
            {
                mobys.Add(moby);
            } else
            {
                mobys[moby.UUID-1] = moby;
            }
            

            if (parent != null)
            {
                Console.WriteLine($"New moby (uid: {moby.UUID}). Parent: {parent?.ID}");
            }
            else
            {
                Console.WriteLine($"New moby (uid: {moby.UUID})");
            }

            return moby;
        }

        public void DeleteMobys(Func<Moby, bool> value)
        {
            Moby[] deleteMobys = mobys.Where(value).ToArray();

            foreach (var moby in deleteMobys)
            {
                Console.WriteLine($"Requesting clients delete moby {moby.UUID}");
                moby.Delete();
            }
        }

        public void Tick()
        {
            foreach (var moby in mobys)
            {
                if (!moby.active)
                {
                    continue;
                }

                moby.Tick();

                List<Client> ignoring = null;
                if (moby.parent != null)
                {
                    ignoring = new List<Client> { moby.parent };
                }

                Lawrence.DistributePacket(Packet.MakeMobyUpdatePacket(moby), moby.level, ignoring);
            }
        }
    }
}

