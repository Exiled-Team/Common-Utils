using Exiled.CustomItems.API.Features;

namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Exiled.Permissions.Features;
    using MEC;
    using PlayerStatsSystem;
    using Player = Exiled.API.Features.Player;
    
    public class PlayerHandlers
    {
        private readonly Plugin _plugin;
        public PlayerHandlers(Plugin plugin) => this._plugin = plugin;

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(_plugin.Config.JoinMessageDuration, message);
        }
        
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn($"{nameof(OnChangingRole)}: Triggering player is null.");
                return;
            }

            if (_plugin.Config.StartingInventories != null && _plugin.Config.StartingInventories.ContainsKey(ev.NewRole) && !ev.Lite && _plugin.Config.StartingInventories[ev.NewRole].CheckGroup(ev.Player.Group.BadgeText))
            {
                ev.Items.Clear();
                List<ItemType> items = StartItems(ev.NewRole, ev.Player);
                ev.Items.AddRange(items);
                if (ev.Reason == SpawnReason.Escaped)
                    Timing.CallDelayed(1f, () => ev.Player.ResetInventory(items));

                if (_plugin.Config.StartingInventories[ev.NewRole].Ammo != null && _plugin.Config.StartingInventories[ev.NewRole].Ammo.Count > 0)
                {
                    if (_plugin.Config.StartingInventories[ev.NewRole].Ammo.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(s.Group, out UserGroup userGroup) && userGroup == ev.Player.Group)))
                    {
                        Timing.CallDelayed(1f, () =>
                        {
                            ev.Ammo.Clear();
                            foreach ((ItemType type, ushort amount, string group) in _plugin.Config.StartingInventories[ev.NewRole].Ammo)
                            {
                                if (string.IsNullOrEmpty(group) || group == "none" || (ServerStatic.PermissionsHandler._groups.TryGetValue(group, out UserGroup userGroup) && userGroup == ev.Player.Group))
                                {
                                    ev.Ammo.Add(type, amount);
                                }
                            }
                        });
                    }
                }
            }

            if (_plugin.Config.HealthValues != null && _plugin.Config.HealthValues.ContainsKey(ev.NewRole))
                Timing.CallDelayed(2.5f, () =>
                {
                    ev.Player.Health = _plugin.Config.HealthValues[ev.NewRole];
                    ev.Player.MaxHealth = _plugin.Config.HealthValues[ev.NewRole];
                });

            if (ev.NewRole != RoleType.Spectator && _plugin.Config.PlayerHealthInfo)
            {
                Timing.CallDelayed(1f, () =>
                    ev.Player.CustomInfo = $"{ev.Player.Health}/{ev.Player.MaxHealth}");
            }
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Killer != null && _plugin.Config.HealthOnKill != null && _plugin.Config.HealthOnKill.ContainsKey(ev.Killer.Role))
            {

                if (ev.Killer.Health + _plugin.Config.HealthOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)
                    ev.Killer.Health += _plugin.Config.HealthOnKill[ev.Killer.Role];
                else
                    ev.Killer.Health = ev.Killer.MaxHealth;
            }
        }

        public List<ItemType> StartItems(RoleType role, Player player = null)
        {
            List<ItemType> items = new List<ItemType>();

            for (int i = 0; i < _plugin.Config.StartingInventories[role].UsedSlots; i++)
            {
                int r = _plugin.Rng.Next(100);
                foreach ((string item, int chance, string groupKey) in _plugin.Config.StartingInventories[role][i])
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
                        else if (CustomItem.TryGet(item, out CustomItem customItem))
                        {
                            if (player != null)
                                Timing.CallDelayed(0.5f, () => customItem.Give(player));
                            else
                                Log.Warn($"{nameof(StartItems)}: Tried to give {customItem.Name} to a null player.");
                            
                            break;
                        }
                        else
                            Log.Warn($"{nameof(StartItems)}: {item} is not a valid ItemType or it is a CustomItem that is not installed! It is being skipper in inventory decisions.");
                    }
                }
            }

            return items;
        }

        private string FormatJoinMessage(Player player) => 
            string.IsNullOrEmpty(_plugin.Config.JoinMessage) ? string.Empty : _plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");

        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (_plugin.Config.RoleDamageMultipliers != null && ev.Attacker != null && _plugin.Config.RoleDamageMultipliers.ContainsKey(ev.Attacker.Role))
                ev.Amount *= _plugin.Config.RoleDamageMultipliers[ev.Attacker.Role];

            if (_plugin.Config.WeaponDamageMultipliers != null)
            {
                ItemType type = ItemType.None;
                if (ev.DamageHandler is ExplosionDamageHandler)
                    type = ItemType.GrenadeHE;
                else if (ev.DamageHandler is MicroHidDamageHandler)
                    type = ItemType.MicroHID;
                else if (ev.DamageHandler is Scp018DamageHandler)
                    type = ItemType.SCP018;
                else if (ev.DamageHandler is FirearmDamageHandler firearmDamageHandler)
                    type = firearmDamageHandler.WeaponType;

                if (type != ItemType.None)
                {
                    if (_plugin.Config.WeaponDamageMultipliers.ContainsKey(type) && ev.Attacker.CurrentItem.Type == type) 
                        ev.Amount *= _plugin.Config.WeaponDamageMultipliers[type];
                }
            }

            if (_plugin.Config.PlayerHealthInfo)
                Timing.CallDelayed(0.5f, () =>
                    ev.Target.CustomInfo = $"{ev.Target.Health}/{ev.Target.MaxHealth}");
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.IsCuffed && _plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (ev.Player.IsCuffed && _plugin.Config.RestrictiveDisarming)
                ev.IsAllowed = false;
        }
    }
}