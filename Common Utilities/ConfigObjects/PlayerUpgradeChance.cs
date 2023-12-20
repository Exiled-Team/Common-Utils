namespace Common_Utilities.ConfigObjects
{
    using PlayerRoles;

    public class PlayerUpgradeChance
    {
        public RoleTypeId Original { get; set; }

        public string New { get; set; } = RoleTypeId.Spectator.ToString();

        public double Chance { get; set; }

        public bool KeepInventory { get; set; } = true;

        public void Deconstruct(out RoleTypeId old, out string newRole, out double i, out bool keepInventory)
        {
            old = Original;
            newRole = New;
            i = Chance;
            keepInventory = KeepInventory;
        }
    }
}