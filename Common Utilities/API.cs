using System;

namespace Common_Utilities
{
    using System.Collections.Generic;

    using Exiled.API.Enums;
    
    public static class API
    {
        public static List<ItemType> GetStartItems(RoleType role, UserGroup userGroup = default) => Plugin.Singleton.PlayerHandlers.StartItems(role, userGroup);

        public static float GetHealthOnKill(RoleType role) => Plugin.Singleton.Config.HealOnKill.ContainsKey(role) ? Plugin.Singleton.Config.HealOnKill[role] : 0f;
    }
}