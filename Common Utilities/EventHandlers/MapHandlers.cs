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
                    foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[ev.KnobSetting])
                    {
                        if (sourceItem == item.ItemId)
                        {
                            if (plugin.Gen.Next(100) <= chance)
                                UpgradeItem(item, destinationItem);
                        }
                    }
                }
                foreach (Player pla in ev.Players)
                {
                    foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[ev.KnobSetting])
                    {
                        if (pla.CurrentItem.id == sourceItem)
                        {
                            if (plugin.Gen.Next(100) <= chance)
                            {
                                pla.ReferenceHub.inventory.NetworkcurItem = destinationItem;
                            }
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
