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
        private readonly Main plugin;

        public PlayerHandlers(Main plugin) => this.plugin = plugin;

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(plugin.Config.JoinMessageDuration, message);
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn($"{nameof(OnChangingRole)}: Triggering player is null.");
                return;
            }

            if (plugin.Config.StartingInventories.ContainsKey(ev.NewRole) && !ev.ShouldPreserveInventory)
            {
                if (ev.Items == null)
                {
                    Log.Warn("items is null");
                    return;
                }

                ev.Items.Clear();
                ev.Items.AddRange(StartItems(ev.NewRole, ev.Player));

                if (plugin.Config.StartingInventories[ev.NewRole].Ammo != null && plugin.Config.StartingInventories[ev.NewRole].Ammo.Count > 0)
                {
                    if (plugin.Config.StartingInventories[ev.NewRole].Ammo.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup) && userGroup == ev.Player.Group)))
                    {
                        ev.Ammo.Clear();
                        foreach ((ItemType type, ushort amount, string group) in plugin.Config.StartingInventories[ev.NewRole].Ammo)
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
            if (plugin.Config.HealthValues != null && plugin.Config.HealthValues.TryGetValue(newRole, out int health))
            {
                ev.Player.Health = health;
                ev.Player.MaxHealth = health;
            }

            if (ev.Player.Role is FpcRole && plugin.Config.PlayerHealthInfo)
            {
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
            }

            if (plugin.Config.AfkIgnoredRoles.Contains(newRole) && plugin.AfkDict.TryGetValue(ev.Player, out Tuple<int, Vector3> value))
                plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(newRole is RoleTypeId.Spectator ? value.Item1 : 0, ev.Player.Position);
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Attacker != null && plugin.Config.HealthOnKill.ContainsKey(ev.Attacker.Role))
            {
                ev.Attacker.Heal(plugin.Config.HealthOnKill[ev.Attacker.Role]);
            }
        }

        public List<ItemType> StartItems(RoleTypeId role, Player player = null)
        {
            List<ItemType> items = new();

            for (int i = 0; i < plugin.Config.StartingInventories[role].UsedSlots; i++)
            {
                IEnumerable<ItemChance> itemChances = plugin.Config.StartingInventories[role][i].Where(x => player == null || string.IsNullOrEmpty(x.Group) || x.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(x.Group, out var group) && group == player.Group));
                double r;
                if (plugin.Config.AdditiveProbabilities)
                    r = plugin.Rng.NextDouble() * itemChances.Sum(val => val.Chance);
                else
                    r = plugin.Rng.NextDouble() * 100;
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
            string.IsNullOrEmpty(plugin.Config.JoinMessage) ? string.Empty : plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");
        
        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            float damageMultiplier;
            if (plugin.Config.RoleDamageMultipliers != null && ev.Attacker != null && plugin.Config.RoleDamageMultipliers.TryGetValue(ev.Attacker.Role, out damageMultiplier))
                ev.Amount *= damageMultiplier;

            if (plugin.Config.DamageMultipliers != null && plugin.Config.DamageMultipliers.TryGetValue(ev.DamageHandler.Type, out damageMultiplier))
                ev.Amount *= damageMultiplier;

            if (plugin.Config.PlayerHealthInfo)
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";

            if (ev.Attacker is not null && plugin.AfkDict.ContainsKey(ev.Attacker))
            {
                Log.Debug($"Resetting {ev.Attacker.Nickname} AFK timer.");
                plugin.AfkDict[ev.Attacker] = new Tuple<int, Vector3>(0, ev.Attacker.Position);
            }
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.IsCuffed && plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;

            if (plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (ev.Player.IsCuffed && plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;

            if (plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.EscapeScenario is EscapeScenario.CustomEscape)
            {
                ev.NewRole = ev.Player.Role.Type switch
                {
                    RoleTypeId.FacilityGuard or RoleTypeId.NtfPrivate or RoleTypeId.NtfSergeant or RoleTypeId.NtfCaptain or RoleTypeId.NtfSpecialist => RoleTypeId.ChaosConscript,
                    RoleTypeId.ChaosConscript or RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosRifleman => RoleTypeId.ChaosConscript,
                    _ => RoleTypeId.None,
                };

                ev.IsAllowed = ev.NewRole is not RoleTypeId.None;
            }
        }

        public void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev)
        {
            ev.Drain *= plugin.Config.RadioBatteryDrainMultiplier;
        }

        public void AntiAfkEventHandler(IPlayerEvent ev)
        {
            if (ev.Player != null && plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }
    }
}