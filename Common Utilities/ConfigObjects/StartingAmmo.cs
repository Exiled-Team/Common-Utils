namespace Common_Utilities.ConfigObjects
{
    using System.ComponentModel;

    /// <summary>
    /// Represents the starting ammo for a player.
    /// </summary>
    public class StartingAmmo
    {
        /// <summary>
        /// Gets or sets the type of item.
        /// </summary>
        [Description("The type of item for starting ammo.")]
        public ItemType Type { get; set; }

        /// <summary>
        /// Gets or sets the amount of starting ammo.
        /// </summary>
        [Description("The amount of starting ammo.")]
        public ushort Amount { get; set; }

        /// <summary>
        /// Gets or sets the group name for the starting ammo.
        /// Defaults to "none".
        /// </summary>
        [Description("The group name for the starting ammo. Defaults to 'none'.")]
        public string Group { get; set; } = "none";

        /// <summary>
        /// Deconstructs the object into its item type, amount, and group.
        /// </summary>
        /// <param name="type">The type of item.</param>
        /// <param name="limit">The amount of starting ammo.</param>
        /// <param name="group">The group name for the starting ammo.</param>
        public void Deconstruct(out ItemType type, out ushort limit, out string group)
        {
            type = Type;
            limit = Amount;
            group = Group;
        }
    }
}
