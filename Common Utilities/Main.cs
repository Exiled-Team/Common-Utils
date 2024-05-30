namespace Common_Utilities
{
#pragma warning disable SA1401 // Fields should be private
    using System;
    using System.Collections.Generic;

    using ConfigObjects;
    using Configs;
    using EventHandlers;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using HarmonyLib;
    using MEC;
    using PlayerRoles;
    using Scp914;
    using UnityEngine;

    using Player = Exiled.Events.Handlers.Player;
    using Random = System.Random;
    using Scp914 = Exiled.Events.Handlers.Scp914;
    using Server = Exiled.Events.Handlers.Server;

    public class Main : Plugin<Config>
    {
        public static Main Instance;
        public PlayerHandlers PlayerHandlers;
        public ServerHandlers ServerHandlers;
        public MapHandlers MapHandlers;
        public Random Rng = new();
        public Harmony Harmony;
        public string HarmonyName;

        public override string Name { get; } = "Common Utilities";

        public override string Author { get; } = "Exiled-Team";

        public override Version Version { get; } = new(7, 1, 0);

        public override Version RequiredExiledVersion { get; } = new(8, 5, 0);

        public override string Prefix { get; } = "CommonUtilities";

        public override PluginPriority Priority => PluginPriority.Higher;

        public List<CoroutineHandle> Coroutines { get; } = new();

        public Dictionary<Exiled.API.Features.Player, Tuple<int, Vector3>> AfkDict { get; } = new();

        public override void OnEnabled()
        {
            if (Config.Debug)
                DebugConfig();

            Instance = this;

            Log.Info($"Instantiating Events..");
            PlayerHandlers = new PlayerHandlers(this);
            ServerHandlers = new ServerHandlers(this);
            MapHandlers = new MapHandlers(this);
            
            Log.Info($"Registering EventHandlers..");
            if (Config.HealthOnKill != null)
                Player.Died += PlayerHandlers.OnPlayerDied;
            Player.Hurting += PlayerHandlers.OnPlayerHurting;
            Player.Verified += PlayerHandlers.OnPlayerVerified;
            if (Config.StartingInventories != null)
                Player.ChangingRole += PlayerHandlers.OnChangingRole;
            Player.Spawned += PlayerHandlers.OnSpawned;
            Player.InteractingDoor += PlayerHandlers.OnInteractingDoor;
            if (Config.RadioBatteryDrainMultiplier is not 1)
                Player.UsingRadioBattery += PlayerHandlers.OnUsingRadioBattery;
            Player.InteractingElevator += PlayerHandlers.OnInteractingElevator;
            if (Config.DisarmSwitchTeams)
                Player.Escaping += PlayerHandlers.OnEscaping;
            if (Config.AfkLimit > 0)
            {
                Player.Jumping += PlayerHandlers.AntiAfkEventHandler;
                Player.Shooting += PlayerHandlers.AntiAfkEventHandler;
                Player.UsingItem += PlayerHandlers.AntiAfkEventHandler;
                Player.MakingNoise += PlayerHandlers.AntiAfkEventHandler;
                Player.ReloadingWeapon += PlayerHandlers.AntiAfkEventHandler;
                Player.ThrownProjectile += PlayerHandlers.AntiAfkEventHandler;
                Player.ChangingMoveState += PlayerHandlers.AntiAfkEventHandler;
            }

            Server.RoundEnded += ServerHandlers.OnRoundEnded;
            Server.RoundStarted += ServerHandlers.OnRoundStarted;
            Server.RestartingRound += ServerHandlers.OnRestartingRound;
            Server.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;

            if (Config.Scp914ItemChanges != null)
                Scp914.UpgradingPickup += MapHandlers.OnScp914UpgradingItem;
            if (Config.Scp914ItemChanges != null)
                Scp914.UpgradingInventoryItem += MapHandlers.OnScp914UpgradingInventoryItem;
            Scp914.UpgradingPlayer += MapHandlers.OnScp914UpgradingPlayer;

            Exiled.Events.Handlers.Warhead.Starting += ServerHandlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping += ServerHandlers.OnWarheadStopping;

            HarmonyName = $"com-joker.cu-{DateTime.UtcNow.Ticks}";
            Harmony = new Harmony(HarmonyName);
            Harmony.PatchAll();

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
            Player.UsingRadioBattery -= PlayerHandlers.OnUsingRadioBattery;
            Player.ChangingMoveState -= PlayerHandlers.AntiAfkEventHandler;
            Player.InteractingElevator -= PlayerHandlers.OnInteractingElevator;
            Player.Escaping -= PlayerHandlers.OnEscaping;

            Server.RoundEnded -= ServerHandlers.OnRoundEnded;
            Server.RoundStarted -= ServerHandlers.OnRoundStarted;
            Server.RestartingRound -= ServerHandlers.OnRestartingRound;
            Server.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;

            Scp914.UpgradingPickup -= MapHandlers.OnScp914UpgradingItem;
            Scp914.UpgradingPlayer -= MapHandlers.OnScp914UpgradingPlayer;
            Scp914.UpgradingInventoryItem -= MapHandlers.OnScp914UpgradingInventoryItem;

            Exiled.Events.Handlers.Warhead.Starting -= ServerHandlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping -= ServerHandlers.OnWarheadStopping;
            
            Harmony.UnpatchAll(HarmonyName);

            ServerHandlers = null;
            PlayerHandlers = null;
            MapHandlers = null;
            base.OnDisabled();
        }

        public void DebugConfig()
        {
            if (Config.StartingInventories != null)
            {
                Log.Debug($"{Config.StartingInventories.Count}");
                foreach (KeyValuePair<RoleTypeId, RoleInventory> inv in Config.StartingInventories)
                {
                    for (int i = 0; i < inv.Value.UsedSlots; i++)
                    {
                        foreach (ItemChance chance in inv.Value[i])
                        {
                            Log.Debug($"Inventory Config: {inv.Key} - Slot{i + 1}: {chance.ItemName} ({chance.Chance})");
                        }
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
                    foreach ((ItemType oldItem, ItemType newItem, double chance, int count) in upgrade.Value)
                        Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem}x({count}) - {chance}");
                }
            }

            if (Config.Scp914ClassChanges != null)
            {
                Log.Debug($"{Config.Scp914ClassChanges.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config.Scp914ClassChanges)
                {
                    foreach ((RoleTypeId oldRole, string newRole, double chance, bool keepInventory, bool keepHealth) in upgrade.Value)
                        Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} keepInventory: {keepInventory} keepHealth: {keepHealth}");
                }
            }

            if (Config.Scp914EffectChances != null)
            {
                Log.Debug($"{Config.Scp914EffectChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Config.Scp914EffectChances)
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
                    foreach ((RoomType room, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in upgrade.Value)
                    {
                        Log.Debug($"914 Teleport Config: {upgrade.Key}: {room}/{zone} + {offset} - {chance} [{damage}]");
                        Log.Debug("Ignored rooms:");
                        if (ignoredRooms != null)
                        {
                            foreach (RoomType roomType in ignoredRooms)
                            {
                                Log.Debug(roomType);
                            }
                        }
                    }
                }
            }
        }
    }
}