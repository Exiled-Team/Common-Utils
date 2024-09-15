namespace Common_Utilities.ConfigObjects
{
    using PlayerRoles;

    public class PlayerUpgradeChance
    {
        public object OriginalRole { get; set; }

        public object NewRole { get; set; } = RoleTypeId.Spectator.ToString();

        public double Chance { get; set; }

        public bool KeepInventory { get; set; } = true;

        public bool KeepHealth { get; set; } = true;

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