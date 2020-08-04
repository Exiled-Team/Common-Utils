using System.Collections.Generic;
using Exiled.API.Features;
using Scp914;

namespace Common_Utilities.EventHandlers
{
    using System;
    using Exiled.API.Extensions;
    using Exiled.Events.EventArgs;
    
    public class MapHandlers
    {
        private readonly Plugin plugin;
        public MapHandlers(Plugin plugin) => this.plugin = plugin;
        
        public void OnScp914UpgradingItems(UpgradingItemsEventArgs ev)
        {
            if (plugin.Config.Scp914Configs.ContainsKey(ev.KnobSetting))
            {
                foreach (Pickup item in ev.Items)
                {
                    foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[
                        ev.KnobSetting])
                    {
                        if (sourceItem != item.ItemId)
                            continue;

                        if (plugin.Gen.Next(100) <= chance)
                        {
                            UpgradeItem(item, destinationItem);
                            break;
                        }
                    }
                }

                if (Exiled.API.Features.Scp914.ConfigMode.Value.HasFlagFast(Scp914Mode.Inventory))
                {
                    foreach (Player player in ev.Players)
                    {
                        if (Exiled.API.Features.Scp914.ConfigMode.Value.HasFlagFast(Scp914Mode.Held))
                        {
                            ItemType original = player.CurrentItem.id;

                            foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[ev.KnobSetting])
                            {
                                if (sourceItem != original)
                                    continue;

                                if (plugin.Gen.Next(100) <= chance)
                                {
                                    player.RemoveItem(player.CurrentItem);
                                    player.AddItem(destinationItem);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in player.Inventory.items)
                            {
                                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[ev.KnobSetting])
                                {
                                    if (sourceItem != item.id)
                                        continue;

                                    if (plugin.Gen.Next(100) <= chance)
                                    {
                                        player.RemoveItem(item);
                                        player.AddItem(destinationItem);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (plugin.Config.Scp914RoleChanges.ContainsKey(ev.KnobSetting))
            {
                foreach (Player player in ev.Players)
                {
                    foreach ((RoleType originalRole, RoleType newRole, int chance) in plugin.Config.Scp914RoleChanges[ev.KnobSetting])
                    {
                        if (player.Role != originalRole)
                            continue;

                        if (plugin.Gen.Next(100) <= chance)
                        {
                            player.SetRole(newRole, true);
                            break;
                        }
                    }
                }
            }
        }

        internal void UpgradeItem(Pickup oldItem, ItemType newItem)
        {
            oldItem.Delete();
            newItem.Spawn(default, Exiled.API.Features.Scp914.OutputBooth.position);
        }
    }
}