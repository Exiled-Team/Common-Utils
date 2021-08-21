using System.Collections.Generic;
using Exiled.API.Features;
using Scp914;

namespace Common_Utilities.EventHandlers
{
    using System;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs;

    public class MapHandlers
    {
        private readonly Plugin plugin;
        public MapHandlers(Plugin plugin) => this.plugin = plugin;
        
        public void OnScp914UpgradingItem(UpgradingItemEventArgs ev)
        {
            if (plugin.Config.Scp914Configs.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[
                        ev.KnobSetting])
                    {
                        if (sourceItem != ev.Item.Type)
                            continue;

                        if (plugin.Gen.Next(100) <= chance)
                        {
                            UpgradeItem(ev.Item, destinationItem);
                            break;
                        }
                    }
            }
        }

        public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (plugin.Config.Scp914Configs.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[
                    ev.KnobSetting])
                {
                    if (sourceItem != ev.Item.Type)
                        continue;

                    if (plugin.Gen.Next(100) <= chance)
                    {
                        ev.Player.RemoveItem(ev.Item);
                        ev.Player.AddItem(destinationItem);
                    }
                }
            }
        }

        public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (plugin.Config.Scp914RoleChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((RoleType sourceRole, RoleType destinationRole, int chance) in plugin.Config.Scp914RoleChanges[
                    ev.KnobSetting])
                {
                    if (sourceRole != ev.Player.Role)
                        continue;

                    if (plugin.Gen.Next(100) <= chance)
                    {
                        ev.Player.SetRole(destinationRole, SpawnReason.ForceClass, true);
                        ev.Player.Position = Exiled.API.Features.Scp914.OutputBooth.position;
                    }
                }
            }
        }

        internal void UpgradeItem(Pickup oldItem, ItemType newItem)
        {
            oldItem.Destroy();
            new Item(newItem).Spawn(Exiled.API.Features.Scp914.OutputBooth.position, default);
        }
    }
}