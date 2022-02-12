using Exiled.API.Features;

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
        public MapHandlers(Plugin plugin) => this._plugin = plugin;
        
        public void OnScp914UpgradingItem(UpgradingItemEventArgs ev)
        {
            if (_plugin.Config.Scp914ItemChanges != null && _plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in _plugin.Config.Scp914ItemChanges[ev.KnobSetting])
                {
                    if (sourceItem != ev.Item.Type)
                        continue;

                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingItem)}: SCP-914 is trying to upgrade a {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})", _plugin.Config.Debug);
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
            if (_plugin.Config.Scp914ItemChanges != null && _plugin.Config.Scp914ItemChanges.ContainsKey(ev.KnobSetting))
            {
                foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in _plugin.Config.Scp914ItemChanges[ev.KnobSetting])
                {
                    if (sourceItem != ev.Item.Type)
                        continue;

                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingInventoryItem)}: {ev.Player.Nickname} is attempting to upgrade hit {ev.Item.Type}. {sourceItem} -> {destinationItem} ({chance}). Should process: {r <= chance} ({r})", _plugin.Config.Debug);
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
                foreach ((RoleType sourceRole, RoleType destinationRole, int chance, bool keepInventory) in _plugin.Config.Scp914ClassChanges[ev.KnobSetting])
                {
                    if (sourceRole != ev.Player.Role || (destinationRole == RoleType.Scp106 && OneOhSixContainer.used))
                        continue;

                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} ({ev.Player.Role})is trying to upgrade his class. {sourceRole} -> {destinationRole} ({chance}). Should be processed: {r <= chance} ({r})", _plugin.Config.Debug);
                    if (r <= chance)
                    {
                        if (!keepInventory)
                            foreach (Item item in ev.Player.Items.ToList())
                            {
                                ev.Player.RemoveItem(item, false);
                                item.Spawn(ev.OutputPosition);
                            }
                        
                        ev.Player.SetRole(destinationRole, SpawnReason.ForceClass, keepInventory);
                        Timing.CallDelayed(0.45f, () => ev.Player.Position = ev.OutputPosition);
                        break;
                    }
                }
            }

            if (_plugin.Config.Scp914EffectChances != null && _plugin.Config.Scp914EffectChances.ContainsKey(ev.KnobSetting) && (ev.Player.Role.Side != Side.Scp || !_plugin.Config.ScpsImmuneTo914Effects))
            {
                foreach ((EffectType effect, int chance, float duration) in _plugin.Config.Scp914EffectChances[ev.KnobSetting])
                {
                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to gain an effect. {effect} ({chance}). Should be added: {r <= chance} ({r})", _plugin.Config.Debug);
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
                foreach ((RoomType roomType, Vector3 offset, int chance, float damage) in _plugin.Config.Scp914TeleportChances[ev.KnobSetting])
                {
                    int r = _plugin.Rng.Next(100);
                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is trying to be teleported by 914. {roomType} + {offset} ({chance}). Should be teleported: {r <= chance} ({r})", _plugin.Config.Debug);
                    if (r <= chance)
                    {
                        foreach (Room room in Map.Rooms)
                            if (room.Type == roomType)
                            {
                                ev.OutputPosition = (room.Position + (Vector3.up * 1.5f)) + offset;
                                if (damage > 0f)
                                {
                                    float amount = ev.Player.MaxHealth * damage;
                                    if (damage > 1f) 
                                        amount = damage;

                                    Log.Debug($"{nameof(OnScp914UpgradingPlayer)}: {ev.Player.Nickname} is being damaged for {amount}. -- {ev.Player.Health} * {damage}", _plugin.Config.Debug);
                                    ev.Player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
                                }
                                break;
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