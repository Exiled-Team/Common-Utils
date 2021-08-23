namespace Common_Utilities.Structs
{
    public class PlayerUpgradeChance
    {
        public RoleType Original { get; set; }
        public RoleType New { get; set; }
        public int Chance { get; set; }

        public void Deconstruct(out RoleType old, out RoleType newRole, out int i)
        {
            old = Original;
            newRole = New;
            i = Chance;
        }
    }
}