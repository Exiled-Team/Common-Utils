using PluginAPI.Roles;

namespace Common_Utilities.EventHandlers
{
#pragma warning disable IDE0018
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Common_Utilities.ConfigObjects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.CustomItems.API.Features;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.EventArgs.Player;
    using PlayerRoles;
    using UnityEngine;

    using Player = Exiled.API.Features.Player;

    public class PlayerHandlers
    {
        private readonly Config config;

        public PlayerHandlers(Plugin plugin) => this.config = plugin.Config;

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(config.JoinMessageDuration, message);
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn($"{nameof(OnChangingRole)}: Triggering player is null.");
                return;
            }

            if (config.StartingInventories.ContainsKey(ev.NewRole) && !ev.ShouldPreserveInventory)
            {
                if (ev.Items == null)
                {
                    Log.Warn("items is null");
                    return;
                }

                ev.Items.Clear();
                ev.Items.AddRange(StartItems(ev.NewRole, ev.Player));

                if (config.StartingInventories[ev.NewRole].Ammo != null && config.StartingInventories[ev.NewRole].Ammo.Count > 0)
                {
                    if (config.StartingInventories[ev.NewRole].Ammo.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup) && userGroup == ev.Player.Group)))
                    {
                        ev.Ammo.Clear();
                        foreach ((ItemType type, ushort amount, string group) in config.StartingInventories[ev.NewRole].Ammo)
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

            RoleTypeId newRole = ev.Player.Role.Type;
            if (config.HealthValues != null && config.HealthValues.TryGetValue(newRole, out int health))
            {
                ev.Player.Health = health;
                ev.Player.MaxHealth = health;
            }

            if (ev.Player.Role is FpcRole && config.PlayerHealthInfo)
            {
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
            }

            if (config.AfkIgnoredRoles.Contains(newRole) && Plugin.AfkDict.TryGetValue(ev.Player, out Tuple<int, Vector3> value))
                Plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(newRole is RoleTypeId.Spectator ? value.Item1 : 0, ev.Player.Position);
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Attacker != null && config.HealthOnKill.ContainsKey(ev.Attacker.Role))
            {
                ev.Attacker.Heal(config.HealthOnKill[ev.Attacker.Role]);
            }
        }

        public List<ItemType> StartItems(RoleTypeId role, Player player = null)
        {
            List<ItemType> items = new();

            for (int i = 0; i < config.StartingInventories[role].UsedSlots; i++)
            {
                IEnumerable<ItemChance> itemChances = config.StartingInventories[role][i].Where(x => player == null || string.IsNullOrEmpty(x.Group) || x.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(x.Group, out var group) && group == player.Group));
                double r;
                if (config.AdditiveProbabilities)
                    r = Plugin.Random.NextDouble() * itemChances.Sum(val => val.Chance);
                else
                    r = Plugin.Random.NextDouble() * 100;
                Log.Debug($"[StartItems] ActualChance ({r})/{itemChances.Sum(val => val.Chance)}");
                foreach ((string item, double chance, string groupKey) in itemChances)
                {
                    Log.Debug($"[StartItems] Probability ({r})/{chance}");
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
                                customItem!.Give(player);
                            else
                                Log.Warn($"{nameof(StartItems)}: Tried to give {customItem!.Name} to a null player.");
                            
                            break;
                        }
                        else
                            Log.Warn($"{nameof(StartItems)}: {item} is not a valid ItemType or it is a CustomItem that is not installed! It is being skipper in inventory decisions.");
                    }

                    r -= chance;
                }
            }

            return items;
        }

        public string FormatJoinMessage(Player player) => 
            string.IsNullOrEmpty(config.JoinMessage) ? string.Empty : config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");
        
        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            float damageMultiplier;
            if (config.RoleDamageMultipliers != null && ev.Attacker != null && config.RoleDamageMultipliers.TryGetValue(ev.Attacker.Role, out damageMultiplier))
                ev.Amount *= damageMultiplier;

            if (config.DamageMultipliers != null && config.DamageMultipliers.TryGetValue(ev.DamageHandler.Type, out damageMultiplier))
                ev.Amount *= damageMultiplier;

            if (config.PlayerHealthInfo)
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";

            if (ev.Attacker is not null && Plugin.AfkDict.ContainsKey(ev.Attacker))
            {
                Log.Debug($"Resetting {ev.Attacker.Nickname} AFK timer.");
                Plugin.AfkDict[ev.Attacker] = new Tuple<int, Vector3>(0, ev.Attacker.Position);
            }
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (Plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                Plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (Plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                Plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player.IsCuffed && config.DisarmedEscapeSwitchRole.TryGetValue(ev.Player.Role, out RoleTypeId newRole))
            {
                ev.NewRole = newRole;
                ev.IsAllowed = newRole != RoleTypeId.None;
            }
        }

        public void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev)
        {
            ev.Drain *= config.RadioBatteryDrainMultiplier;
        }

        public void AntiAfkEventHandler(IPlayerEvent ev)
        {
            if (ev.Player != null && Plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                Plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }
    }
}