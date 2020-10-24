namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using MEC;
    using Player = Exiled.API.Features.Player;
    
    public class PlayerHandlers
    {
        private readonly Plugin plugin;
        public PlayerHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnPlayerJoined(JoinedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(plugin.Config.JoinMessageDuration, message);
        }
        
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (plugin.Config.Inventories.ContainsKey(ev.NewRole))
            {
                ev.Items.Clear();
                ev.Items.AddRange(StartItems(ev.NewRole));
            }

            if (plugin.Config.Health.ContainsKey(ev.NewRole))
                Timing.CallDelayed(1.5f, () =>
                {
                    ev.Player.Health = plugin.Config.Health[ev.NewRole];
                    ev.Player.MaxHealth = plugin.Config.Health[ev.NewRole];
                });
        }
        
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (plugin.Config.HealOnKill.ContainsKey(ev.Killer.Role))
            {
                if (ev.Killer.Health + plugin.Config.HealOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)
                    ev.Killer.Health += plugin.Config.HealOnKill[ev.Killer.Role];
                else
                    ev.Killer.Health = ev.Killer.MaxHealth;
            }
        }

        public List<ItemType> StartItems(RoleType role)
        {
            List<ItemType> items = new List<ItemType>();
            
            if (plugin.Config.Inventories[role] == default)
                return items;
            
            foreach (KeyValuePair<string, List<Tuple<ItemType, int>>> itemChanceBig in plugin.Config.Inventories[role])
            {
                foreach (Tuple<ItemType, int> itemChance in itemChanceBig.Value)
                {
                    if (plugin.Gen.Next(100) <= itemChance.Item2)
                    {
                        items.Add(itemChance.Item1);
                        break;
                    }
                }
            }

            return items;
        }

        private string FormatJoinMessage(Player player) => 
            string.IsNullOrEmpty(plugin.Config.JoinMessage) ? string.Empty : plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");
    }
}