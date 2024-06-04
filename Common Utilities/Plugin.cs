// ReSharper disable InconsistentNaming
namespace Common_Utilities;

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

#pragma warning disable SA1211
using player = Exiled.Events.Handlers.Player;
using scp914 = Exiled.Events.Handlers.Scp914;
using server = Exiled.Events.Handlers.Server;
using warhead = Exiled.Events.Handlers.Warhead;
using Random = System.Random;
#pragma warning restore SA1211

public class Plugin : Plugin<Config>
{
    public static Plugin Instance;
    public static Random Random;
    public PlayerHandlers playerHandlers;
    private ServerHandlers serverHandlers;
    private MapHandlers mapHandlers;
    private Harmony harmony;
    private string harmonyName;
        
    public static List<CoroutineHandle> Coroutines { get; } = new();
    
    public static Dictionary<Player, Tuple<int, Vector3>> AfkDict { get; } = new();

    public override string Name { get; } = "Common Utilities";

    public override string Author { get; } = "Exiled-Team";

    public override Version Version { get; } = new(7, 1, 1);

    public override Version RequiredExiledVersion { get; } = new(8, 8, 1);

    public override string Prefix { get; } = "CommonUtilities";

    public override PluginPriority Priority => PluginPriority.Higher;

    public override void OnEnabled()
    {
        if (Config.Debug)
            DebugConfig();

        Instance = this;

        Random = new Random();

        Log.Info($"Instantiating Events..");
            
        playerHandlers = new PlayerHandlers(this);
        serverHandlers = new ServerHandlers(this);
        mapHandlers = new MapHandlers(this);
            
        Log.Info($"Registering EventHandlers..");
            
        player.Hurting += playerHandlers.OnPlayerHurting;
        player.Verified += playerHandlers.OnPlayerVerified;
        player.Spawned += playerHandlers.OnSpawned;
        player.Escaping += playerHandlers.OnEscaping;
            
        if (Config.HealthOnKill != null)
            player.Died += playerHandlers.OnPlayerDied;
            
        if (Config.StartingInventories != null)
            player.ChangingRole += playerHandlers.OnChangingRole;
            
        if (Config.RadioBatteryDrainMultiplier is not 1)
            player.UsingRadioBattery += playerHandlers.OnUsingRadioBattery;
            
        if (Config.AfkLimit > 0)
        {
            player.Jumping += playerHandlers.AntiAfkEventHandler;
            player.Shooting += playerHandlers.AntiAfkEventHandler;
            player.UsingItem += playerHandlers.AntiAfkEventHandler;
            player.MakingNoise += playerHandlers.AntiAfkEventHandler;
            player.ReloadingWeapon += playerHandlers.AntiAfkEventHandler;
            player.ThrownProjectile += playerHandlers.AntiAfkEventHandler;
            player.ChangingMoveState += playerHandlers.AntiAfkEventHandler;
            player.InteractingDoor += playerHandlers.AntiAfkEventHandler;
            player.InteractingElevator += playerHandlers.AntiAfkEventHandler;
        }

        server.RoundEnded += serverHandlers.OnRoundEnded;
        server.RoundStarted += serverHandlers.OnRoundStarted;
        server.RestartingRound += serverHandlers.OnRestartingRound;
        server.WaitingForPlayers += serverHandlers.OnWaitingForPlayers;

        scp914.UpgradingPlayer += mapHandlers.OnScp914UpgradingPlayer;
            
        if (Config.Scp914ItemChances != null)
            scp914.UpgradingPickup += mapHandlers.OnUpgradingPickup;
        if (Config.Scp914ItemChances != null)
            scp914.UpgradingInventoryItem += mapHandlers.OnUpgradingInventoryItem;

        warhead.Starting += serverHandlers.OnWarheadStarting;
        warhead.Stopping += serverHandlers.OnWarheadStopping;

        harmonyName = $"com-exiled-team.cu-{DateTime.UtcNow.Ticks}";
        harmony = new Harmony(harmonyName);
        harmony.PatchAll();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        player.Hurting -= playerHandlers.OnPlayerHurting;
        player.Verified -= playerHandlers.OnPlayerVerified;
        player.Spawned -= playerHandlers.OnSpawned;
        player.Escaping -= playerHandlers.OnEscaping;
        player.Died -= playerHandlers.OnPlayerDied;
        player.ChangingRole -= playerHandlers.OnChangingRole;
        player.UsingRadioBattery -= playerHandlers.OnUsingRadioBattery;
        player.Jumping -= playerHandlers.AntiAfkEventHandler;
        player.Shooting -= playerHandlers.AntiAfkEventHandler;
        player.UsingItem -= playerHandlers.AntiAfkEventHandler;
        player.MakingNoise -= playerHandlers.AntiAfkEventHandler;
        player.ReloadingWeapon -= playerHandlers.AntiAfkEventHandler;
        player.ThrownProjectile -= playerHandlers.AntiAfkEventHandler;
        player.ChangingMoveState -= playerHandlers.AntiAfkEventHandler;
        player.InteractingDoor -= playerHandlers.AntiAfkEventHandler;
        player.InteractingElevator -= playerHandlers.AntiAfkEventHandler;

        server.RoundEnded -= serverHandlers.OnRoundEnded;
        server.RoundStarted -= serverHandlers.OnRoundStarted;
        server.RestartingRound -= serverHandlers.OnRestartingRound;
        server.WaitingForPlayers -= serverHandlers.OnWaitingForPlayers;

        scp914.UpgradingPlayer -= mapHandlers.OnScp914UpgradingPlayer;
        scp914.UpgradingPickup -= mapHandlers.OnUpgradingPickup;
        scp914.UpgradingInventoryItem -= mapHandlers.OnUpgradingInventoryItem;

        warhead.Starting -= serverHandlers.OnWarheadStarting;
        warhead.Stopping -= serverHandlers.OnWarheadStopping;
            
        harmony.UnpatchAll(harmonyName);

        serverHandlers = null;
        playerHandlers = null;
        mapHandlers = null;
            
        base.OnDisabled();
    }

    private void DebugConfig()
    {
        if (Config.StartingInventories != null)
        {
            Log.Debug($"Starting Inventories: {Config.StartingInventories.Count}");
            foreach (KeyValuePair<RoleTypeId, RoleInventory> kvp in Config.StartingInventories)
            {
                for (int i = 0; i < kvp.Value.UsedSlots; i++)
                {
                    foreach (ItemChance chance in kvp.Value[i])
                    {
                        Log.Debug($"Inventory Config: {kvp.Key} - Slot{i + 1}: {chance.ItemName} ({chance.Chance})");
                    }
                }

                foreach ((ItemType type, ushort amount, string group) in kvp.Value.Ammo)
                {
                    Log.Debug($"Ammo Config: {kvp.Key} - {type} {amount} ({group})");
                }
            }
        }

        if (Config.Scp914ItemChances != null)
        {
            Log.Debug($"{Config.Scp914ItemChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<ItemUpgradeChance>> upgrade in Config.Scp914ItemChances)
            {
                foreach ((string oldItem, string newItem, double chance, int count) in upgrade.Value)
                    Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem}x({count}) - {chance}");
            }
        }

        if (Config.Scp914ClassChanges != null)
        {
            Log.Debug($"{Config.Scp914ClassChanges.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config.Scp914ClassChanges)
            {
                foreach ((string oldRole, string newRole, double chance, bool keepInventory, bool keepHealth) in upgrade.Value)
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