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

    /// <inheritdoc/>
    public class Main : Plugin<Config>
    {
        /// <summary>
        /// Gets the <see cref="Main"/> instance.
        /// </summary>
        public static Main Instance { get; private set; }

        /// <inheritdoc/>
        public override string Name { get; } = "Common Utilities";

        /// <inheritdoc/>
        public override string Author { get; } = "Exiled-Team";

        /// <inheritdoc/>
        public override Version Version { get; } = new(8, 0, 0);

        /// <inheritdoc/>
        public override Version RequiredExiledVersion { get; } = new(9, 0, 0);

        /// <inheritdoc/>
        public override string Prefix { get; } = "CommonUtilities";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Higher;

        /// <summary>
        /// Gets an instance of <see cref="Random"/> used for generating random values throughout the game.
        /// </summary>
        public Random Rng { get; } = new();

        /// <summary>
        /// Gets the handler responsible for managing player-related events and actions.
        /// </summary>
        public PlayerHandlers PlayerHandlers { get; private set; }

        /// <summary>
        /// Gets the handler responsible for managing server-related events and actions.
        /// </summary>
        public ServerHandlers ServerHandlers { get; private set; }

        /// <summary>
        /// Gets the handler responsible for managing map-related events and actions.
        /// </summary>
        public MapHandlers MapHandlers { get; private set; }

        /// <summary>
        /// Gets the <see cref="Harmony"/> instance used for patching game methods.
        /// </summary>
        public Harmony Harmony { get; private set; }

        /// <summary>
        /// Gets the unique name assigned to this <see cref="Harmony"/> instance.
        /// </summary>
        public string HarmonyName { get; private set; }
        
        internal List<CoroutineHandle> Coroutines { get; } = new();

        internal Dictionary<Exiled.API.Features.Player, Tuple<int, Vector3>> AfkDict { get; } = new();

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            if (Config.Debug)
                DebugConfig();

            Instance = this;

            HarmonyName = $"com-joker.cu-{DateTime.UtcNow.Ticks}";
            Harmony = new Harmony(HarmonyName);
            Harmony.PatchAll();

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Harmony.UnpatchAll(HarmonyName);
            
            base.OnDisabled();
        }
        
        /// <summary>
        /// Validates configuration by sending logs.
        /// </summary>
        public void DebugConfig()
        {
            if (Config.StartingInventories is not null)
            {
                Log.Debug($"{Config.StartingInventories.Count}");
                foreach (KeyValuePair<RoleTypeId, RoleInventory> inv in Config.StartingInventories)
                {
                    for (int i = 0; i < inv.Value.UsedSlots; i++)
                    {
                        foreach (ItemChance chance in inv.Value[i])
                            Log.Debug($"Inventory Config: {inv.Key} - Slot{i + 1}: {chance.Item} ({chance.Chance})");
                    }

                    foreach ((ItemType type, ushort amount, string group) in inv.Value.Ammo)
                        Log.Debug($"Ammo Config: {inv.Key} - {type} {amount} ({group})");
                }
            }

            if (Config.Scp914ItemChanges is not null)
            {
                Log.Debug($"{Config.Scp914ItemChanges.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<ItemUpgradeChance>> upgrade in Config.Scp914ItemChanges)
                {
                    foreach ((object oldItem, object newItem, double chance, int count) in upgrade.Value)
                        Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem}x({count}) - {chance}");
                }
            }

            if (Config.Scp914ClassChanges is not null)
            {
                Log.Debug($"{Config.Scp914ClassChanges.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config.Scp914ClassChanges)
                {
                    foreach ((object oldRole, object newRole, double chance, bool keepInventory, bool keepHealth) in upgrade.Value)
                        Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} keepInventory: {keepInventory} keepHealth: {keepHealth}");
                }
            }

            if (Config.Scp914EffectChances is not null)
            {
                Log.Debug($"{Config.Scp914EffectChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Config.Scp914EffectChances)
                {
                    foreach ((EffectType effect, double chance, float duration) in upgrade.Value)
                        Log.Debug($"914 Effect Config: {upgrade.Key}: {effect} + {duration} - {chance}");
                }
            }

            if (Config.Scp914TeleportChances is not null)
            {
                Log.Debug($"{Config.Scp914TeleportChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Config.Scp914TeleportChances)
                {
                    foreach ((RoomType room, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in upgrade.Value)
                    {
                        Log.Debug($"914 Teleport Config: {upgrade.Key}: {room}/{zone} + {offset} - {chance} [{damage}]");
                        Log.Debug("Ignored rooms:");
                        if (ignoredRooms is not null)
                        {
                            foreach (RoomType roomType in ignoredRooms)
                                Log.Debug(roomType);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();
            
            PlayerHandlers = new PlayerHandlers(this);
            ServerHandlers = new ServerHandlers(this);
            MapHandlers = new MapHandlers(this);
            
            if (Config.HealthOnKill is not null)
                Player.Died += PlayerHandlers.OnPlayerDied;
            
            Player.Hurting += PlayerHandlers.OnPlayerHurting;
            Player.Verified += PlayerHandlers.OnPlayerVerified;
            
            if (Config.StartingInventories is not null)
                Player.ChangingRole += PlayerHandlers.OnChangingRole;
            
            Player.ChangedRole += PlayerHandlers.OnChangedRole;
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

            if (Config.Scp914ItemChanges is not null)
                Scp914.UpgradingPickup += MapHandlers.OnScp914UpgradingItem;
            
            if (Config.Scp914ItemChanges is not null)
                Scp914.UpgradingInventoryItem += MapHandlers.OnScp914UpgradingInventoryItem;
            
            Scp914.UpgradingPlayer += MapHandlers.OnScp914UpgradingPlayer;

            Exiled.Events.Handlers.Warhead.Starting += ServerHandlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Stopping += ServerHandlers.OnWarheadStopping;
        }

        /// <inheritdoc/>
        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            
            Player.Died -= PlayerHandlers.OnPlayerDied;
            Player.Jumping -= PlayerHandlers.AntiAfkEventHandler;
            Player.Shooting -= PlayerHandlers.AntiAfkEventHandler;
            Player.UsingItem -= PlayerHandlers.AntiAfkEventHandler;
            Player.Hurting -= PlayerHandlers.OnPlayerHurting;
            Player.Verified -= PlayerHandlers.OnPlayerVerified;
            Player.MakingNoise -= PlayerHandlers.AntiAfkEventHandler;
            Player.ReloadingWeapon -= PlayerHandlers.AntiAfkEventHandler;
            Player.ChangingRole -= PlayerHandlers.OnChangingRole;
            Player.ChangedRole -= PlayerHandlers.OnChangedRole;
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
            
            ServerHandlers = null;
            PlayerHandlers = null;
            MapHandlers = null;
        }
    }
}