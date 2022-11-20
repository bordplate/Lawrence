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

        bool deleted = false;

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
	}
}
