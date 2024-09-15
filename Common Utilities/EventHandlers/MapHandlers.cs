namespace Common_Utilities.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;

    using Common_Utilities.ConfigObjects;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pickups;
    using Exiled.CustomModules;
    using Exiled.CustomModules.API.Features;
    using Exiled.CustomModules.API.Features.CustomItems;
    using Exiled.CustomModules.API.Features.CustomRoles;
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
            if (!plugin.Config.Scp914ItemChanges.TryGetValue(ev.KnobSetting, out List<ItemUpgradeChance> change))
                return;

            IEnumerable<ItemUpgradeChance> itemUpgradeChance = CustomItem.TryGet(ev.Pickup, out CustomItem customItem) ?
                change.Where(i => CustomItem.TryGet(i.OriginalItem, out CustomItem ci) && ci.Name == customItem.Name) :
                change.Where(i => i.OriginalItem is ItemType itemType && itemType == ev.Pickup.Type);

            foreach ((object sourceItem, object destinationItem, double chance, int count) in itemUpgradeChance)
            {
                double r = plugin.Config.AdditiveProbabilities
                    ? plugin.Rng.NextDouble() * itemUpgradeChance.Sum(x => x.Chance)
                    : plugin.Rng.NextDouble() * 100;

                Log.DebugWithContext($"SCP-914 is trying to upgrade a {ev.Pickup.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                if (r <= chance)
                {
                    UpgradeItem(ev.Pickup, destinationItem, ev.OutputPosition, count);
                    ev.IsAllowed = false;
                    break;
                }
            }
        }

        public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!plugin.Config.Scp914ItemChanges.TryGetValue(ev.KnobSetting, out List<ItemUpgradeChance> change))
                return;

            IEnumerable<ItemUpgradeChance> itemUpgradeChance = CustomItem.TryGet(ev.Item, out CustomItem customItem) ?
                change.Where(i => CustomItem.TryGet(i.OriginalItem, out CustomItem ci) && ci.Name == customItem.Name) :
                change.Where(i => i.OriginalItem is ItemType itemType && itemType == ev.Item.Type);

            foreach ((object sourceItem, object destinationItem, double chance, int count) in itemUpgradeChance)
            {
                double r = plugin.Config.AdditiveProbabilities
                    ? plugin.Rng.NextDouble() * itemUpgradeChance.Sum(x => x.Chance)
                    : plugin.Rng.NextDouble() * 100;

                Log.DebugWithContext($"{ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                if (r <= chance)
                {
                    ev.Player.RemoveItem(ev.Item);
                    if (destinationItem is ItemType itemType && itemType is not ItemType.None)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (!ev.Player.IsInventoryFull)
                                ev.Player.AddItem(itemType);
                            else
                                Pickup.CreateAndSpawn(itemType, Scp914.OutputPosition, ev.Player.Rotation, ev.Player);
                        }
                    }
                    else if (CustomItem.TryGet(destinationItem, out CustomItem ci))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (!ev.Player.IsInventoryFull)
                                ev.Player.Cast<Pawn>().AddItem(ci);
                            else
                                ci.Spawn(Scp914.OutputPosition, ev.Player);
                        }
                    }

                    break;
                }
            }
        }

        public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (plugin.Config.Scp914ClassChanges is not null && plugin.Config.Scp914ClassChanges.TryGetValue(ev.KnobSetting, out List<PlayerUpgradeChance> change))
            {
                IEnumerable<PlayerUpgradeChance> playerUpgradeChance = CustomModules.Instance is not null && ev.Player.Cast<Pawn>().CustomRole is not null ?
                    change.Where(i => CustomRole.TryGet(i.OriginalRole, out CustomRole cr) && cr.Name == ev.Player.Cast<Pawn>().CustomRole.Name) :
                    change.Where(i => i.OriginalRole is RoleTypeId roleType && roleType == ev.Player.Role);
                
                foreach ((object sourceRole, object destinationRole, double chance, bool keepInventory, bool keepHealth) in playerUpgradeChance)
                {
                    double r = plugin.Config.AdditiveProbabilities
                        ? plugin.Rng.NextDouble() * playerUpgradeChance.Sum(x => x.Chance)
                        : plugin.Rng.NextDouble() * 100;

                    Log.DebugWithContext($"{ev.Player.Nickname} ({ev.Player.Role}) is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        float originalHealth = ev.Player.Health;
                        List<object> originalItems = ev.Player.Items.Cast<object>().ToList();
                        Dictionary<ItemType, ushort> originalAmmo = ev.Player.Ammo;

                        IReadOnlyDictionary<uint, ushort> originalCustomAmmo = null;
                        if (CustomModules.Instance is not null)
                        {
                            originalItems.AddRange(ev.Player.Cast<Pawn>().CustomItems.Cast<object>());
                            originalCustomAmmo = ev.Player.Cast<Pawn>().CustomAmmoBox;
                        }
                        
                        if (destinationRole is RoleTypeId roleType)
                        {
                            ev.Player.Role.Set(roleType, SpawnReason.Respawn, RoleSpawnFlags.None);
                        }
                        else if (CustomRole.TryGet(destinationRole, out CustomRole customRole))
                        {
                            ev.Player.Cast<Pawn>().SetRole(customRole, spawnReason: SpawnReason.Respawn, roleSpawnFlags: RoleSpawnFlags.None);
                            Timing.CallDelayed(0.5f, () => ev.Player.Teleport(ev.OutputPosition));
                        }

                        if (keepHealth)
                            ev.Player.Health = originalHealth;

                        if (keepInventory)
                        {
                            if (CustomModules.Instance is not null)
                            {
                                foreach (object obj in originalItems)
                                    ev.Player.Cast<Pawn>().AddItem(obj);
                                
                                foreach (KeyValuePair<uint, ushort> kvp in originalCustomAmmo)
                                    ev.Player.Cast<Pawn>().SetAmmo(kvp.Key, kvp.Value);
                            }
                            else
                            {
                                foreach (object obj in originalItems)
                                {
                                    if (obj is Item item)
                                        ev.Player.AddItem(item);
                                }
                                
                                foreach (KeyValuePair<ItemType, ushort> kvp in originalAmmo)
                                    ev.Player.SetAmmo(kvp.Key.GetAmmoType(), kvp.Value);
                            }
                        }
                        
                        ev.Player.Position = ev.OutputPosition;
                        break;
                    }
                }
            }

            if (plugin.Config.Scp914EffectChances is not null && plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) &&
                (ev.Player.Role.Side != Side.Scp || !plugin.Config.ScpsImmuneTo914Effects))
            {
                IEnumerable<Scp914EffectChance> scp914EffectChances = plugin.Config.Scp914EffectChances[ev.KnobSetting];
                foreach ((EffectType effect, double chance, float duration) in scp914EffectChances)
                {
                    double r = plugin.Config.AdditiveProbabilities
                        ? plugin.Rng.NextDouble() * scp914EffectChances.Sum(x => x.Chance)
                        : plugin.Rng.NextDouble() * 100;

                    Log.DebugWithContext($"{ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.EnableEffect(effect, duration);
                        if (plugin.Config.Scp914EffectsExclusivity)
                            break;
                    }
                }
            }

            if (plugin.Config.Scp914TeleportChances is not null && plugin.Config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
            {
                IEnumerable<Scp914TeleportChance> scp914TeleportChances = plugin.Config.Scp914TeleportChances[ev.KnobSetting];

                foreach ((RoomType roomType, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in plugin.Config.Scp914TeleportChances[ev.KnobSetting])
                {
                    double r = plugin.Config.AdditiveProbabilities
                        ? plugin.Rng.NextDouble() * scp914TeleportChances.Sum(x => x.Chance)
                        : plugin.Rng.NextDouble() * 100;

                    Log.DebugWithContext($"{ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        if (zone != ZoneType.Unspecified)
                        {
                            ev.OutputPosition = Room.List.Where(x => x.Zone == zone && !ignoredRooms.Contains(x.Type)).Random().Position + ((Vector3.up * 1.5f) + offset);
                            if (damage > 0f)
                            {
                                float amount = ev.Player.MaxHealth * damage;
                                if (damage > 1f)
                                    amount = damage;

                                Log.DebugWithContext($"{ev.Player.Nickname} is being damaged for {amount}. -- {ev.Player.Health} * {damage}");
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

                                Log.DebugWithContext($"{ev.Player.Nickname} is being damaged for {amount}. -- {ev.Player.Health} * {damage}");
                                ev.Player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
                            }
                        }

                        break;
                    }
                }
            }
        }

        internal void UpgradeItem(Pickup oldItem, object newItem, Vector3 pos, int count)
        {
            Quaternion quaternion = oldItem.Rotation;
            Player previousOwner = oldItem.PreviousOwner;
            oldItem.Destroy();
            
            if (newItem is ItemType itemType && itemType is not ItemType.None)
            {
                for (int i = 0; i < count; i++)
                    Pickup.CreateAndSpawn(itemType, pos, quaternion, previousOwner);

                return;
            }

            if (CustomItem.TryGet(newItem, out CustomItem customItem))
            {
                for (int i = 0; i < count; i++)
                    customItem.Spawn(pos, previousOwner);
            }
        }
    }
}