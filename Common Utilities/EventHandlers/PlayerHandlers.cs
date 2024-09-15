namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Common_Utilities.ConfigObjects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.CustomModules;
    using Exiled.CustomModules.API.Features;
    using Exiled.CustomModules.API.Features.CustomItems;
    using Exiled.CustomModules.API.Features.CustomRoles;
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
            if (ev.Player is null)
            {
                Log.DebugWithContext($"Triggering player is null.");
                return;
            }

            if (ev.Player.Is(out Pawn pawn) && pawn.HasCustomRole)
            {
                Log.DebugWithContext($"Triggering player has a custom role.");
                return;
            }

            if (!plugin.Config.StartingInventories.ContainsKey(ev.NewRole) || ev.ShouldPreserveInventory)
                return;

            if (ev.Items is null)
            {
                Log.DebugWithContext("Items collection is null.");
                return;
            }

            ev.Items.Clear();
            ev.Items.AddRange(StartItems(ev.NewRole, ev.Player));

            if (plugin.Config.StartingInventories[ev.NewRole].Ammo is not null && plugin.Config.StartingInventories[ev.NewRole].Ammo.Count > 0)
            {
                if (plugin.Config.StartingInventories[ev.NewRole].Ammo.Any(s =>
                        string.IsNullOrEmpty(s.Group) || s.Group == "none" ||
                        (Server.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup)
                         && userGroup == ev.Player.Group)))
                {
                    ev.Ammo.Clear();
                    foreach ((ItemType type, ushort amount, string group) in plugin.Config.StartingInventories[ev.NewRole].Ammo)
                    {
                        if (string.IsNullOrEmpty(group) || group == "none" || 
                            (Server.PermissionsHandler._groups.TryGetValue(group, out UserGroup userGroup)
                             && userGroup == ev.Player.Group))
                            ev.Ammo.Add(type, amount);
                    }
                }
            }
        }

        public void OnChangedRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player is null)
            {
                Log.DebugWithContext("Triggering player is null.");
                return;
            }

            RoleTypeId newRole = ev.Player.Role.Type;
            if (plugin.Config.HealthValues is not null &&
                plugin.Config.HealthValues.TryGetValue(newRole, out int health) &&
                (CustomModules.Instance is null || !ev.Player.Cast<Pawn>().HasCustomRole))
            {
                ev.Player.Health = health;
                ev.Player.MaxHealth = health;
            }

            if (ev.Player.Role is FpcRole && plugin.Config.PlayerHealthInfo)
                ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";

            if (plugin.Config.AfkIgnoredRoles.Contains(newRole) && plugin.AfkDict.TryGetValue(ev.Player, out Tuple<int, Vector3> value))
                plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(newRole is RoleTypeId.Spectator ? value.Item1 : 0, ev.Player.Position);
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Attacker is not null && plugin.Config.HealthOnKill.ContainsKey(ev.Attacker.Role))
            {
                foreach (KeyValuePair<object, float> kvp in plugin.Config.HealthOnKill)
                {
                    if (kvp.Key is RoleTypeId roleType && ev.Attacker.Role == roleType)
                    {
                        ev.Attacker.Heal(kvp.Value);
                        return;
                    }
                    
                    if (CustomRole.TryGet(kvp.Key, out CustomRole customRole) &&
                        ev.Attacker.Cast<Pawn>().TryGetCustomRole(out CustomRole attackerRole) &&
                        attackerRole == customRole)
                    {
                        ev.Attacker.Heal(kvp.Value);
                        return;
                    }
                }
            }
        }

        public List<ItemType> StartItems(RoleTypeId role, Player player = null)
        {
            List<ItemType> items = new();

            for (int i = 0; i < plugin.Config.StartingInventories[role].UsedSlots; i++)
            {
                IEnumerable<ItemChance> itemChances = plugin.Config.StartingInventories[role][i].Where(x =>
                        player is null || string.IsNullOrEmpty(x.Group) || x.Group == "none" || 
                        (ServerStatic.PermissionsHandler._groups.TryGetValue(x.Group, out UserGroup group) && group == player.Group));

                double r = plugin.Config.AdditiveProbabilities
                    ? plugin.Rng.NextDouble() * itemChances.Sum(val => val.Chance)
                    : plugin.Rng.NextDouble() * 100;

                Log.Debug($"[StartItems] ActualChance ({r})/{itemChances.Sum(val => val.Chance)}");

                foreach ((string item, double chance, _) in itemChances)
                {
                    Log.Debug($"[StartItems] Probability ({r})/{chance}");

                    if (r <= chance)
                    {
                        if (Enum.TryParse(item, true, out ItemType type))
                        {
                            items.Add(type);
                            break;
                        }

                        if (CustomItem.TryGet(item, out CustomItem customItem))
                        {
                            if (player is not null)
                                customItem!.Give(player);
                            else
                                Log.DebugWithContext($"Tried to give {customItem!.Name} to a null player.");
                            
                            break;
                        }

                        Log.WarnWithContext($"{item} is not a valid ItemType or it is a CustomItem that is not registered! It is being skipped in inventory decisions.");
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
            if (plugin.Config.RoleDamageMultipliers is not null && ev.Attacker is not null)
            {
                foreach (KeyValuePair<object, float> kvp in plugin.Config.HealthOnKill)
                {
                    if (kvp.Key is RoleTypeId roleType && ev.Attacker.Role == roleType)
                    {
                        ev.Amount *= kvp.Value;
                        return;
                    }
                    
                    if (CustomRole.TryGet(kvp.Key, out CustomRole customRole) &&
                        ev.Attacker.Cast<Pawn>().TryGetCustomRole(out CustomRole attackerRole) &&
                        attackerRole == customRole)
                    {
                        ev.Amount *= kvp.Value;
                        return;
                    }
                }
            }

            if (plugin.Config.DamageMultipliers is not null &&
                plugin.Config.DamageMultipliers.TryGetValue(ev.DamageHandler.Type, out float damageMultiplier))
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
            if (ev.EscapeScenario == EscapeScenario.CustomEscape)
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
            if (ev.Player is not null && plugin.AfkDict.ContainsKey(ev.Player))
            {
                Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
                plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
            }
        }
    }
}