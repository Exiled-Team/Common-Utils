using System.Collections.Generic;
using Exiled.API.Features;
using Scp914;

namespace Common_Utilities.EventHandlers
{
    using System;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs;
    using InventorySystem.Items.Firearms;
    using InventorySystem.Items.ThrowableProjectiles;
    using UnityEngine;
    using Firearm = Exiled.API.Features.Items.Firearm;

    public class MapHandlers
    {
        private readonly Plugin plugin;
        public MapHandlers(Plugin plugin) => this.plugin = plugin;
        
        public void OnScp914UpgradingItem(UpgradingItemEventArgs ev)
        {
            if (plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914ItemChanges[ev.KnobSetting])
                {
                    if (sourceItem != ev.Item.Type)
                        continue;

                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingItem)}: SCP-914 is trying to upgrade a {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        UpgradeItem(ev.Item, destinationItem, ev.OutputPosition);
                        ev.IsAllowed = false;
                        break;
                    }
                }
            }
        }

        public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914ItemChanges[
                    ev.KnobSetting])
                {
                    if (sourceItem != ev.Item.Type)
                        continue;

                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        ev.Player.RemoveItem(ev.Item);
                        if (destinationItem != ItemType.None)
                            ev.Player.AddItem(destinationItem);
                        break;
                    }
                }
            }
        }

        public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (plugin.Config.Scp914ClassChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((RoleType sourceRole, RoleType destinationRole, int chance) in plugin.Config.Scp914ClassChanges[ev.KnobSetting])
                {
                    if (sourceRole != ev.Player.Role || (destinationRole == RoleType.Scp106 && OneOhSixContainer.used))
                        continue;

                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role})is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        ev.Player.SetRole(destinationRole, SpawnReason.ForceClass, true);
                        break;
                    }
                }
            }

            if (plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Side != Side.Scp || !plugin.Config.ScpsImmuneTo914Effects))
            {
                foreach ((EffectType effect, int chance, float duration) in plugin.Config.Scp914EffectChances[ev.KnobSetting])
                {
                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r}", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        ev.Player.EnableEffect(effect, duration);
                        if (plugin.Config.Scp914EffectsExclusivity)
                            break;
                    }
                }
            }

            if (plugin.Config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
            {
                foreach ((RoomType roomType, Vector3 offset, int chance) in plugin.Config.Scp914TeleportChances[ev.KnobSetting])
                {
                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {r <= chance} ({r})", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        foreach (Room room in Map.Rooms)
                            if (room.Type == roomType)
                            {
                                ev.OutputPosition = (room.Position + (Vector3.up * 1.5f)) + offset;
                                break;
                            }

                        break;
                    }
                }
            }

            if (plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting))
            {
                foreach ((EffectType effect, int chance, float duration) in plugin.Config.Scp914EffectChances[ev.KnobSetting])
                {
                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r}", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        ev.Player.EnableEffect(effect, duration);
                        if (plugin.Config.Scp914EffectsExclusivity)
                            break;
                    }
                }
            }

            if (plugin.Config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
            {
                foreach ((RoomType roomType, Vector3 offset, int chance) in plugin.Config.Scp914TeleportChances[
                    ev.KnobSetting])
                {
                    int r = plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {r <= chance} ({r})", plugin.Config.Debug);
                    if (r <= chance)
                    {
                        foreach (Room room in Map.Rooms)
                            if (room.Type == roomType)
                            {
                                ev.OutputPosition = (room.Position + (Vector3.up * 1.5f)) + offset;
                                break;
                            }
                    }
                }
            }
        }

        internal void UpgradeItem(Pickup oldItem, ItemType newItem, Vector3 pos)
        {
            if (newItem != ItemType.None)
            {
                Item item = new Item(newItem);
                if (oldItem.Base is FirearmPickup firearmPickup && item is Firearm firearm)
                    firearm.Ammo = firearmPickup.NetworkStatus.Ammo <= firearm.MaxAmmo
                        ? firearmPickup.NetworkStatus.Ammo
                        : firearm.MaxAmmo;

                item.Spawn(pos);
            }

            oldItem.Destroy();
        }
    }
}