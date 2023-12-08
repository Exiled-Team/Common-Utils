namespace Common_Utilities;

using Exiled.API.Features;
using System.Collections.Generic;

using PlayerRoles;

public static class API
{
    public static List<ItemType> GetStartItems(RoleTypeId role) => Main.Instance.PlayerHandlers.StartItems(role);

    public static List<ItemType> GetStartItems(RoleTypeId role, Player player) => Main.Instance.PlayerHandlers.StartItems(role, player);

    public static float GetHealthOnKill(RoleTypeId role) => Main.Instance.Config.HealthOnKill?.ContainsKey(role) ?? false ? Main.Instance.Config.HealthOnKill[role] : 0f;
}