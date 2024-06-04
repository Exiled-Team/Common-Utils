namespace Common_Utilities.EventHandlers;

using Exiled.CustomItems.API.Features;
using Exiled.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using ConfigObjects;
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
    private readonly Config config;

    public MapHandlers(Plugin plugin) => config = plugin.Config;
        
    public void OnUpgradingPickup(UpgradingPickupEventArgs ev)
    {
        if (config.Scp914ItemChances.TryGetValue(ev.KnobSetting, out List<ItemUpgradeChance> itemUpgradeChances))
        {
            var itemUpgradeChance = (List<ItemUpgradeChance>)itemUpgradeChances.Where(x => x.OriginalItem == ev.Pickup.Type.ToString() || (CustomItem.TryGet(ev.Pickup, out CustomItem item) && item!.Name == x.OriginalItem));

            double rolledChance = API.RollChance(itemUpgradeChance);

            foreach ((string sourceItem, string destinationItem, double chance, int count) in itemUpgradeChance)
            {
                Log.Debug($"{nameof(OnUpgradingPickup)}: SCP-914 is trying to upgrade a {ev.Pickup.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {rolledChance <= chance} ({rolledChance})");

                if (rolledChance <= chance)
                {
                    if (Enum.TryParse(destinationItem, out ItemType itemType))
                    {
                        if (itemType is not ItemType.None)
                        {
                            UpgradePickup(ev.Pickup, ev.OutputPosition, count, false, itemType: itemType);
                        }
                    }
                    else if (CustomItem.TryGet(destinationItem, out CustomItem customItem))
                    {
                        if (customItem is not null)
                        {
                            UpgradePickup(ev.Pickup, ev.OutputPosition, count, true, customItem: customItem);
                        }
                    }
                    
                    ev.Pickup.Destroy();
                    ev.IsAllowed = false;
                    break;
                }

                if (config.AdditiveProbabilities)
                    rolledChance -= chance;
            }
        }
    }

    public void OnUpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
    {
        if (config.Scp914ItemChances.TryGetValue(ev.KnobSetting, out List<ItemUpgradeChance> itemUpgradeChances))
        {
            var itemUpgradeChance = (List<ItemUpgradeChance>)itemUpgradeChances.Where(x => x.OriginalItem == ev.Item.Type.ToString() || (CustomItem.TryGet(ev.Item, out CustomItem item) && item!.Name == x.OriginalItem));
            
            double rolledChance = API.RollChance(itemUpgradeChance);

            foreach ((string sourceItem, string destinationItem, double chance, int count) in itemUpgradeChance)
            {
                Log.Debug($"{nameof(OnUpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {rolledChance <= chance} ({rolledChance})");
             
                if (rolledChance <= chance)
                {
                    if (Enum.TryParse(destinationItem, out ItemType itemType))
                    {
                        if (itemType is not ItemType.None)
                        {
                            UpgradeInventoryItem(ev, count, false, itemType: itemType);
                        }
                    }
                    else if (CustomItem.TryGet(destinationItem, out CustomItem customItem))
                    {
                        if (customItem is not null)
                        {
                            UpgradeInventoryItem(ev, count, true, customItem: customItem);
                        }
                    }

                    break;
                }

                if (config.AdditiveProbabilities)
                    rolledChance -= chance;
            }
        }
    }

    public void OnScp914UpgradingPlayer(UpgradingPlayerEventArgs ev)
    {
        if (config.Scp914ClassChanges != null && config.Scp914ClassChanges.TryGetValue(ev.KnobSetting, out var change))
        {
            var playerUpgradeChance = (List<PlayerUpgradeChance>)change.Where(
                x => x.OriginalRole == ev.Player.Role.ToString() 
                || (CustomRole.TryGet(ev.Player, out IReadOnlyCollection<CustomRole> customRoles) && customRoles.Select(r => r.Name).Contains(x.OriginalRole)));

            double rolledChance = API.RollChance(playerUpgradeChance);

            foreach ((string sourceRole, string destinationRole, double chance, bool keepInventory, bool keepHealth) in playerUpgradeChance)
            {
                Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role}) is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {rolledChance <= chance} ({rolledChance})");
                if (rolledChance <= chance)
                {
                    float originalHealth = ev.Player.Health;
                    var originalItems = ev.Player.Items;
                    var originalAmmo = ev.Player.Ammo;
                        
                    if (Enum.TryParse(destinationRole,  out RoleTypeId roleType))
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

                    if (keepHealth)
                    {
                        ev.Player.Health = originalHealth;
                    }

                    if (keepInventory)
                    {
                        foreach (var item in originalItems)
                        {
                            ev.Player.AddItem(item);
                        }

                        foreach (var kvp in originalAmmo)
                        {
                            ev.Player.SetAmmo(kvp.Key.GetAmmoType(), kvp.Value);
                        }
                    }
                        
                    ev.Player.Position = ev.OutputPosition;
                    break;
                }

                if (config.AdditiveProbabilities)
                    rolledChance -= chance;
            }
        }

        if (config.Scp914EffectChances != null && config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Role.Side != Side.Scp || !config.ScpsImmuneTo914Effects))
        {
            IEnumerable<Scp914EffectChance> scp914EffectChances = config.Scp914EffectChances[ev.KnobSetting];
            
            double rolledChance = API.RollChance(scp914EffectChances);

            foreach ((EffectType effect, double chance, float duration) in scp914EffectChances)
            {
                Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect through SCP-914. {effect} ({chance}). Should be added: {rolledChance <= chance} ({rolledChance})");
                
                if (rolledChance <= chance)
                {
                    ev.Player.EnableEffect(effect, duration);
                    if (config.Scp914EffectsExclusivity)
                        break;
                }

                if (config.AdditiveProbabilities) 
                    rolledChance -= chance;
            }
        }

        if (config.Scp914TeleportChances != null && config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
        {
            IEnumerable<Scp914TeleportChance> scp914TeleportChances = config.Scp914TeleportChances[ev.KnobSetting];

            double rolledChance = API.RollChance(scp914TeleportChances);

            foreach ((RoomType roomType, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in config.Scp914TeleportChances[ev.KnobSetting])
            {
                Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {rolledChance <= chance} ({rolledChance})");

                if (rolledChance <= chance)
                {
                    ev.OutputPosition = ChoosePosition(zone, ignoredRooms, offset, roomType);
                    
                    DealDamage(ev.Player, damage);

                    break;
                }

                if (config.AdditiveProbabilities)
                    rolledChance -= chance;
            }
        }
    }

    private Vector3 ChoosePosition(ZoneType zone, List<RoomType> ignoredRooms, Vector3 offset, RoomType roomType)
    {
        Vector3 pos1 = Room.List.Where(x => x.Zone == zone && !ignoredRooms.Contains(x.Type)).GetRandomValue().Position + ((Vector3.up * 1.5f) + offset);
        Vector3 pos2 = Room.Get(roomType).Position + (Vector3.up * 1.5f) + offset;
       
        return zone != ZoneType.Unspecified ? pos1 : pos2;
    }

    private void DealDamage(Player player, float damage)
    {
        if (damage > 0f)
        {
            float amount = player.MaxHealth * damage;
            if (damage > 1f)
                amount = damage;

            Log.Debug(
                $"{nameof(OnScp914UpgradingPlayer)}: {player.Nickname} is being damaged for {amount}. -- {player.Health} * {damage}");
            player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
        }
    }

    private void UpgradePickup(Pickup oldItem, Vector3 outputPos, int count, bool isCustomItem, ItemType itemType = ItemType.None, CustomItem customItem = null)
    {
        if (!isCustomItem)
        {
            Quaternion quaternion = oldItem.Rotation;
            Player previousOwner = oldItem.PreviousOwner;
            for (int i = 0; i < count; i++)
            {
                Pickup.CreateAndSpawn(itemType, outputPos, quaternion, previousOwner);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                customItem!.Spawn(outputPos, oldItem.PreviousOwner);
            }
        }
    }
    
    private void UpgradeInventoryItem(UpgradingInventoryItemEventArgs ev, int count, bool isCustomItem, ItemType itemType = ItemType.None, CustomItem customItem = null)
    {
        ev.Player.RemoveItem(ev.Item);
        if (!isCustomItem)
        {
            for (int i = 0; i < count; i++)
            {
                if (!ev.Player.IsInventoryFull)
                    ev.Player.AddItem(itemType);
                else
                    Pickup.CreateAndSpawn(itemType, Scp914.OutputPosition, ev.Player.Rotation, ev.Player);
            }   
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (!ev.Player.IsInventoryFull)
                    customItem!.Give(ev.Player);
                else
                    customItem!.Spawn(Scp914.OutputPosition, ev.Player);
            }
        }
    }
}