using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Configs;
using InventorySystem.Items;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_Utilities.EventHandlers;
using Common_Utilities.ConfigObjects;
using Common_Utilities.Configs;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(InventoryItemProvider), nameof(InventoryItemProvider.ServerGrantLoadout))]
    public class ServerGrantLoadoutPatches
    {
        public static bool Prefix(ReferenceHub target, RoleTypeId roleTypeId, bool resetInventory = true)
        {
            if (Plugin.Singleton.Config.StartingInventories == null || !Plugin.Singleton.Config.StartingInventories.TryGetValue(roleTypeId, out RoleInventory startingInventories) || !Player.TryGet(target, out Player player))
                return true;

            if (resetInventory)
                player.ClearInventory();

            player.AddItem(Plugin.Singleton.PlayerHandlers.StartItems(roleTypeId, player));

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
