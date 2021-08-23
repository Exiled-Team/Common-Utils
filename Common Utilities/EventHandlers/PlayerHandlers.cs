using Exiled.CustomItems.API.Features;

namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using Common_Utilities.Structs;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.CustomItems;
    using Exiled.Events.EventArgs;
    using MEC;
    using Player = Exiled.API.Features.Player;
    
    public class PlayerHandlers
    {
        private readonly Plugin _plugin;
        public PlayerHandlers(Plugin plugin) => this._plugin = plugin;

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(_plugin.Config.JoinMessageDuration, message);
        }
        
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (_plugin.Config.StartingInventories.ContainsKey(ev.NewRole))
            {
                ev.Items.Clear();
                ev.Items.AddRange(StartItems(ev.NewRole, ev.Player));
            }

            if (_plugin.Config.HealthValues.ContainsKey(ev.NewRole))
                Timing.CallDelayed(1.5f, () =>
                {
                    ev.Player.Health = _plugin.Config.HealthValues[ev.NewRole];
                    ev.Player.MaxHealth = _plugin.Config.HealthValues[ev.NewRole];
                });
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (!_plugin.Config.HealthOnKill.ContainsKey(ev.Killer.Role)) 
                return;
            
            if (ev.Killer.Health + _plugin.Config.HealthOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)
                ev.Killer.Health += _plugin.Config.HealthOnKill[ev.Killer.Role];
            else
                ev.Killer.Health = ev.Killer.MaxHealth;
        }

        public List<ItemType> StartItems(RoleType role, Player player = null)
        {
            List<ItemType> items = new List<ItemType>();

            for (int i = 0; i < _plugin.Config.StartingInventories[role].UsedSlots; i++)
            {
                int r = _plugin.Gen.Next(100);
                foreach ((string item, int chance, string groupKey) in _plugin.Config.StartingInventories[role][i])
                {
                    if (player != null && !string.IsNullOrEmpty(groupKey) && groupKey != "none" && (!ServerStatic.PermissionsHandler._groups.TryGetValue(groupKey, out var group) || group != player.Group))
                        continue;
                    
                    if (r <= chance)
                    {
                        if (Enum.TryParse(item, true, out ItemType type))
                        {
                            items.Add(type);
                            break;
                        }
                        else if (CustomItem.TryGet(item, out CustomItem customItem))
                        {
                            if (player != null)
                                Timing.CallDelayed(0.5f, () => customItem.Give(player));
                            else
                                Log.Warn($"{nameof(StartItems)}: Tried to give {customItem.Name} to a null player.");
                            break;
                        }
                        else
                            Log.Warn($"{nameof(StartItems)}: {item} is not a valid ItemType or it is a CustomItem that is not installed! It is being skipper in inventory decisions.");
                    }
                }
            }

            return items;
        }

        private string FormatJoinMessage(Player player) => 
            string.IsNullOrEmpty(_plugin.Config.JoinMessage) ? string.Empty : _plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");

        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (_plugin.Config.RoleDamageMultipliers.ContainsKey(ev.Attacker.Role))
                ev.Amount *= _plugin.Config.RoleDamageMultipliers[ev.Attacker.Role];

            if (_plugin.Config.WeaponDamageMultipliers.ContainsKey(ev.DamageType.Weapon))
                ev.Amount *= _plugin.Config.WeaponDamageMultipliers[ev.DamageType.Weapon];
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.IsCuffed && _plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (ev.Player.IsCuffed && _plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;
        }
    }
}