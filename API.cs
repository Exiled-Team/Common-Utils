namespace Common_Utilities
{
    using System.Collections.Generic;

    public static class API
    {
        public static List<ItemType> GetStartItems(RoleType role) => Plugin.Singleton.Handlers.StartItems(role);

        public static float GetHealthOnKill(RoleType role) => Plugin.Singleton.Config.HealOnKill.ContainsKey(role) ? Plugin.Singleton.Config.HealOnKill[role] : 0f;
    }
}