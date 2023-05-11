using PlayerRoles;

namespace Common_Utilities
{
    using System;
    using System.Collections.Generic;
    using ConfigObjects;
    using Configs;
    using EventHandlers;
    using Exiled.API.Enums;
    using MEC;
    using Exiled.API.Features;
    using HarmonyLib;
    using Scp914;
    using UnityEngine;
    using Server = Exiled.Events.Handlers.Server;
    using Player = Exiled.Events.Handlers.Player;
    using Random = System.Random;
    using Scp914 = Exiled.Events.Handlers.Scp914;

    public class Plugin : Plugin<Config>
    {
        public override string Name { get; } = "Common Utilities";
        public override string Author { get; } = "Joker119";
        public override Version RequiredExiledVersion { get; } = new(7, 0, 0);
        public override string Prefix { get; } = "CommonUtilities";
        public override PluginPriority Priority => PluginPriority.Higher;

        public PlayerHandlers PlayerHandlers;
        public ServerHandlers ServerHandlers;
        public MapHandlers MapHandlers;
        public Random Rng = new();
        public static Plugin Singleton;
        public string HarmonyName;
        public Harmony Instance;
        public List<CoroutineHandle> Coroutines { get; } = new();
        public Dictionary<Exiled.API.Features.Player, Tuple<int, Vector3>> AfkDict { get; } = new();

        public override void OnEnabled()
        {
            if (Config.Debug)
            {
                if (Config.StartingInventories != null)
                {
                    Log.Debug($"{Config.StartingInventories.Count}");
                    foreach (KeyValuePair<RoleTypeId, RoleInventory> inv in Config.StartingInventories)
                    {
                        for (int i = 0; i < inv.Value.UsedSlots; i++)
                        {
                            foreach (ItemChance chance in inv.Value[i])
                                Log.Debug(
                                    $"Inventory Config: {inv.Key} - Slot{i + 1}: {chance.ItemName} ({chance.Chance})");
                        }

                        foreach ((ItemType type, ushort amount, string group) in inv.Value.Ammo)
                        {
                            Log.Debug($"Ammo Config: {inv.Key} - {type} {amount} ({group})");
                        }
                    }
                }

                if (Config.Scp914ItemChanges != null)
                {
                    Log.Debug($"{Config.Scp914ItemChanges.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<ItemUpgradeChance>> upgrade in Config.Scp914ItemChanges)
                    {
                        foreach ((ItemType oldItem, ItemType newItem, double chance) in upgrade.Value)
                            Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem} - {chance}");
                    }
                }

                if (Config.Scp914ClassChanges != null)
                {
                    Log.Debug($"{Config.Scp914ClassChanges.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config
                        .Scp914ClassChanges)
                    {
                        foreach ((RoleTypeId oldRole, RoleTypeId newRole, double chance, RoleSpawnFlags spawnFlags) in upgrade.Value)
                            Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} RoleSpawnFlags: {spawnFlags}");
                    }
                }

                if (Config.Scp914EffectChances != null)
                {
                    Log.Debug($"{Config.Scp914EffectChances.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Config
                        .Scp914EffectChances)
                    {
                        foreach ((EffectType effect, double chance, float duration) in upgrade.Value)
                            Log.Debug($"914 Effect Config: {upgrade.Key}: {effect} + {duration} - {chance}");
                    }
                }

                if (Config.Scp914TeleportChances != null)
                {
                    Log.Debug($"{Config.Scp914TeleportChances.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Config.Scp914TeleportChances)
                    {
                        foreach ((RoomType room, Vector3 offset, double chance, float damage, ZoneType zone) in upgrade.Value)
                            Log.Debug($"914 Teleport Config: {upgrade.Key}: {room}/{zone} + {offset} - {chance} [{damage}]");
                    }
                }
            }

            Singleton = this;

            Log.Info($"Instantiating Events..");
            PlayerHandlers = new PlayerHandlers(this);
            ServerHandlers = new ServerHandlers(this);
            MapHandlers = new MapHandlers(this);
            
            Log.Info($"Registering EventHandlers..");
            Player.Died += PlayerHandlers.OnPlayerDied;
            Player.Jumping += PlayerHandlers.AntiAfkEventHandler;
            Player.Shooting += PlayerHandlers.AntiAfkEventHandler;
            Player.UsingItem += PlayerHandlers.AntiAfkEventHandler;
            Player.Hurting += PlayerHandlers.OnPlayerHurting;
            Player.Verified += PlayerHandlers.OnPlayerVerified;
            Player.MakingNoise += PlayerHandlers.AntiAfkEventHandler;
            Player.ReloadingWeapon += PlayerHandlers.AntiAfkEventHandler;
            Player.ChangingRole += PlayerHandlers.OnChangingRole;
            Player.Spawned += PlayerHandlers.OnSpawned;
            Player.ThrownProjectile += PlayerHandlers.AntiAfkEventHandler;
            Player.InteractingDoor += PlayerHandlers.OnInteractingDoor;
            Player.ProcessingHotkey += PlayerHandlers.AntiAfkEventHandler;
            Player.UsingRadioBattery += PlayerHandlers.OnUsingRadioBattery;
            Player.ChangingMoveState += PlayerHandlers.AntiAfkEventHandler;
            Player.InteractingElevator += PlayerHandlers.OnInteractingElevator;

            Server.RoundEnded += ServerHandlers.OnRoundEnded;
            Server.RoundStarted += ServerHandlers.OnRoundStarted;
            Server.RestartingRound += ServerHandlers.OnRestartingRound;
            Server.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;

            Scp914.UpgradingPickup += MapHandlers.OnScp914UpgradingItem;
            Scp914.UpgradingPlayer += MapHandlers.OnScp914UpgradingPlayer;
            Scp914.UpgradingInventoryItem += MapHandlers.OnScp914UpgradingInventoryItem;

            Exiled.Events.Handlers.Warhead.Starting += ServerHandlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping += ServerHandlers.OnWarheadStopping;

            HarmonyName = $"com-joker.cu-{DateTime.UtcNow.Ticks}";
            Instance = new Harmony(HarmonyName);
            Instance.PatchAll();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Died -= PlayerHandlers.OnPlayerDied;
            Player.Jumping -= PlayerHandlers.AntiAfkEventHandler;
            Player.Shooting -= PlayerHandlers.AntiAfkEventHandler;
            Player.UsingItem -= PlayerHandlers.AntiAfkEventHandler;
            Player.Hurting -= PlayerHandlers.OnPlayerHurting;
            Player.Verified -= PlayerHandlers.OnPlayerVerified;
            Player.MakingNoise -= PlayerHandlers.AntiAfkEventHandler;
            Player.ReloadingWeapon -= PlayerHandlers.AntiAfkEventHandler;
            Player.ChangingRole -= PlayerHandlers.OnChangingRole;
            Player.Spawned -= PlayerHandlers.OnSpawned;
            Player.ThrownProjectile -= PlayerHandlers.AntiAfkEventHandler;
            Player.InteractingDoor -= PlayerHandlers.OnInteractingDoor;
            Player.ProcessingHotkey -= PlayerHandlers.AntiAfkEventHandler;
            Player.UsingRadioBattery -= PlayerHandlers.OnUsingRadioBattery;
            Player.ChangingMoveState -= PlayerHandlers.AntiAfkEventHandler;
            Player.InteractingElevator -= PlayerHandlers.OnInteractingElevator;
            
            Server.RoundEnded -= ServerHandlers.OnRoundEnded;
            Server.RoundStarted -= ServerHandlers.OnRoundStarted;
            Server.RestartingRound -= ServerHandlers.OnRestartingRound;
            Server.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;

            Scp914.UpgradingPickup -= MapHandlers.OnScp914UpgradingItem;
            Scp914.UpgradingPlayer -= MapHandlers.OnScp914UpgradingPlayer;
            Scp914.UpgradingInventoryItem -= MapHandlers.OnScp914UpgradingInventoryItem;

            Exiled.Events.Handlers.Warhead.Starting -= ServerHandlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping -= ServerHandlers.OnWarheadStopping;
            
            Instance.UnpatchAll(HarmonyName);

            ServerHandlers = null;
            PlayerHandlers = null;
            MapHandlers = null;
            base.OnDisabled();
        }
    }
}