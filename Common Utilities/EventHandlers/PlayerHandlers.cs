namespace Common_Utilities.EventHandlers;

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Loader;

using PlayerRoles;
using UnityEngine;

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
        if (ev.Player == null)
        {
            Log.Warn($"{nameof(OnChangingRole)}: Triggering player is null.");
            return;
        }

        if (plugin.Config.StartingInventories != null && plugin.Config.StartingInventories.ContainsKey(ev.NewRole) && !ev.ShouldPreserveInventory)
        {
            if (ev.Items == null)
            {
                Log.Warn("items is null");
                return;
            }

            ev.Items.Clear();
            ev.Items.AddRange(StartItems(ev.NewRole, ev.Player));

            if (plugin.Config.StartingInventories[ev.NewRole].Ammo?.Count > 0)
            {
                if (plugin.Config.StartingInventories[ev.NewRole].Ammo!.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup) && userGroup == ev.Player.Group)))
                {
                    ev.Ammo.Clear();
                    foreach ((ItemType type, ushort amount, string group) in plugin.Config.StartingInventories[ev.NewRole].Ammo!)
                    {
                        if (string.IsNullOrEmpty(group) || group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(group, out UserGroup userGroup) && userGroup == ev.Player.Group))
                        {
                            ev.Ammo.Add(type, amount);
                        }
                    }
                }
            }
        }

        if (plugin.Config.HealthValues != null && plugin.Config.HealthValues.ContainsKey(ev.NewRole))
        {
            ev.Player.Health = plugin.Config.HealthValues[ev.NewRole];
            ev.Player.MaxHealth = plugin.Config.HealthValues[ev.NewRole];
        }

        if (ev.NewRole is not RoleTypeId.Spectator && plugin.Config.PlayerHealthInfo)
        {
            ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
        }

        if (plugin.Config.AfkIgnoredRoles.Contains(ev.NewRole) && plugin.AfkDict.ContainsKey(ev.Player))
            plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(ev.NewRole is RoleTypeId.Spectator ? plugin.AfkDict[ev.Player].Item1 : 0, ev.Player.Position);;
    }

    public void OnPlayerDied(DiedEventArgs ev)
    {
        if (ev.Player != null && plugin.Config.HealthOnKill != null && plugin.Config.HealthOnKill.ContainsKey(ev.Player.Role))
        {

            if (ev.Player.Health + plugin.Config.HealthOnKill[ev.Player.Role] <= ev.Player.MaxHealth)
                ev.Player.Health += plugin.Config.HealthOnKill[ev.Player.Role];
            else
                ev.Player.Health = ev.Player.MaxHealth;
        }
    }

    public List<ItemType> StartItems(RoleTypeId role, Player? player = null)
    {
        List<ItemType> items = new();

        if (plugin.Config.StartingInventories is null)
            return new List<ItemType>();

        for (int i = 0; i < plugin.Config.StartingInventories[role].UsedSlots; i++)
        {
            int r = Loader.Random.Next(101);
            foreach ((string item, double chance, string groupKey) in plugin.Config.StartingInventories[role][i])
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

                    if (CustomItem.TryGet(item, out CustomItem? customItem))
                    {
                        if (player != null)
                            customItem?.Give(player);
                        else
                            Log.Warn($"{nameof(StartItems)}: Tried to give {customItem?.Name} to a null player.");
                        break;
                    }

                    Log.Warn($"{nameof(StartItems)}: {item} is not a valid ItemType or it is a CustomItem that is not installed! It is being skipper in inventory decisions.");
                }
            }
        }

        return items;
    }

    public void OnPlayerHurting(HurtingEventArgs ev)
    {
        if (plugin.Config.RoleDamageMultipliers != null && ev.Attacker != null && plugin.Config.RoleDamageMultipliers.TryGetValue(ev.Attacker.Role, out float damageMultiplier))
            ev.Amount *= damageMultiplier;

        if (plugin.Config.DamageMultipliers != null && plugin.Config.DamageMultipliers.TryGetValue(ev.DamageHandler.Type, out float multiplier))
        {
            ev.Amount *= multiplier;
        }

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

    public void OnPlayerEscaping(EscapingEventArgs ev)
    {
        if (!plugin.Config.DisarmSwitchTeams)
            return;

        if (ev.Player.IsCuffed)
        {
            ev.NewRole = ev.Player.Role.Type switch
            {
                RoleTypeId.ClassD => RoleTypeId.NtfPrivate,
                RoleTypeId.NtfSpecialist => RoleTypeId.ChaosRepressor,
                RoleTypeId.Scientist => RoleTypeId.ChaosConscript,
                RoleTypeId.ChaosConscript => RoleTypeId.NtfSergeant,
                RoleTypeId.NtfSergeant => RoleTypeId.ChaosRifleman,
                RoleTypeId.NtfCaptain => RoleTypeId.ChaosMarauder,
                RoleTypeId.NtfPrivate => RoleTypeId.ChaosConscript,
                RoleTypeId.FacilityGuard => RoleTypeId.ChaosConscript,
                RoleTypeId.ChaosRifleman => RoleTypeId.NtfSergeant,
                RoleTypeId.ChaosRepressor => RoleTypeId.NtfSpecialist,
                RoleTypeId.ChaosMarauder => RoleTypeId.NtfCaptain,
                _ => throw new ArgumentOutOfRangeException()
            };

            ev.IsAllowed = true;
        }
    }

    private string FormatJoinMessage(Player player) =>
        string.IsNullOrEmpty(plugin.Config.JoinMessage) ? string.Empty : plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");
}