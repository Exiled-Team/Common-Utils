using Exiled.CustomItems.API.Features;

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

        public void OnPlayerVerified(VerifiedEventArgs ev)
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

            if (plugin.Config.CustomInventories.ContainsKey(ev.NewRole))
                Timing.CallDelayed(1.5f, () => HandleCustomItems(ev.Player));
        }

        private void HandleCustomItems(Player player)
        {
            foreach (KeyValuePair<string, List<Tuple<CustomItem, int>>> itemChanceBig in plugin.Config.CustomInventories[player.Role])
            {
                foreach ((CustomItem item, int chance) in itemChanceBig.Value)
                {
                    if (plugin.Gen.Next(100) > chance) 
                        continue;
                    item.Give(player);

                    break;
                }
            }
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (!plugin.Config.HealOnKill.ContainsKey(ev.Killer.Role)) 
                return;
            
            if (ev.Killer.Health + plugin.Config.HealOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)
                ev.Killer.Health += plugin.Config.HealOnKill[ev.Killer.Role];
            else
                ev.Killer.Health = ev.Killer.MaxHealth;
        }

        public List<ItemType> StartItems(RoleType role)
        {
            List<ItemType> items = new List<ItemType>();
            
            if (plugin.Config.Inventories[role] == default)
                return items;
            
            foreach (KeyValuePair<string, List<Tuple<ItemType, int>>> itemChanceBig in plugin.Config.Inventories[role])
            {
                foreach ((ItemType item, int chance) in itemChanceBig.Value)
                {
                    if (plugin.Gen.Next(100) > chance) 
                        continue;
                    
                    items.Add(item);
                    break;
                }
            }

            return items;
        }

        private string FormatJoinMessage(Player player) => 
            string.IsNullOrEmpty(plugin.Config.JoinMessage) ? string.Empty : plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");

        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker.Team == Team.SCP)
            {
                if (plugin.Config.ScpDmgMult.ContainsKey(ev.Attacker.Role))
                    ev.Amount *= plugin.Config.ScpDmgMult[ev.Attacker.Role];
            }
            else if (plugin.Config.WepDmgMult.ContainsKey((ItemType) ev.Tool))
                ev.Amount *= plugin.Config.WepDmgMult[(ItemType) ev.Tool];
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.IsCuffed && plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (ev.Player.IsCuffed && plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;
        }
    }
}