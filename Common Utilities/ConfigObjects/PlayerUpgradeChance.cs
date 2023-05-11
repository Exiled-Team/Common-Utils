using PlayerRoles;

namespace Common_Utilities.ConfigObjects
{
    public class PlayerUpgradeChance
    {
        public RoleTypeId Original { get; set; }
        public RoleTypeId New { get; set; }
        public double Chance { get; set; }
        public RoleSpawnFlags SpawnFlags { get; set; }

        public void Deconstruct(out RoleTypeId old, out RoleTypeId newRole, out double i, out RoleSpawnFlags spawnFlags)
        {
            old = Original;
            newRole = New;
            i = Chance;
            spawnFlags = SpawnFlags;
        }
    }
}