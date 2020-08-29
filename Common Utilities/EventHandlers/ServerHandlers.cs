namespace Common_Utilities.EventHandlers
{
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Respawning;
    using MEC;
    using UnityEngine;
    
    public class ServerHandlers
    {
        private readonly Plugin plugin;
        public ServerHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnConsoleCommand(SendingConsoleCommandEventArgs ev)
        {
            if (ev.Name.ToLower().Contains("nukelockdown"))
            {
                ev.Allow = false;
                if (ev.Player.Role == RoleType.Scp079 && Warhead.IsInProgress && plugin.Config.NukeLockdown)
                {
                    if (ev.Player.Energy < plugin.Config.NukeLockdownCost)
                    {
                        ev.ReturnMessage = $"Energy too low. You need {plugin.Config.NukeLockdownCost} energy for this command.";
                        ev.Color = "Red";
                        return;
                    }
                    Generator079.Generators[0].ServerOvercharge(plugin.Config.NukeLockdownDuration, false);
                    foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
                    {
                        door.SetStateWithSound(false);
                        door.Networklocked = true;
                    }
                    Timing.CallDelayed(plugin.Config.NukeLockdownDuration, () =>
                    {
                        foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
                        {
                            door.Networklocked = false;
                        }
                    });
                    return;
                }
                else
                {
                    ev.ReturnMessage = "You can't use this command!";
                    ev.Color = "Red";
                    return;
                }
            }
		}
		
        public void OnRoundStarted()
        {
            if (plugin.Config.AutonukeTime > -1)
                plugin.Coroutines.Add(Timing.RunCoroutine(AutoNuke()));
            
            if (plugin.Config.RagdollCleanupDelay > 0)
                plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));
            
            if (plugin.Config.ItemCleanupDelay > 0)
                plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));
        }

        public void OnWaitingForPlayers()
        {
            if (plugin.Config.TimedBroadcastDelay > 0)
                plugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));
            
            Warhead.IsLocked = false;
        }
		
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (CoroutineHandle coroutine in plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
        }
		
        private IEnumerator<float> ServerBroadcast()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(plugin.Config.TimedBroadcastDelay);
                
                Map.Broadcast(plugin.Config.TimedBroadcastDuration, plugin.Config.TimedBroadcast);
            }
        }

        private IEnumerator<float> ItemCleanup()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(plugin.Config.ItemCleanupDelay);

                foreach (Pickup item in Object.FindObjectsOfType<Pickup>())
                    if ((plugin.Config.ItemCleanupOnlyPocket && item.position.y < -1500f) || (plugin.Config.ItemCleanupNotPocket && item.position.y > -1500f))
                        item.Delete();
            }
        }

        private IEnumerator<float> RagdollCleanup()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(plugin.Config.RagdollCleanupDelay);
                
                foreach (Ragdoll ragdoll in Object.FindObjectsOfType<Ragdoll>())
                    if ((plugin.Config.RagdollCleanupOnlyPocket && ragdoll.transform.position.y < -1500f) || (plugin.Config.RagdollCleanupNotPocket && ragdoll.transform.position.y > -1500f))
                        Object.Destroy(ragdoll);
            }
        }

        private IEnumerator<float> AutoNuke()
        {
            yield return Timing.WaitForSeconds(plugin.Config.AutonukeTime - 30);
            RespawnEffectsController.PlayCassieAnnouncement(plugin.Config.AutonukeCassieMessage, false, true);
            yield return Timing.WaitForSeconds(30);
            Warhead.Start();
            
            if (plugin.Config.AutonukeLock)
                Warhead.IsLocked = true;
        }

        public void OnRestartingRound()
        {
            foreach (CoroutineHandle coroutine in plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
        }
    }
}
