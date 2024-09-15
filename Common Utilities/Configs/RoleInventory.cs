namespace Common_Utilities.Configs
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using ConfigObjects;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a player's role inventory with item slots and starting ammunition.
    /// </summary>
    public class RoleInventory
    {
        /// <summary>
        /// Gets the number of used slots in the inventory.
        /// A slot is considered used if it is not null and not empty.
        /// </summary>
        [YamlIgnore]
        public int UsedSlots
        {
            get
            {
                int i = 0;
                if (Slot1 is not null && !Slot1.IsEmpty())
                    i++;
                if (Slot2 is not null && !Slot2.IsEmpty())
                    i++;
                if (Slot3 is not null && !Slot3.IsEmpty())
                    i++;
                if (Slot4 is not null && !Slot4.IsEmpty())
                    i++;
                if (Slot5 is not null && !Slot5.IsEmpty())
                    i++;
                if (Slot6 is not null && !Slot6.IsEmpty())
                    i++;
                if (Slot7 is not null && !Slot7.IsEmpty())
                    i++;
                if (Slot8 is not null && !Slot8.IsEmpty())
                    i++;
                return i;
            }
        }

        /// <summary>
        /// Gets or sets the list of items in slot 1.
        /// </summary>
        [Description("The list of items in slot 1.")]
        public List<ItemChance> Slot1 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 2.
        /// </summary>
        [Description("The list of items in slot 2.")]
        public List<ItemChance> Slot2 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 3.
        /// </summary>
        [Description("The list of items in slot 3.")]
        public List<ItemChance> Slot3 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 4.
        /// </summary>
        [Description("The list of items in slot 4.")]
        public List<ItemChance> Slot4 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 5.
        /// </summary>
        [Description("The list of items in slot 5.")]
        public List<ItemChance> Slot5 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 6.
        /// </summary>
        [Description("The list of items in slot 6.")]
        public List<ItemChance> Slot6 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 7.
        /// </summary>
        [Description("The list of items in slot 7.")]
        public List<ItemChance> Slot7 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of items in slot 8.
        /// </summary>
        [Description("The list of items in slot 8.")]
        public List<ItemChance> Slot8 { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of starting ammunition.
        /// </summary>
        [Description("The list of starting ammunition.")]
        public List<StartingAmmo> Ammo { get; set; } = new();

        /// <summary>
        /// Provides access to the item slots by index.
        /// Index 0 corresponds to Slot1, 1 to Slot2, and so on.
        /// </summary>
        /// <param name="i">The index of the slot (0 to 7).</param>
        /// <returns>The list of <see cref="ItemChance"/> for the specified slot.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        public IEnumerable<ItemChance> this[int i] => i switch
        {
            0 => Slot1,
            1 => Slot2,
            2 => Slot3,
            3 => Slot4,
            4 => Slot5,
            5 => Slot6,
            6 => Slot7,
            7 => Slot8,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}