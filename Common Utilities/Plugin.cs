namespace Common_Utilities
{
    using System;
    using System.Collections.Generic;
    using Common_Utilities.ConfigObjects;
    using Common_Utilities.Configs;
    using Common_Utilities.EventHandlers;
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
        public override Version RequiredExiledVersion { get; } = new(5, 2, 1);
        public override string Prefix { get; } = "CommonUtilities";
        
        public PlayerHandlers PlayerHandlers;
        public ServerHandlers ServerHandlers;
        public MapHandlers MapHandlers;
        public Random Rng = new();
        public static Plugin Singleton;
        public string HarmonyName;
        public Harmony Instance;
        public List<CoroutineHandle> Coroutines { get; } = new();
        public Dictionary<Exiled.API.Features.Player, int> AfkDict { get; } = new();

        public override void OnEnabled()
        {
            if (Config.Debug)
            {
                if (Config.StartingInventories != null)
                {
                    Log.Debug($"{Config.StartingInventories.Count}");
                    foreach (KeyValuePair<RoleType, RoleInventory> inv in Config.StartingInventories)
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
                        foreach ((ItemType oldItem, ItemType newItem, int chance) in upgrade.Value)
                            Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem} - {chance}");
                    }
                }

                if (Config.Scp914ClassChanges != null)
                {
                    Log.Debug($"{Config.Scp914ClassChanges.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config
                        .Scp914ClassChanges)
                    {
                        foreach ((RoleType oldRole, RoleType newRole, int chance, bool keepInventory) in upgrade.Value)
                            Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} Keep Inventory: {keepInventory}");
                    }
                }

                if (Config.Scp914EffectChances != null)
                {
                    Log.Debug($"{Config.Scp914EffectChances.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Config
                        .Scp914EffectChances)
                    {
                        foreach ((EffectType effect, int chance, float duration) in upgrade.Value)
                            Log.Debug($"914 Effect Config: {upgrade.Key}: {effect} + {duration} - {chance}");
                    }
                }

                if (Config.Scp914TeleportChances != null)
                {
                    Log.Debug($"{Config.Scp914TeleportChances.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Config.Scp914TeleportChances)
                    {
                        foreach ((RoomType room, Vector3 offset, int chance, float damage, ZoneType zone) in upgrade.Value)
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
            Player.Jumping += PlayerHandlers.OnJumping;
            Player.Shooting += PlayerHandlers.OnShooting;
            Player.UsingItem += PlayerHandlers.OnUsingItem;
            Player.Hurting += PlayerHandlers.OnPlayerHurting;
            Player.Verified += PlayerHandlers.OnPlayerVerified;
            Player.MakingNoise += PlayerHandlers.OnMakingNoise;
            Player.ReloadingWeapon += PlayerHandlers.OnReloading;
            Player.ChangingRole += PlayerHandlers.OnChangingRole;
            Player.ThrowingItem += PlayerHandlers.OnThrowingItem;
            Player.InteractingDoor += PlayerHandlers.OnInteractingDoor;
            Player.UsingRadioBattery += PlayerHandlers.OnUsingRadioBattery;
            Player.InteractingElevator += PlayerHandlers.OnInteractingElevator;
            
            Server.RoundEnded += ServerHandlers.OnRoundEnded;
            Server.RoundStarted += ServerHandlers.OnRoundStarted;
            Server.RestartingRound += ServerHandlers.OnRestartingRound;
            Server.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;

            Scp914.UpgradingItem += MapHandlers.OnScp914UpgradingItem;
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
            Player.Jumping -= PlayerHandlers.OnJumping;
            Player.Shooting -= PlayerHandlers.OnShooting;
            Player.UsingItem -= PlayerHandlers.OnUsingItem;
            Player.Hurting -= PlayerHandlers.OnPlayerHurting;
            Player.Verified -= PlayerHandlers.OnPlayerVerified;
            Player.MakingNoise -= PlayerHandlers.OnMakingNoise;
            Player.ReloadingWeapon -= PlayerHandlers.OnReloading;
            Player.ChangingRole -= PlayerHandlers.OnChangingRole;
            Player.ThrowingItem -= PlayerHandlers.OnThrowingItem;
            Player.InteractingDoor -= PlayerHandlers.OnInteractingDoor;
            Player.UsingRadioBattery -= PlayerHandlers.OnUsingRadioBattery;
            Player.InteractingElevator -= PlayerHandlers.OnInteractingElevator;
            
            Server.RoundEnded -= ServerHandlers.OnRoundEnded;
            Server.RoundStarted -= ServerHandlers.OnRoundStarted;
            Server.RestartingRound -= ServerHandlers.OnRestartingRound;
            Server.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;

            Scp914.UpgradingItem -= MapHandlers.OnScp914UpgradingItem;
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