using PlayerRoles;

namespace Common_Utilities.ConfigObjects
{
    public class PlayerUpgradeChance
    {
        public RoleTypeId Original { get; set; }
        public RoleTypeId New { get; set; }
        public double Chance { get; set; }
        public bool KeepInventory { get; set; } = true;

        public void Deconstruct(out RoleTypeId old, out RoleTypeId newRole, out double i, out bool keepInventory)
        {
            old = Original;
            newRole = New;
            i = Chance;
            keepInventory = KeepInventory;
        }
    }
}