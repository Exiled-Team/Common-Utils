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
    using Map = Exiled.Events.Handlers.Map;
    using Warhead = Exiled.Events.Handlers.Warhead;

    public class Plugin : Exiled.API.Features.Plugin<Config>
    {
        public override string Name { get; } = "Common Utilities";
        public override string Author { get; } = "Galaxy119";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 0, 0);
        public override string Prefix { get; } = "CommonUtilities";
        
        public PlayerHandlers PlayerHandlers;
        public ServerHandlers ServerHandlers;
        public MapHandlers MapHandlers;
        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        public Random Gen = new Random();
        public static Plugin Singleton;

        public override void OnEnabled()
        {
            Singleton = this;
            
            Log.Info($"Parsing config..");
            try
            {
                Log.Debug($"Parsing inventory config..", Config.Debug);
                Config.ParseInventorySettings();
                Log.Debug($"Parsing health config..", Config.Debug);
                Config.ParseHealthSettings();
                Log.Debug($"Parsing health on kill config..", Config.Debug);
                Config.ParseHealthOnKill();
                Log.Debug($"Parsing 914 config..", Config.Debug);
                Config.Parse914Settings();
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
            Player.Joined += PlayerHandlers.OnPlayerJoined;
            Player.ChangingRole += PlayerHandlers.OnChangingRole;
            Player.Died += PlayerHandlers.OnPlayerDied;
            

            Server.RoundStarted += ServerHandlers.OnRoundStarted;
            Server.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;
            Server.RoundEnded += ServerHandlers.OnRoundEnded;

            Scp914.UpgradingItems += MapHandlers.OnScp914UpgradingItems;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Joined -= PlayerHandlers.OnPlayerJoined;
            Player.ChangingRole -= PlayerHandlers.OnChangingRole;
            Player.Died -= PlayerHandlers.OnPlayerDied;

            Server.RoundStarted -= ServerHandlers.OnRoundStarted;
            Server.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;
            Server.RoundEnded -= ServerHandlers.OnRoundEnded;
            
            Scp914.UpgradingItems -= MapHandlers.OnScp914UpgradingItems;

            base.OnDisabled();
        }
    }
}