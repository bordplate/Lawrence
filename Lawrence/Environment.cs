using System;
using System.Collections.Generic;

namespace Lawrence
{
	public class Environment
	{
		static Environment SharedEnvironment;

        List<Moby> mobys = new List<Moby>();

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
            return mobys[(int)uuid - 1];
        }


        public Moby NewMoby(Client parent = null)
        {
            Moby moby = new Moby(parent);
            moby.UUID = (ushort)(mobys.Count + 1);
            mobys.Add(moby);

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
    }
}

