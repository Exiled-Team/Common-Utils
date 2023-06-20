using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Scp914;
using PlayerRoles;

namespace Common_Utilities.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Common_Utilities.ConfigObjects;
    using Exiled.API.Enums;
    using Exiled.API.Features.Items;
    using UnityEngine;
    using Firearm = Exiled.API.Features.Items.Firearm;

    public class MapHandlers
    {
        private readonly Plugin _plugin;
        public MapHandlers(Plugin plugin) => _plugin = plugin;
        
        public void OnScp914UpgradingItem(UpgradingPickupEventArgs ev)
        {
            if (_plugin.Config.Scp914ItemChanges != null && _plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<ItemUpgradeChance> itemUpgradeChance = _plugin.Config.Scp914ItemChanges[ev.KnobSetting].Where(x => x.Original == ev.Pickup.Type);

                foreach ((ItemType sourceItem, ItemType destinationItem, double chance) in itemUpgradeChance)
                {
                    double r;
                    if (_plugin.Config.AdditiveProbabilities)
                        r = _plugin.Rng.NextDouble() * itemUpgradeChance.Sum(x => x.Chance);
                    else
                        r = _plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingItem)}: SCP-914 is trying to upgrade a {ev.Pickup.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        UpgradeItem(ev.Pickup, destinationItem, ev.OutputPosition);
                        ev.IsAllowed = false;
                        break;
                    }
                    r -= chance;
                }
            }
        }

        public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (_plugin.Config.Scp914ItemChanges != null && _plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<ItemUpgradeChance> itemUpgradeChance = _plugin.Config.Scp914ItemChanges[ev.KnobSetting].Where(x => x.Original == ev.Item.Type);

                foreach ((ItemType sourceItem, ItemType destinationItem, double chance) in itemUpgradeChance)
                {
                    double r;
                    if (_plugin.Config.AdditiveProbabilities)
                        r = _plugin.Rng.NextDouble() * itemUpgradeChance.Sum(x => x.Chance);
                    else
                        r = _plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.RemoveItem(ev.Item);
                        if (destinationItem is not ItemType.None)
                            ev.Player.AddItem(destinationItem);
                        break;
                    }
                    r -= chance;
                }
            }
        }

        public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (_plugin.Config.Scp914ClassChanges != null && _plugin.Config.Scp914ClassChanges.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<PlayerUpgradeChance> playerUpgradeChance = _plugin.Config.Scp914ClassChanges[ev.KnobSetting].Where(x => x.Original == ev.Player.Role);

                foreach ((RoleTypeId sourceRole, RoleTypeId destinationRole, double chance, bool keepInventory) in playerUpgradeChance)
                {
                    double r;
                    if (_plugin.Config.AdditiveProbabilities)
                        r = _plugin.Rng.NextDouble() * playerUpgradeChance.Sum(x => x.Chance);
                    else
                        r = _plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role})is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.Role.Set(destinationRole, SpawnReason.Respawn, keepInventory ? RoleSpawnFlags.None : RoleSpawnFlags.AssignInventory);

                        ev.Player.Position = ev.OutputPosition;
                        break;
                    }
                    r -= chance;
                }
            }

            if (_plugin.Config.Scp914EffectChances != null && _plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Role.Side != Side.Scp || !_plugin.Config.ScpsImmuneTo914Effects))
            {
                IEnumerable<Scp914EffectChance> scp914EffectChances = _plugin.Config.Scp914EffectChances[ev.KnobSetting];

                foreach ((EffectType effect, double chance, float duration) in scp914EffectChances)
                {
                    double r;
                    if (_plugin.Config.AdditiveProbabilities)
                        r = _plugin.Rng.NextDouble() * scp914EffectChances.Sum(x => x.Chance);
                    else
                        r = _plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.EnableEffect(effect, duration);
                        if (_plugin.Config.Scp914EffectsExclusivity)
                            break;
                    }
                    r -= chance;
                }
            }

            if (_plugin.Config.Scp914TeleportChances != null && _plugin.Config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<Scp914TeleportChance> scp914TeleportChances = _plugin.Config.Scp914TeleportChances[ev.KnobSetting];

                foreach ((RoomType roomType, Vector3 offset, double chance, float damage, ZoneType zone) in _plugin.Config.Scp914TeleportChances[ev.KnobSetting])
                {
                    double r;
                    if (_plugin.Config.AdditiveProbabilities)
                        r = _plugin.Rng.NextDouble() * scp914TeleportChances.Sum(x => x.Chance);
                    else
                        r = _plugin.Rng.NextDouble() * 100;

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
                            ev.OutputPosition = Room.Get(roomType).Position + (Vector3.up * 1.5f) + offset;
                            if (damage > 0f)
                            {
                                float amount = ev.Player.MaxHealth * damage;
                                if (damage > 1f)
                                    amount = damage;

                                Log.Debug(
                                    $"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is being damaged for {amount}. -- {ev.Player.Health} * {damage}");
                                ev.Player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
                            }
                        }

                        break;
                    }
                    r -= chance;
                }
            }
        }

        internal void UpgradeItem(Pickup oldItem, ItemType newItem, Vector3 pos)
        {
            if (newItem is not ItemType.None)
            {
                Item item = Item.Create(newItem);
                if (oldItem is FirearmPickup oldFirearm && item is Firearm firearm)
                    firearm.Ammo = oldFirearm.Ammo <= firearm.MaxAmmo ? oldFirearm.Ammo : firearm.MaxAmmo;

                item.CreatePickup(pos);
            }

            oldItem.Destroy();
        }
    }
}