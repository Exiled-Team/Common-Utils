using HarmonyLib;

namespace Common_Utilities
{
    using System;
    using System.Collections.Generic;
    using Common_Utilities.EventHandlers;
    using MEC;
    using Exiled.API.Features;
    using Server = Exiled.Events.Handlers.Server;
    using Player = Exiled.Events.Handlers.Player;
    using Scp914 = Exiled.Events.Handlers.Scp914;

    public class Plugin : Exiled.API.Features.Plugin<Config>
    {
        public override string Name { get; } = "Common Utilities";
        public override string Author { get; } = "Galaxy119";
        public override Version Version { get; } = new Version(2, 5, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 11, 1);
        public override string Prefix { get; } = "CommonUtilities";
        
        public PlayerHandlers PlayerHandlers;
        public ServerHandlers ServerHandlers;
        public MapHandlers MapHandlers;
        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        public Random Gen = new Random();
        public static Plugin Singleton;
        public Harmony Instance;

        public override void OnEnabled()
        {
            Singleton = this;
            
            Log.Info($"Parsing config..");
            try
            {
                Timing.CallDelayed(10f, () =>
                {
                    try
                    {
                        Log.Debug($"Parsing inventory config..", Config.Debug);
                        Config.ParseInventorySettings();
                    }
                    catch (Exception e)
                    {
                        Log.Error("Ya fucked up the parsing.");
                        Log.Error($"{e.Message}\n{e.StackTrace}");
                    }
                });
                Log.Debug($"Parsing health config..", Config.Debug);
                Config.ParseHealthSettings();
                Log.Debug($"Parsing health on kill config..", Config.Debug);
                Config.ParseHealthOnKill();
                Log.Debug($"Parsing 914 config..", Config.Debug);
                Config.Parse914Settings();
                Log.Debug($"Parsing 914 role config..", Config.Debug);
                Config.Parse914ClassChanges();
                Log.Debug($"Parsing SCP Damage Multipliers..", Config.Debug);
                Config.ParseScpDamageMultipliers();
                Log.Debug($"Parsing Weapon Damage Multipliers..", Config.Debug);
                Config.ParseWeaponDamageMultipliers();
            }
            catch (Exception e)
            {
                Log.Error($"Ya fucked up the parsing.");
                Log.Error($"{e.Message}\n{e.StackTrace}");
            }

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

            Scp914.UpgradingItems += MapHandlers.OnScp914UpgradingItems;

            Instance = new Harmony($"com.galaxy.cu-{DateTime.UtcNow.Ticks}");
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

            Scp914.UpgradingItems -= MapHandlers.OnScp914UpgradingItems;
            
            Instance.UnpatchAll();

            base.OnDisabled();
        }
    }
}