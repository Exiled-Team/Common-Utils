using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Scp914;
using PlayerRoles;

namespace Common_Utilities.EventHandlers;

using System;
using System.Collections.Generic;
using System.Linq;

using Common_Utilities.ConfigObjects;

using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Loader;

using InventorySystem.Items.Firearms;
using MEC;
using UnityEngine;
using Firearm = Exiled.API.Features.Items.Firearm;

public class MapHandlers
{
    private readonly Plugin plugin;

    public MapHandlers(Plugin plugin) => this.plugin = plugin;

    public void OnScp914UpgradingItem(UpgradingPickupEventArgs ev)
    {
        if (plugin.Config.Scp914ItemChanges != null && plugin.Config.Scp914ItemChanges.TryGetValue(ev.KnobSetting, out List<ItemUpgradeChance>? change))
        {
            foreach ((ItemType sourceItem, ItemType destinationItem, double chance) in change)
            {
                if (sourceItem != ev.Pickup.Type)
                    continue;

                double r = Loader.Random.NextDouble() * 100;
                Log.Debug($"{nameof(OnScp914UpgradingItem)}: SCP-914 is trying to upgrade a {ev.Pickup.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                if (r <= chance)
                {
                    UpgradeItem(ev.Pickup, destinationItem, ev.OutputPosition);
                    ev.IsAllowed = false;
                    break;
                }
            }
        }
    }

    public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
    {
        if (plugin.Config.Scp914ItemChanges != null && plugin.Config.Scp914ItemChanges.TryGetValue(ev.KnobSetting, out List<ItemUpgradeChance>? change))
        {
            foreach ((ItemType sourceItem, ItemType destinationItem, double chance) in change)
            {
                if (sourceItem != ev.Item.Type)
                    continue;

                double r = Loader.Random.NextDouble() * 100;
                Log.Debug($"{nameof(OnScp914UpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                if (r <= chance)
                {
                    ev.Player.RemoveItem(ev.Item);
                    if (destinationItem is not ItemType.None)
                        ev.Player.AddItem(destinationItem);
                    break;
                }
            }
        }
    }

    public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
    {
        if (plugin.Config.Scp914ClassChanges != null && plugin.Config.Scp914ClassChanges.TryGetValue(ev.KnobSetting, out List<PlayerUpgradeChance>? change))
        {
            foreach ((RoleTypeId sourceRole, string destinationRole, double chance, bool keepInventory) in change)
            {
                if (sourceRole != ev.Player.Role)
                    continue;

                double r = Loader.Random.NextDouble() * 100;
                Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role})is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})");
                if (r <= chance)
                {
                    if (Enum.TryParse(destinationRole, true, out RoleTypeId roleType))
                    {
                        ev.Player.Role.Set(roleType, SpawnReason.Respawn, RoleSpawnFlags.None);
                    }
                    else if (CustomRole.TryGet(destinationRole, out CustomRole? customRole))
                    {
                        if (customRole is not null)
                        {
                            customRole.AddRole(ev.Player);
                            Timing.CallDelayed(0.5f, () => ev.Player.Teleport(ev.OutputPosition));
                        }
                    }

                    if (!keepInventory)
                    {
                        ev.Player.ClearInventory();
                        ev.Player.Ammo.Clear();
                    }

                    ev.Player.Position = ev.OutputPosition;
                    break;
                }
            }
        }

        if (plugin.Config.Scp914EffectChances != null && plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Role.Side != Side.Scp || !plugin.Config.ScpsImmuneTo914Effects))
        {
            foreach ((EffectType effect, double chance, float duration) in plugin.Config.Scp914EffectChances[ev.KnobSetting])
            {
                double r = Loader.Random.NextDouble() * 100;
                Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r})");
                if (r <= chance)
                {
                    ev.Player.EnableEffect(effect, duration);
                    if (plugin.Config.Scp914EffectsExclusivity)
                        break;
                }
            }
        }

        if (plugin.Config.Scp914TeleportChances != null && plugin.Config.Scp914TeleportChances.TryGetValue(ev.KnobSetting, out List<Scp914TeleportChance>? teleportChance))
        {
            foreach ((RoomType roomType, Vector3 offset, double chance, float damage, ZoneType zone) in teleportChance)
            {
                double r = Loader.Random.NextDouble() * 100;
                Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {r <= chance} ({r})");
                if (r <= chance)
                {
                    if (zone != ZoneType.Unspecified)
                    {
                        ev.OutputPosition = Room.Random(zone).Position + ((Vector3.up * 1.5f) + offset);
                        if (damage > 0f)
                        {
                            float amount = ev.Player.MaxHealth * damage;
                            if (damage > 1f)
                                amount = damage;

                            Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is being damaged for {amount}. -- {ev.Player.Health} * {damage}");
                            ev.Player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
                        }
                    }
                    else
                    {
                        foreach (Room room in Room.List)
                        {
                            if (room.Type == roomType)
                            {
                                ev.OutputPosition = (room.Position + (Vector3.up * 1.5f)) + offset;
                                if (damage > 0f)
                                {
                                    float amount = ev.Player.MaxHealth * damage;
                                    if (damage > 1f)
                                        amount = damage;

                                    Log.Debug(
                                        $"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is being damaged for {amount}. -- {ev.Player.Health} * {damage}");
                                    ev.Player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
                                }

                                break;
                            }
                        }
                    }

                    break;
                }
            }
        }
    }

    internal void UpgradeItem(Pickup oldItem, ItemType newItem, Vector3 pos)
    {
        if (newItem is not ItemType.None)
        {
            Item item = Item.Create(newItem);
            if (oldItem is Exiled.API.Features.Pickups.FirearmPickup oldFirearm && item is Firearm firearm)
                firearm.Ammo = oldFirearm.Ammo <= firearm.MaxAmmo ? oldFirearm.Ammo : firearm.MaxAmmo;

            item.CreatePickup(pos);
        }

        oldItem.Destroy();
    }
}