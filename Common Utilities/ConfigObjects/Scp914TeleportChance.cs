namespace Common_Utilities.ConfigObjects
{
    using Exiled.API.Enums;
    using UnityEngine;

    public class Scp914TeleportChance
    {
        public ZoneType Zone { get; set; } = ZoneType.Unspecified;
        public RoomType Room { get; set; }
        public Vector3 Offset { get; set; } = Vector3.zero;
        public double Chance { get; set; }
        public float Damage { get; set; } = 0f;

        public void Deconstruct(out RoomType room, out Vector3 offset, out double chance, out float damage, out ZoneType zone)
        {
            room = Room;
            offset = Offset;
            chance = Chance;
            damage = Damage;
            zone = Zone;
        }
    }
}