// ReSharper disable IteratorNeverReturns
namespace Common_Utilities.EventHandlers
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Server;
    using Exiled.Events.EventArgs.Warhead;
    using InventorySystem.Configs;
    using MEC;
    using PlayerRoles;
    using UnityEngine;

    public class ServerHandlers
    {
        private readonly Config config;
        private bool friendlyFireDisable;

        public ServerHandlers(Plugin plugin) => config = config;

        public void OnRoundStarted()
        {
            if (config.AutonukeTime > -1)
                Plugin.Coroutines.Add(Timing.CallDelayed(config.AutonukeTime, AutoNuke));

            if (config.RagdollCleanupDelay > 0)
                Plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));

            if (config.ItemCleanupDelay > 0)
                Plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));
        }

        public void OnWaitingForPlayers()
        {
            if (config.AfkLimit > 0)
            {
                Plugin.AfkDict.Clear();
                Plugin.Coroutines.Add(Timing.RunCoroutine(AfkCheck()));
            }

            if (friendlyFireDisable)
            {
                Log.Debug($"{nameof(OnWaitingForPlayers)}: Disabling friendly fire.");
                Server.FriendlyFire = false;
                friendlyFireDisable = false;
            }

            if (config.TimedBroadcastDelay > 0)
                Plugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));

            // Fix GrandLoadout not able to give this 2 inventory
            StartingInventories.DefinedInventories[RoleTypeId.Tutorial] = new(Array.Empty<ItemType>(), new());
            StartingInventories.DefinedInventories[RoleTypeId.ClassD] = new(Array.Empty<ItemType>(), new());

            Warhead.IsLocked = false;
        }
        
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (config.FriendlyFireOnRoundEnd && !Server.FriendlyFire)
            {
                Log.Debug($"{nameof(OnRoundEnded)}: Enabling friendly fire.");
                Server.FriendlyFire = true;
                friendlyFireDisable = true;
            }

            foreach (CoroutineHandle coroutine in Plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            Plugin.Coroutines.Clear();
        }

        private IEnumerator<float> ServerBroadcast()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(config.TimedBroadcastDelay);

                Map.Broadcast(config.TimedBroadcastDuration, config.TimedBroadcast);
            }
        }

        private IEnumerator<float> ItemCleanup()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(config.ItemCleanupDelay);

                foreach (Pickup pickup in Pickup.List.ToList())
                {
                    if (!config.ItemCleanupOnlyPocket || pickup.Position.y < -1500f)
                        pickup.Destroy();
                }
            }
        }

        private IEnumerator<float> RagdollCleanup()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(config.RagdollCleanupDelay);

                foreach (Ragdoll ragdoll in Ragdoll.List.ToList())
                {
                    if (!config.RagdollCleanupOnlyPocket || ragdoll.Position.y < -1500f)
                        ragdoll.Destroy();
                }
            }
        }

        private void AutoNuke()
        {
            if (!Warhead.IsInProgress)
            {
                switch (config.AutonukeBroadcast.Duration)
                {
                    case 0:
                        break;
                    case 1:
                        Cassie.Message(config.AutonukeBroadcast.Content);
                        break;
                    default:
                        Map.Broadcast(config.AutonukeBroadcast);
                        break;
                }

                Warhead.Start();
            }

            if (config.AutonukeLock)
                Warhead.IsLocked = true;
        }

        private IEnumerator<float> AfkCheck()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);

                foreach (Player player in Player.List)
                {
                    if (!Plugin.AfkDict.ContainsKey(player))
                        Plugin.AfkDict.Add(player, new Tuple<int, Vector3>(0, player.Position));

                    if (player.Role.IsDead || player.IsGodModeEnabled || player.IsNoclipPermitted || player.Role is FpcRole { IsGrounded: false } || player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity) || config.AfkIgnoredRoles.Contains(player.Role.Type))
                    {
#pragma warning disable SA1013
                        Log.Debug($"Player {player.Nickname} ({player.Role.Type}) is not a checkable player. NoClip: {player.IsNoclipPermitted} GodMode: {player.IsGodModeEnabled} IsNotGrounded: {player.Role is FpcRole { IsGrounded: false }} AFKImunity: {player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)}");
                        continue;
#pragma warning restore SA1013
                    }

                    if ((Plugin.AfkDict[player].Item2 - player.Position).sqrMagnitude > 2)
                    {
                        Log.Debug($"Player {player.Nickname} has moved, resetting AFK timer.");
                        Plugin.AfkDict[player] = new Tuple<int, Vector3>(0, player.Position);
                    }

                    if (Plugin.AfkDict[player].Item1 >= config.AfkLimit)
                    {
                        Plugin.AfkDict.Remove(player);
                        Log.Debug($"Kicking {player.Nickname} for being AFK.");
                        player.Kick("You were kicked by CommonUtilities for being AFK.");
                    }
                    else if (Plugin.AfkDict[player].Item1 >= (config.AfkLimit / 2))
                    {
                        player.Broadcast(2, $"You have been AFK for {Plugin.AfkDict[player].Item1} seconds. You will be automatically kicked if you remain AFK for a total of {config.AfkLimit} seconds.", shouldClearPrevious: true);
                    }

                    Plugin.AfkDict[player] = new Tuple<int, Vector3>(Plugin.AfkDict[player].Item1 + 1, Plugin.AfkDict[player].Item2);
                }
            }
        }

        public void OnRestartingRound()
        {
            foreach (CoroutineHandle coroutine in Plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            Plugin.Coroutines.Clear();
        }

        public void OnWarheadStarting(StartingEventArgs _)
        {
            foreach (Room room in Room.List)
                room.Color = config.WarheadColor;
        }

        public void OnWarheadStopping(StoppingEventArgs _)
        {
            if (Warhead.IsLocked)
                return;
            
            foreach (Room room in Room.List)
                room.ResetColor();
        }
    }
}