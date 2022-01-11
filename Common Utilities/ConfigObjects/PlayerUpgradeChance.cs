namespace Common_Utilities.ConfigObjects
{
    public class PlayerUpgradeChance
    {
        public RoleType Original { get; set; }
        public RoleType New { get; set; }
        public int Chance { get; set; }
        public bool KeepInventory { get; set; } = true;

        public void Deconstruct(out RoleType old, out RoleType newRole, out int i, out bool keepInventory)
        {
            old = Original;
            newRole = New;
            i = Chance;
            keepInventory = KeepInventory;
        }
    }
}