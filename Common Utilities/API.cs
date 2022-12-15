using PlayerRoles;

namespace Common_Utilities
{
    using System.Collections.Generic;

    public static class API
    {
        public static List<ItemType> GetStartItems(RoleTypeId role) => Plugin.Singleton.PlayerHandlers.StartItems(role);

        public static float GetHealthOnKill(RoleTypeId role) => Plugin.Singleton.Config.HealthOnKill.ContainsKey(role) ? Plugin.Singleton.Config.HealthOnKill[role] : 0f;
    }
}