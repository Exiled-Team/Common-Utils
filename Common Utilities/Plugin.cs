using HarmonyLib;

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
    using Scp914;
    using UnityEngine;
    using Server = Exiled.Events.Handlers.Server;
    using Player = Exiled.Events.Handlers.Player;
    using Random = System.Random;
    using Scp914 = Exiled.Events.Handlers.Scp914;

    public class Plugin : Exiled.API.Features.Plugin<Config>
    {
        public override string Name { get; } = "Common Utilities";
        public override string Author { get; } = "Galaxy119";
        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override string Prefix { get; } = "CommonUtilities";
        
        public PlayerHandlers PlayerHandlers;
        public ServerHandlers ServerHandlers;
        public MapHandlers MapHandlers;
        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        public Random Rng = new Random();
        public static Plugin Singleton;
        public Harmony Instance;

        public override void OnEnabled()
        {
            if (Config.Debug)
            {
                Log.Debug($"{Config.StartingInventories.Count}");
                foreach (KeyValuePair<RoleType, RoleInventory> inv in Config.StartingInventories)
                {
                    for (int i = 0; i < inv.Value.UsedSlots; i++)
                    {
                        foreach (ItemChance chance in inv.Value[i])
                            Log.Debug($"Inventory Config: {inv.Key} - Slot{i + 1}: {chance.ItemName} ({chance.Chance})");
                    }

                    foreach ((ItemType type, ushort amount, string group) in inv.Value.Ammo)
                    {
                        Log.Debug($"Ammo Config: {inv.Key} - {type} {amount} ({group})");
                    }
                }

                Log.Debug($"{Config.Scp914ItemChanges.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<ItemUpgradeChance>> upgrade in Config.Scp914ItemChanges)
                {
                    foreach ((ItemType oldItem, ItemType newItem, int chance) in upgrade.Value)
                        Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem} - {chance}");
                }

                Log.Debug($"{Config.Scp914ClassChanges.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config.Scp914ClassChanges)
                {
                    foreach ((RoleType oldRole, RoleType newRole, int chance) in upgrade.Value)
                        Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance}");
                }

                Log.Debug($"{Config.Scp914EffectChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Config.Scp914EffectChances)
                {
                    foreach ((EffectType effect, int chance, float duration) in upgrade.Value)
                        Log.Debug($"914 Effect Config: {upgrade.Key}: {effect} + {duration} - {chance}");
                }

                Log.Debug($"{Config.Scp914TeleportChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Config.Scp914TeleportChances)
                {
                    foreach ((RoomType room, Vector3 offset, int chance, float damage) in upgrade.Value)
                        Log.Debug($"914 Teleport Config: {upgrade.Key}: {room} + {offset} - {chance} [{damage}]");
                }
            }

            Singleton = this;

            Log.Info($"Instantiating Events..");
            PlayerHandlers = new PlayerHandlers(this);
            ServerHandlers = new ServerHandlers(this);
            MapHandlers = new MapHandlers(this);
            
            Log.Info($"Registering EventHandlers..");
            Player.Verified += PlayerHandlers.OnPlayerVerified;
            Player.ChangingRole += PlayerHandlers.OnChangingRole;
            Player.Died += PlayerHandlers.OnPlayerDied;
            Player.Hurting += PlayerHandlers.OnPlayerHurting;
            Player.InteractingDoor += PlayerHandlers.OnInteractingDoor;
            Player.InteractingElevator += PlayerHandlers.OnInteractingElevator;


            Server.RoundStarted += ServerHandlers.OnRoundStarted;
            Server.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;
            Server.RoundEnded += ServerHandlers.OnRoundEnded;
            Server.RestartingRound += ServerHandlers.OnRestartingRound;

            Scp914.UpgradingItem += MapHandlers.OnScp914UpgradingItem;
            Scp914.UpgradingPlayer += MapHandlers.OnScp914UpgradingPlayer;
            Scp914.UpgradingInventoryItem += MapHandlers.OnScp914UpgradingInventoryItem;

            Instance = new Harmony($"com.joker.cu-{DateTime.UtcNow.Ticks}");
            Instance.PatchAll();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Verified -= PlayerHandlers.OnPlayerVerified;
            Player.ChangingRole -= PlayerHandlers.OnChangingRole;
            Player.Died -= PlayerHandlers.OnPlayerDied;
            Player.Hurting -= PlayerHandlers.OnPlayerHurting;
            Player.InteractingDoor -= PlayerHandlers.OnInteractingDoor;
            Player.InteractingElevator -= PlayerHandlers.OnInteractingElevator;
            

            Server.RoundStarted -= ServerHandlers.OnRoundStarted;
            Server.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;
            Server.RoundEnded -= ServerHandlers.OnRoundEnded;
            Server.RestartingRound -= ServerHandlers.OnRestartingRound;

            Scp914.UpgradingItem -= MapHandlers.OnScp914UpgradingItem;
            Scp914.UpgradingPlayer -= MapHandlers.OnScp914UpgradingPlayer;
            Scp914.UpgradingInventoryItem -= MapHandlers.OnScp914UpgradingInventoryItem;
            
            Instance.UnpatchAll();

            base.OnDisabled();
        }
    }
}