namespace Common_Utilities.Patches
{
    using System.Collections.Generic;
    using System.Linq;

    using Common_Utilities.ConfigObjects;
    using Common_Utilities.Configs;
    using Exiled.API.Features;
    using HarmonyLib;
    using InventorySystem;
    using PlayerRoles;

    [HarmonyPatch(typeof(InventoryItemProvider), nameof(InventoryItemProvider.ServerGrantLoadout))]
    public class ServerGrantLoadoutPatches
    {
        public static bool Prefix(ReferenceHub target, RoleTypeId roleTypeId, bool resetInventory = true)
        {
            if (Plugin.Instance.Config.StartingInventories == null || !Plugin.Instance.Config.StartingInventories.TryGetValue(roleTypeId, out RoleInventory startingInventories) || !Player.TryGet(target, out Player player))
                return true;

            if (resetInventory)
                player.ClearInventory();

            player.AddItem(Plugin.Instance.playerHandlers.StartItems(roleTypeId, player));

            if (startingInventories.Ammo != null && startingInventories.Ammo.Count > 0)
            {
                IEnumerable<StartingAmmo> startingAmmo = startingInventories.Ammo.Where(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup) && userGroup == player.Group));
                if (startingAmmo.Any())
                {
                    player.Ammo.Clear();

                    foreach ((ItemType type, ushort amount, string group) in startingAmmo)
                    {
                        player.Ammo.Add(type, amount);
                    }
                }
            }

            return false;
        }
    }
}
