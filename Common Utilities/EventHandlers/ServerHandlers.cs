namespace Common_Utilities.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs;
    using InventorySystem.Items.Pickups;
    using MEC;
    using UnityEngine;
    
    public class ServerHandlers
    {
        private readonly Plugin _plugin;
        public ServerHandlers(Plugin plugin) => this._plugin = plugin;

        public Vector3 EscapeZone = Vector3.zero;
        private bool _friendlyFireDisable = false;
        
        public void OnRoundStarted()
        {
            if (_plugin.Config.AutonukeTime > -1)
                _plugin.Coroutines.Add(Timing.RunCoroutine(AutoNuke()));
            
            if (_plugin.Config.RagdollCleanupDelay > 0)
                _plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));
            
            if (_plugin.Config.ItemCleanupDelay > 0)
                _plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));
            
            if (_plugin.Config.DisarmSwitchTeams)
                _plugin.Coroutines.Add(Timing.RunCoroutine(BetterDisarm()));
        }

        private IEnumerator<float> BetterDisarm()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(1.5f);

                foreach (Player player in Player.List)
                {
                    if (EscapeZone == Vector3.zero)
                        EscapeZone = player.GameObject.GetComponent<Escape>().worldPosition;

                    if (!player.IsCuffed || (player.Team != Team.CHI && player.Team != Team.MTF) || (EscapeZone - player.Position).sqrMagnitude > 400f)
                        continue;

                    switch (player.Role)
                    {
                        case RoleType.FacilityGuard:
                        case RoleType.NtfPrivate:
                        case RoleType.NtfSergeant:
                        case RoleType.NtfCaptain:
                        case RoleType.NtfSpecialist:
                            _plugin.Coroutines.Add(Timing.RunCoroutine(DropItems(player, player.Items.ToList())));
                            player.Role = RoleType.ChaosConscript;
                            break;
                        case RoleType.ChaosConscript:
                        case RoleType.ChaosMarauder:
                        case RoleType.ChaosRepressor:
                        case RoleType.ChaosRifleman:
                            _plugin.Coroutines.Add(Timing.RunCoroutine(DropItems(player, player.Items.ToList())));
                            player.Role = RoleType.NtfPrivate;
                            break;
                    }
                }
            }
        }

        public IEnumerator<float> DropItems(Player player, IEnumerable<Item> items)
        {
            yield return Timing.WaitForSeconds(1f);

            foreach (Item item in items)
                item.Spawn(player.Position, default);
        }

        public void OnWaitingForPlayers()
        {
            if (_friendlyFireDisable)
            {
                Log.Debug($"{nameof(OnWaitingForPlayers)}: Disabling friendly fire.", _plugin.Config.Debug);
                Server.FriendlyFire = false;
                _friendlyFireDisable = false;
            }

            if (_plugin.Config.TimedBroadcastDelay > 0)
                _plugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));
            
            Warhead.IsLocked = false;
        }
        
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (_plugin.Config.FriendlyFireOnRoundEnd && !Server.FriendlyFire)
            {
                Log.Debug($"{nameof(OnRoundEnded)}: Enabling friendly fire.", _plugin.Config.Debug);
                Server.FriendlyFire = true;
                _friendlyFireDisable = true;
            }

            foreach (CoroutineHandle coroutine in _plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            _plugin.Coroutines.Clear();
        }

        private IEnumerator<float> ServerBroadcast()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(_plugin.Config.TimedBroadcastDelay);
                
                Map.Broadcast(_plugin.Config.TimedBroadcastDuration, _plugin.Config.TimedBroadcast);
            }
        }

        private IEnumerator<float> ItemCleanup()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(_plugin.Config.ItemCleanupDelay);

                foreach (ItemPickupBase item in Object.FindObjectsOfType<ItemPickupBase>())
                    if (!_plugin.Config.ItemCleanupOnlyPocket || item.NetworkInfo.Position.y < -1500f)
                        item.DestroySelf();
            }
        }

        private IEnumerator<float> RagdollCleanup()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(_plugin.Config.RagdollCleanupDelay);
                
                foreach (Ragdoll ragdoll in Map.Ragdolls)
                    if (!_plugin.Config.RagdollCleanupOnlyPocket || ragdoll.Position.y < -1500f)
                        ragdoll.Delete();
            }
        }

        private IEnumerator<float> AutoNuke()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.AutonukeTime);
            
            Warhead.Start();

            if (_plugin.Config.AutonukeLock)
                Warhead.IsLocked = true;
        }

        public void OnRestartingRound()
        {
            foreach (CoroutineHandle coroutine in _plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            _plugin.Coroutines.Clear();
        }
    }
}