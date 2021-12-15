using System;

namespace Necromancy.Server.Model
{
    public class MapMeshCapture 
    {
        public MapMeshCapture()
        {
            created = DateTime.Now;
        }

        public uint characterInstanceId { get; set; }
        public int mapId { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public byte heading { get; set; }
        public DateTime created { get; set; }
        public float verticalMovementSpeedMultiplier { get; set; }

    }
}
