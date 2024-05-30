using Exiled.API.Extensions;

namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Common_Utilities.ConfigObjects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events.EventArgs.Scp914;
    using MEC;
    using PlayerRoles;
    using UnityEngine;

    public class MapHandlers
    {
        private readonly Main plugin;

        public MapHandlers(Main plugin) => this.plugin = plugin;
        
        public void OnScp914UpgradingItem(UpgradingPickupEventArgs ev)
        {
            if (plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<ItemUpgradeChance> itemUpgradeChance = plugin.Config.Scp914ItemChanges[ev.KnobSetting].Where(x => x.Original == ev.Pickup.Type);

                foreach ((ItemType sourceItem, ItemType destinationItem, double chance, int count) in itemUpgradeChance)
                {
                    double r;
                    if (plugin.Config.AdditiveProbabilities)
                        r = plugin.Rng.NextDouble() * itemUpgradeChance.Sum(x => x.Chance);
                    else
                        r = plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingItem)}: SCP-914 is trying to upgrade a {ev.Pickup.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        UpgradeItem(ev.Pickup, destinationItem, ev.OutputPosition, count);
                        ev.IsAllowed = false;
                        break;
                    }

                    r -= chance;
                }
            }
        }

        public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<ItemUpgradeChance> itemUpgradeChance = plugin.Config.Scp914ItemChanges[ev.KnobSetting].Where(x => x.Original == ev.Item.Type);

                foreach ((ItemType sourceItem, ItemType destinationItem, double chance, int count) in itemUpgradeChance)
                {
                    double r;
                    if (plugin.Config.AdditiveProbabilities)
                        r = plugin.Rng.NextDouble() * itemUpgradeChance.Sum(x => x.Chance);
                    else
                        r = plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.RemoveItem(ev.Item);
                        if (destinationItem is not ItemType.None)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                if (!ev.Player.IsInventoryFull)
                                    ev.Player.AddItem(destinationItem);
                                else
                                    Pickup.CreateAndSpawn(destinationItem, Scp914.OutputPosition, ev.Player.Rotation, ev.Player);
                            }
                        }

                        break;
                    }

                    r -= chance;
                }
            }
        }

        public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (plugin.Config.Scp914ClassChanges != null && plugin.Config.Scp914ClassChanges.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<PlayerUpgradeChance> playerUpgradeChance = plugin.Config.Scp914ClassChanges[ev.KnobSetting].Where(x => x.Original == ev.Player.Role);

                foreach ((RoleTypeId sourceRole, string destinationRole, double chance, bool keepInventory) in playerUpgradeChance)
                {
                    double r;
                    if (plugin.Config.AdditiveProbabilities)
                        r = plugin.Rng.NextDouble() * playerUpgradeChance.Sum(x => x.Chance);
                    else
                        r = plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role})is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        if (Enum.TryParse(destinationRole, true, out RoleTypeId roleType))
                        {
                            ev.Player.Role.Set(roleType, SpawnReason.Respawn, RoleSpawnFlags.None);
                        }
                        else if (CustomRole.TryGet(destinationRole, out CustomRole customRole))
                        {
                            if (customRole is not null)
                            {
                                customRole.AddRole(ev.Player);
                                Timing.CallDelayed(0.5f, () => ev.Player.Teleport(ev.OutputPosition));
                            }
                        }

                        ev.Player.Position = ev.OutputPosition;
                        break;
                    }

                    r -= chance;
                }
            }

            if (plugin.Config.Scp914EffectChances != null && plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Role.Side != Side.Scp || !plugin.Config.ScpsImmuneTo914Effects))
            {
                IEnumerable<Scp914EffectChance> scp914EffectChances = plugin.Config.Scp914EffectChances[ev.KnobSetting];

                foreach ((EffectType effect, double chance, float duration) in scp914EffectChances)
                {
                    double r;
                    if (plugin.Config.AdditiveProbabilities)
                        r = plugin.Rng.NextDouble() * scp914EffectChances.Sum(x => x.Chance);
                    else
                        r = plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.EnableEffect(effect, duration);
                        if (plugin.Config.Scp914EffectsExclusivity)
                            break;
                    }

                    r -= chance;
                }
            }

            if (plugin.Config.Scp914TeleportChances != null && plugin.Config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<Scp914TeleportChance> scp914TeleportChances = plugin.Config.Scp914TeleportChances[ev.KnobSetting];

                foreach ((RoomType roomType, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in plugin.Config.Scp914TeleportChances[ev.KnobSetting])
                {
                    double r;
                    if (plugin.Config.AdditiveProbabilities)
                        r = plugin.Rng.NextDouble() * scp914TeleportChances.Sum(x => x.Chance);
                    else
                        r = plugin.Rng.NextDouble() * 100;

                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        if (zone != ZoneType.Unspecified)
                        {
                            ev.OutputPosition = Room.List.Where(x => x.Zone == zone && !ignoredRooms.Contains(x.Type)).GetRandomValue().Position + ((Vector3.up * 1.5f) + offset);
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

        internal void UpgradeItem(Pickup oldItem, ItemType newItem, Vector3 pos, int count)
        {
            Quaternion quaternion = oldItem.Rotation;
            Player previousOwner = oldItem.PreviousOwner;
            oldItem.Destroy();
            if (newItem is not ItemType.None)
            {
                for (int i = 0; i < count; i++)
                {
                    Pickup.CreateAndSpawn(newItem, pos, quaternion, previousOwner);
                }
            }
        }
    }
}