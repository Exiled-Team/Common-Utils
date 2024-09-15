namespace Common_Utilities.ConfigObjects
{
    using System.ComponentModel;

    using PlayerRoles;

    /// <summary>
    /// Represents the chance of upgrading a player's role, with options to keep their inventory and health.
    /// </summary>
    public class PlayerUpgradeChance
    {
        /// <summary>
        /// Gets or sets the player's original role before the upgrade.
        /// </summary>
        [Description("The original role of the player before the upgrade.")]
        public object OriginalRole { get; set; }

        /// <summary>
        /// Gets or sets the player's new role after the upgrade.
        /// Defaults to Spectator role.
        /// </summary>
        [Description("The new role of the player after the upgrade. Defaults to Spectator.")]
        public object NewRole { get; set; } = RoleTypeId.Spectator.ToString();

        /// <summary>
        /// Gets or sets the chance of upgrading the player's role.
        /// </summary>
        [Description("The probability of upgrading the player's role.")]
        public double Chance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player's inventory should be kept after the upgrade.
        /// Defaults to true.
        /// </summary>
        [Description(
            "Indicates whether the player's inventory should be retained after the upgrade. Defaults to true.")]
        public bool KeepInventory { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the player's health should be kept after the upgrade.
        /// Defaults to true.
        /// </summary>
        [Description("Indicates whether the player's health should be retained after the upgrade. Defaults to true.")]
        public bool KeepHealth { get; set; } = true;

        /// <summary>
        /// Deconstructs the object into its original role, new role, upgrade chance, inventory retention, and health retention status.
        /// </summary>
        /// <param name="old">The original role of the player.</param>
        /// <param name="newRole">The new role of the player.</param>
        /// <param name="i">The upgrade chance.</param>
        /// <param name="keepInventory">Whether the inventory is retained.</param>
        /// <param name="keepHealth">Whether the health is retained.</param>
        public void Deconstruct(out object old, out object newRole, out double i, out bool keepInventory, out bool keepHealth)
        {
            old = OriginalRole;
            newRole = NewRole;
            i = Chance;
            keepInventory = KeepInventory;
            keepHealth = KeepHealth;
        }
    }
}