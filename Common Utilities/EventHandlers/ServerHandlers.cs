namespace Common_Utilities.EventHandlers
{
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using MEC;
    using UnityEngine;
    using System;
    using Exiled.API.Features.Pickups;
    using Exiled.Events.EventArgs.Server;
    using Exiled.Events.EventArgs.Warhead;
    using PlayerRoles;
    using System.Collections.Generic;

    public class ServerHandlers
    {
        private readonly Plugin _plugin;
        public ServerHandlers(Plugin plugin) => _plugin = plugin;

        private bool friendlyFireDisable = false;
        
        public void OnRoundStarted()
        {
            if (_plugin.Config.AutonukeTime > -1)
                _plugin.Coroutines.Add(Timing.CallDelayed(_plugin.Config.AutonukeTime, AutoNuke));

            if (_plugin.Config.RagdollCleanupDelay > 0)
                _plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));

            if (_plugin.Config.ItemCleanupDelay > 0)
                _plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));

            if (_plugin.Config.DisarmSwitchTeams)
                _plugin.Coroutines.Add(Timing.RunCoroutine(BetterDisarm()));
        }

        private IEnumerator<float> BetterDisarm()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(.5f);

                foreach (Player player in Player.List)
                {
                    if (!player.IsCuffed || (player.Role.Team != Team.ChaosInsurgency && player.Role.Team != Team.FoundationForces) || (Escape.WorldPos - player.Position).sqrMagnitude > Escape.RadiusSqr)
                        continue;

                    switch (player.Role.Type)
                    {
                        case RoleTypeId.FacilityGuard:
                        case RoleTypeId.NtfPrivate:
                        case RoleTypeId.NtfSergeant:
                        case RoleTypeId.NtfCaptain:
                        case RoleTypeId.NtfSpecialist:
                            player.Role.Set(RoleTypeId.ChaosConscript, SpawnReason.Escaped, RoleSpawnFlags.All);
                            break;
                        case RoleTypeId.ChaosConscript:
                        case RoleTypeId.ChaosMarauder:
                        case RoleTypeId.ChaosRepressor:
                        case RoleTypeId.ChaosRifleman:
                            player.Role.Set(RoleTypeId.NtfPrivate, SpawnReason.Escaped, RoleSpawnFlags.All);
                            break;
                    }
                }
            }
        }

        public void OnWaitingForPlayers()
        {
            if (_plugin.Config.AfkLimit > 0)
            {
                _plugin.AfkDict.Clear();
                _plugin.Coroutines.Add(Timing.RunCoroutine(AfkCheck()));
            }

            if (friendlyFireDisable)
            {
                Log.Debug($"{nameof(OnWaitingForPlayers)}: Disabling friendly fire.");
                Server.FriendlyFire = false;
                friendlyFireDisable = false;
            }

            if (_plugin.Config.TimedBroadcastDelay > 0)
                _plugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));

            Warhead.IsLocked = false;
        }
        
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (_plugin.Config.FriendlyFireOnRoundEnd && !Server.FriendlyFire)
            {
                Log.Debug($"{nameof(OnRoundEnded)}: Enabling friendly fire.");
                Server.FriendlyFire = true;
                friendlyFireDisable = true;
            }

            foreach (CoroutineHandle coroutine in _plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            _plugin.Coroutines.Clear();
        }

        private IEnumerator<float> ServerBroadcast()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(_plugin.Config.TimedBroadcastDelay);

                Map.Broadcast(_plugin.Config.TimedBroadcastDuration, _plugin.Config.TimedBroadcast);
            }
        }

        private IEnumerator<float> ItemCleanup()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(_plugin.Config.ItemCleanupDelay);

                foreach (Pickup pickup in Pickup.List.ToList())
                    if (!_plugin.Config.ItemCleanupOnlyPocket || pickup.Position.y < -1500f)
                        pickup.Destroy();
            }
        }

        private IEnumerator<float> RagdollCleanup()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(_plugin.Config.RagdollCleanupDelay);

                foreach (Ragdoll ragdoll in Ragdoll.List.ToList())
                    if (!_plugin.Config.RagdollCleanupOnlyPocket || ragdoll.Position.y < -1500f)
                        ragdoll.Destroy();
            }
        }

        private void AutoNuke()
        {
            if (!Warhead.IsInProgress)
            {
                switch (_plugin.Config.AutonukeBroadcast.Duration)
                {
                    case 0:
                        break;
                    case 1:
                        Cassie.Message(_plugin.Config.AutonukeBroadcast.Content);
                        break;
                    default:
                        Map.Broadcast(_plugin.Config.AutonukeBroadcast);
                        break;
                }

                Warhead.Start();
            }

            if (_plugin.Config.AutonukeLock)
                Warhead.IsLocked = true;
        }

        private IEnumerator<float> AfkCheck()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);

                foreach (Player player in Player.List)
                {
                    if (!_plugin.AfkDict.ContainsKey(player))
                        _plugin.AfkDict.Add(player, new Tuple<int, Vector3>(0, player.Position));

                    if (player.Role.Type == RoleTypeId.None || player.IsNoclipPermitted || player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity) || _plugin.Config.AfkIgnoredRoles.Contains(player.Role.Type))
                    {
                        Log.Debug($"Player {player.Nickname} ({player.Role.Type}) is not a checkable player. NoClip: {player.IsNoclipPermitted} AFKImunity: {player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)}");
                        continue;
                    }

                    if ((_plugin.AfkDict[player].Item2 - player.Position).sqrMagnitude > 2)
                    {
                        Log.Debug($"Player {player.Nickname} has moved, resetting AFK timer.");
                        _plugin.AfkDict[player] = new Tuple<int, Vector3>(0, player.Position);
                    }

                    if (_plugin.AfkDict[player].Item1 >= _plugin.Config.AfkLimit)
                    {
                        _plugin.AfkDict.Remove(player);
                        Log.Debug($"Kicking {player.Nickname} for being AFK.");
                        player.Kick("You were kicked by a plugin for being AFK.");
                    }
                    else if (_plugin.AfkDict[player].Item1 >= (_plugin.Config.AfkLimit / 2))
                    {
                        player.ClearBroadcasts();
                        player.Broadcast(5,
                            $"You have been AFK for {_plugin.AfkDict[player].Item1} seconds. You will be automatically kicked if you remain AFK for a total of {_plugin.Config.AfkLimit} seconds.");
                    }

                    _plugin.AfkDict[player] = new Tuple<int, Vector3>(_plugin.AfkDict[player].Item1 + 1, _plugin.AfkDict[player].Item2);
                }
            }
        }

        public void OnRestartingRound()
        {
            foreach (CoroutineHandle coroutine in _plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            _plugin.Coroutines.Clear();
        }

        public void OnWarheadStarting(StartingEventArgs _)
        {
            foreach (Room room in Room.List)
                room.Color = _plugin.Config.WarheadColor;
        }

        public void OnWarheadStopping(StoppingEventArgs _)
        {
            foreach (Room room in Room.List)
                room.ResetColor();
        }
    }
}