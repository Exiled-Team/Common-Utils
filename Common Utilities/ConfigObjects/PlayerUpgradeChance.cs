namespace Common_Utilities.ConfigObjects
{
    using PlayerRoles;

    public class PlayerUpgradeChance
    {
        public RoleTypeId Original { get; set; }

        public string New { get; set; } = RoleTypeId.Spectator.ToString();

        public double Chance { get; set; }

        public bool KeepInventory { get; set; } = true;

        public bool KeepHealth { get; set; } = true;

        public void Deconstruct(out RoleTypeId old, out string newRole, out double i, out bool keepInventory, out bool keepHealth)
        {
            old = Original;
            newRole = New;
            i = Chance;
            keepInventory = KeepInventory;
            keepHealth = KeepHealth;
        }
    }
}