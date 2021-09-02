namespace Common_Utilities.ConfigObjects
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using UnityEngine;

    public class Scp914TeleportChance
    {
        public RoomType Room { get; set; }
        public Vector3 Offset { get; set; } = Vector3.zero;
        public int Chance { get; set; }

        public void Deconstruct(out RoomType room, out Vector3 offset, out int chance)
        {
            room = Room;
            offset = Offset;
            chance = Chance;
        }
    }
}