using System;
namespace Lawrence
{
	public class Moby
	{
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

        public Moby(Client parent = null)
		{
            this.parent = parent;
		}
	}
}
