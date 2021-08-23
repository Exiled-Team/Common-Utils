namespace Common_Utilities.Structs
{
    public class PlayerUpgradeChance
    {
        public RoleType OldRole { get; set; }
        public RoleType NewRole { get; set; }
        public int Chance { get; set; }

        public void Deconstruct(out RoleType old, out RoleType newRole, out int i)
        {
            old = OldRole;
            newRole = NewRole;
            i = Chance;
        }
    }
}