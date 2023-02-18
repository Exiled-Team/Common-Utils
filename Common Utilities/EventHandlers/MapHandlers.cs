using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Scp914;
using PlayerRoles;

namespace Common_Utilities.EventHandlers
{
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs;
    using InventorySystem.Items.Firearms;
    using MEC;
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
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in _plugin.Config.Scp914ItemChanges[ev.KnobSetting])
                {
                    if (sourceItem != ev.Pickup.Type)
                        continue;

                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingItem)}: SCP-914 is trying to upgrade a {ev.Pickup.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        UpgradeItem(ev.Pickup, destinationItem, ev.Scp914.OutputChamber.position);
                        ev.IsAllowed = false;
                        break;
                    }
                }
            }
        }

        public void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (_plugin.Config.Scp914ItemChanges != null && _plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in _plugin.Config.Scp914ItemChanges[ev.KnobSetting])
                {
                    if (sourceItem != ev.Item.Type)
                        continue;

                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})");
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
            if (_plugin.Config.Scp914ClassChanges != null && _plugin.Config.Scp914ClassChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((RoleTypeId sourceRole, RoleTypeId destinationRole, int chance, bool keepInventory) in _plugin.Config.Scp914ClassChanges[ev.KnobSetting])
                {
                    if (sourceRole != ev.Player.Role)
                        continue;

                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role})is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        if (!keepInventory)
                            foreach (Item item in ev.Player.Items.ToList())
                            {
                                ev.Player.RemoveItem(item, false);
                                item.CreatePickup(ev.OutputPosition);
                            }

                        ev.Player.Role.Set(destinationRole, SpawnReason.Respawn);
                        ev.Player.ClearInventory();
                        ev.Player.Position = ev.OutputPosition;
                        break;
                    }
                }
            }

            if (_plugin.Config.Scp914EffectChances != null && _plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Role.Side != Side.Scp || !_plugin.Config.ScpsImmuneTo914Effects))
            {
                foreach ((EffectType effect, int chance, float duration) in _plugin.Config.Scp914EffectChances[ev.KnobSetting])
                {
                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r})");
                    if (r <= chance)
                    {
                        ev.Player.EnableEffect(effect, duration);
                        if (_plugin.Config.Scp914EffectsExclusivity)
                            break;
                    }
                }
            }

            if (_plugin.Config.Scp914TeleportChances != null && _plugin.Config.Scp914TeleportChances.ContainsKey(ev.KnobSetting))
            {
                foreach ((RoomType roomType, Vector3 offset, int chance, float damage, ZoneType zone) in _plugin.Config.Scp914TeleportChances[ev.KnobSetting])
                {
                    int r = _plugin.Rng.Next(100);
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

                        break;
                    }
                }
            }
        }

        internal void UpgradeItem(Pickup oldItem, ItemType newItem, Vector3 pos)
        {
            if (newItem != ItemType.None)
            {
                Item item = Item.Create(newItem);
                if (oldItem is Exiled.API.Features.Pickups.FirearmPickup oldFirearm && item is Firearm firearm)
                    firearm.Ammo = oldFirearm.Ammo <= firearm.MaxAmmo ? oldFirearm.Ammo : firearm.MaxAmmo;

                item.CreatePickup(pos);
            }

            oldItem.Destroy();
        }
    }
}