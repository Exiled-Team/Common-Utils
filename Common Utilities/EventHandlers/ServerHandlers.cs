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
        private readonly Main plugin;

        private bool friendlyFireDisable = false;

        public ServerHandlers(Main plugin) => this.plugin = plugin;

        public void OnRoundStarted()
        {
            if (plugin.Config.AutonukeTime > -1)
                plugin.Coroutines.Add(Timing.CallDelayed(plugin.Config.AutonukeTime, AutoNuke));

            if (plugin.Config.RagdollCleanupDelay > 0)
                plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));

            if (plugin.Config.ItemCleanupDelay > 0)
                plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));
        }

        public void OnWaitingForPlayers()
        {
            if (plugin.Config.AfkLimit > 0)
            {
                plugin.AfkDict.Clear();
                plugin.Coroutines.Add(Timing.RunCoroutine(AfkCheck()));
            }

            if (friendlyFireDisable)
            {
                Log.Debug($"{nameof(OnWaitingForPlayers)}: Disabling friendly fire.");
                Server.FriendlyFire = false;
                friendlyFireDisable = false;
            }

            if (plugin.Config.TimedBroadcastDelay > 0)
                plugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));

            // Fix GrandLoadout not able to give this 2 inventory
            StartingInventories.DefinedInventories[RoleTypeId.Tutorial] = new(Array.Empty<ItemType>(), new());
            StartingInventories.DefinedInventories[RoleTypeId.ClassD] = new(Array.Empty<ItemType>(), new());

            Warhead.IsLocked = false;
        }
        
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (plugin.Config.FriendlyFireOnRoundEnd && !Server.FriendlyFire)
            {
                Log.Debug($"{nameof(OnRoundEnded)}: Enabling friendly fire.");
                Server.FriendlyFire = true;
                friendlyFireDisable = true;
            }

            foreach (CoroutineHandle coroutine in plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            plugin.Coroutines.Clear();
        }

        public IEnumerator<float> ServerBroadcast()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(plugin.Config.TimedBroadcastDelay);

                Map.Broadcast(plugin.Config.TimedBroadcastDuration, plugin.Config.TimedBroadcast);
            }
        }

        public IEnumerator<float> ItemCleanup()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(plugin.Config.ItemCleanupDelay);

                foreach (Pickup pickup in Pickup.List.ToList())
                {
                    if (!plugin.Config.ItemCleanupOnlyPocket || pickup.Position.y < -1500f)
                        pickup.Destroy();
                }
            }
        }

        public IEnumerator<float> RagdollCleanup()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(plugin.Config.RagdollCleanupDelay);

                foreach (Ragdoll ragdoll in Ragdoll.List.ToList())
                {
                    if (!plugin.Config.RagdollCleanupOnlyPocket || ragdoll.Position.y < -1500f)
                        ragdoll.Destroy();
                }
            }
        }

        public void AutoNuke()
        {
            if (!Warhead.IsInProgress)
            {
                switch (plugin.Config.AutonukeBroadcast.Duration)
                {
                    case 0:
                        break;
                    case 1:
                        Cassie.Message(plugin.Config.AutonukeBroadcast.Content);
                        break;
                    default:
                        Map.Broadcast(plugin.Config.AutonukeBroadcast);
                        break;
                }

                Warhead.Start();
            }

            if (plugin.Config.AutonukeLock)
                Warhead.IsLocked = true;
        }

        public IEnumerator<float> AfkCheck()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);

                foreach (Player player in Player.List)
                {
                    if (!plugin.AfkDict.ContainsKey(player))
                        plugin.AfkDict.Add(player, new Tuple<int, Vector3>(0, player.Position));

                    if (player.Role.IsDead || player.IsGodModeEnabled || player.IsNoclipPermitted || player.Role is FpcRole { IsGrounded: false } || player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity) || plugin.Config.AfkIgnoredRoles.Contains(player.Role.Type))
                    {
#pragma warning disable SA1013 // Closing braces should be spaced correctly
                        Log.Debug($"Player {player.Nickname} ({player.Role.Type}) is not a checkable player. NoClip: {player.IsNoclipPermitted} GodMode: {player.IsGodModeEnabled} IsNotGrounded: {player.Role is FpcRole { IsGrounded: false }} AFKImunity: {player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)}");
#pragma warning restore SA1013 // Closing braces should be spaced correctly
                        continue;
                    }

                    if ((plugin.AfkDict[player].Item2 - player.Position).sqrMagnitude > 2)
                    {
                        Log.Debug($"Player {player.Nickname} has moved, resetting AFK timer.");
                        plugin.AfkDict[player] = new Tuple<int, Vector3>(0, player.Position);
                    }

                    if (plugin.AfkDict[player].Item1 >= plugin.Config.AfkLimit)
                    {
                        plugin.AfkDict.Remove(player);
                        Log.Debug($"Kicking {player.Nickname} for being AFK.");
                        player.Kick("You were kicked by CommonUtilities for being AFK.");
                    }
                    else if (plugin.AfkDict[player].Item1 >= (plugin.Config.AfkLimit / 2))
                    {
                        player.Broadcast(2, $"You have been AFK for {plugin.AfkDict[player].Item1} seconds. You will be automatically kicked if you remain AFK for a total of {plugin.Config.AfkLimit} seconds.", shouldClearPrevious: true);
                    }

                    plugin.AfkDict[player] = new Tuple<int, Vector3>(plugin.AfkDict[player].Item1 + 1, plugin.AfkDict[player].Item2);
                }
            }
        }

        public void OnRestartingRound()
        {
            foreach (CoroutineHandle coroutine in plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
            plugin.Coroutines.Clear();
        }

        public void OnWarheadStarting(StartingEventArgs _)
        {
            foreach (Room room in Room.List)
                room.Color = plugin.Config.WarheadColor;
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