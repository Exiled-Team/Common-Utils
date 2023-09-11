namespace Common_Utilities;

using PlayerRoles;
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
using Scp914 = Exiled.Events.Handlers.Scp914;

public class Plugin : Plugin<Config>
{
    private string harmonyName = string.Empty;
    private Harmony harmony = null!;

    public static Plugin Instance { get; private set; } = null!;

    public override string Name => "Common Utilities";

    public override string Author => "Joker119";

    public override Version RequiredExiledVersion { get; } = new(8, 0, 0);

    public override string Prefix => "CommonUtilities";

    public override PluginPriority Priority => PluginPriority.Higher;

    public PlayerHandlers PlayerHandlers { get; private set; } = null!;

    public ServerHandlers ServerHandlers { get;  set; } = null!;

    public MapHandlers MapHandlers { get;  set; } = null!;

    public List<CoroutineHandle> Coroutines { get; } = new();

    public Dictionary<Exiled.API.Features.Player, Tuple<int, Vector3>> AfkDict { get; } = new();

    public override void OnEnabled()
    {
        if (Config.Debug)
        {
            if (Config.StartingInventories != null)
            {
                Log.Debug($"{Config.StartingInventories.Count}");
                foreach (KeyValuePair<RoleTypeId, RoleInventory?> inv in Config.StartingInventories)
                {
                    if (inv.Value is null)
                        continue;

                    for (int i = 0; i < inv.Value?.UsedSlots; i++)
                    {
                        foreach (ItemChance chance in inv.Value[i]!)
                        {
                            Log.Debug($"Inventory Config: {inv.Key} - Slot{i + 1}: {chance.ItemName} ({chance.Chance})");
                        }
                    }

                    foreach ((ItemType type, ushort amount, string group) in inv.Value?.Ammo ?? new List<StartingAmmo>())
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
                        Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem} - {chance} - {count}");
                }
            }

            if (Config.Scp914ClassChanges != null)
            {
                Log.Debug($"{Config.Scp914ClassChanges.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config
                    .Scp914ClassChanges)
                {
                    foreach ((RoleTypeId oldRole, string newRole, double chance, bool keepInventory) in upgrade.Value)
                        Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} Keep Inventory: {keepInventory}");
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

        Instance = this;

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
        Player.Spawned += PlayerHandlers.OnPlayerSpawned;
        Player.ThrownProjectile += PlayerHandlers.AntiAfkEventHandler;
        Player.Escaping += PlayerHandlers.OnPlayerEscaping;
        Player.InteractingDoor += PlayerHandlers.OnInteractingDoor;
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

        harmonyName = $"com-joker.cu-{DateTime.UtcNow.Ticks}";
        harmony = new Harmony(harmonyName);
        harmony.PatchAll();

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
        Player.ThrownProjectile -= PlayerHandlers.AntiAfkEventHandler;
        Player.InteractingDoor -= PlayerHandlers.OnInteractingDoor;
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

        harmony.UnpatchAll(harmonyName);
        base.OnDisabled();
    }
}