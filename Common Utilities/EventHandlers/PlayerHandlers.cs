namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.CustomItems.API.Features;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.EventArgs.Player;
    using PlayerRoles;
    using UnityEngine;

    using Player = Exiled.API.Features.Player;

    public class PlayerHandlers
    {
        private readonly Plugin _plugin;
        public PlayerHandlers(Plugin plugin) => _plugin = plugin;

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(_plugin.Config.JoinMessageDuration, message);
        }
        
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn($"{nameof(OnChangingRole)}: Triggering player is null.");
                return;
            }

            if (_plugin.Config.StartingInventories != null && _plugin.Config.StartingInventories.ContainsKey(ev.NewRole) && !ev.ShouldPreserveInventory)
            {
                if (ev.Items == null)
                {
                    Log.Warn("items is null");
                    return;
                }

                ev.Items.Clear();
                ev.Items.AddRange(StartItems(ev.NewRole, ev.Player));

                if (_plugin.Config.StartingInventories[ev.NewRole].Ammo != null && _plugin.Config.StartingInventories[ev.NewRole].Ammo.Count > 0)
                {
                    if (_plugin.Config.StartingInventories[ev.NewRole].Ammo.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup) && userGroup == ev.Player.Group)))
                    {
                        ev.Ammo.Clear();
                            foreach ((ItemType type, ushort amount, string group) in _plugin.Config.StartingInventories[ev.NewRole].Ammo)
                            {
                                if (string.IsNullOrEmpty(group) || group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(group, out UserGroup userGroup) && userGroup == ev.Player.Group))
                                {
                                    ev.Ammo.Add(type, amount);
                                }
                            }
                    }
                }
            }
        }
        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn($"{nameof(OnSpawned)}: Triggering player is null.");
                return;
            }

            RoleTypeId NewRole = ev.Player.Role.Type;
            if (_plugin.Config.HealthValues != null && _plugin.Config.HealthValues.TryGetValue(NewRole, out int health))
            {
                ev.Player.Health = health;
                ev.Player.MaxHealth = health;
            }
            if (NewRole is not RoleTypeId.Spectator && _plugin.Config.PlayerHealthInfo)
            {
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
            }

            if (_plugin.Config.AfkIgnoredRoles.Contains(NewRole) && _plugin.AfkDict.ContainsKey(ev.Player))
                _plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(NewRole is RoleTypeId.Spectator ? _plugin.AfkDict[ev.Player].Item1 : 0, ev.Player.Position); ;

        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player != null && _plugin.Config.HealthOnKill != null && _plugin.Config.HealthOnKill.ContainsKey(ev.Player.Role))
            {

                if (ev.Player.Health + _plugin.Config.HealthOnKill[ev.Player.Role] <= ev.Player.MaxHealth)
                    ev.Player.Health += _plugin.Config.HealthOnKill[ev.Player.Role];
                else
                    ev.Player.Health = ev.Player.MaxHealth;
            }
        }

        public List<ItemType> StartItems(RoleTypeId role, Player player = null)
        {
            List<ItemType> items = new();

            for (int i = 0; i < _plugin.Config.StartingInventories[role].UsedSlots; i++)
            {
                int r = _plugin.Rng.Next(100);
                foreach ((string item, double chance, string groupKey) in _plugin.Config.StartingInventories[role][i])
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
                                customItem.Give(player);
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
            if (_plugin.Config.RoleDamageMultipliers != null && ev.Attacker != null && _plugin.Config.RoleDamageMultipliers.ContainsKey(ev.Attacker.Role))
                ev.Amount *= _plugin.Config.RoleDamageMultipliers[ev.Attacker.Role];

            if (_plugin.Config.DamageMultipliers != null && _plugin.Config.DamageMultipliers.ContainsKey(ev.DamageHandler.Type))
            {
                ev.Amount *= _plugin.Config.DamageMultipliers[ev.DamageHandler.Type];
            }

            if (_plugin.Config.PlayerHealthInfo)
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";

            if (ev.Attacker is not null && _plugin.AfkDict.ContainsKey(ev.Attacker))
            {
                Log.Debug($"Resetting {ev.Attacker.Nickname} AFK timer.");
                _plugin.AfkDict[ev.Attacker] = new Tuple<int, Vector3>(0, ev.Attacker.Position);
            }
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.IsCuffed && _plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;

            if (_plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                _plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (ev.Player.IsCuffed && _plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;

            if (_plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                _plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }

        public void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev)
        {
            ev.Drain *= _plugin.Config.RadioBatteryDrainMultiplier;
        }

        public void AntiAfkEventHandler(IPlayerEvent ev)
        {
            if (ev.Player != null && _plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                _plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }
    }
}