namespace Common_Utilities.ConfigObjects
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using Exiled.API.Enums;
    using UnityEngine;

    /// <summary>
    /// Represents the chance of teleportation within SCP-914.
    /// </summary>
    public class Scp914TeleportChance
    {
        /// <summary>
        /// Gets or sets the zone type for the teleportation.
        /// Defaults to Unspecified.
        /// </summary>
        [Description("The zone type for teleportation. Defaults to Unspecified.")]
        public ZoneType Zone { get; set; } = ZoneType.Unspecified;

        /// <summary>
        /// Gets or sets the list of room types to be ignored during teleportation.
        /// </summary>
        [Description("The list of room types to ignore during teleportation.")]
        public List<RoomType> IgnoredRooms { get; set; } = new List<RoomType>();

        /// <summary>
        /// Gets or sets the room type for teleportation.
        /// </summary>
        [Description("The room type for teleportation.")]
        public RoomType Room { get; set; }

        /// <summary>
        /// Gets or sets the offset for teleportation.
        /// Defaults to Vector3.zero.
        /// </summary>
        [Description("The offset for teleportation. Defaults to Vector3.zero.")]
        public Vector3 Offset { get; set; } = Vector3.zero;

        /// <summary>
        /// Gets or sets the chance of teleportation occurring.
        /// </summary>
        [Description("The probability of teleportation occurring.")]
        public double Chance { get; set; }

        /// <summary>
        /// Gets or sets the damage inflicted during teleportation.
        /// Defaults to 0f.
        /// </summary>
        [Description("The damage inflicted during teleportation. Defaults to 0f.")]
        public float Damage { get; set; } = 0f;

        /// <summary>
        /// Deconstructs the object into its room type, ignored rooms, offset, chance, damage, and zone.
        /// </summary>
        /// <param name="room">The room type for teleportation.</param>
        /// <param name="ignoredRooms">The list of room types to ignore during teleportation.</param>
        /// <param name="offset">The offset for teleportation.</param>
        /// <param name="chance">The probability of teleportation occurring.</param>
        /// <param name="damage">The damage inflicted during teleportation.</param>
        /// <param name="zone">The zone type for teleportation.</param>
        public void Deconstruct(out RoomType room, out List<RoomType> ignoredRooms, out Vector3 offset, out double chance, out float damage, out ZoneType zone)
        {
            room = Room;
            ignoredRooms = IgnoredRooms;
            offset = Offset;
            chance = Chance;
            damage = Damage;
            zone = Zone;
        }
    }
}