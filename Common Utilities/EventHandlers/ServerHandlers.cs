namespace Common_Utilities.EventHandlers
{
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using MEC;
    using UnityEngine;
    
    public class ServerHandlers
    {
        private readonly Plugin plugin;
        public ServerHandlers(Plugin plugin) => this.plugin = plugin;
        
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
            
            Warhead.IsWarheadLocked = false;
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
                    if (!plugin.Config.ItemCleanupOnlyPocket || item.position.y < -1500f)
                        item.Delete();
            }
        }

        private IEnumerator<float> RagdollCleanup()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(plugin.Config.RagdollCleanupDelay);
                
                foreach (Ragdoll ragdoll in Object.FindObjectsOfType<Ragdoll>())
                    if (!plugin.Config.RagdollCleanupOnlyPocket || ragdoll.transform.position.y < -1500f)
                        Object.Destroy(ragdoll);
            }
        }

        private IEnumerator<float> AutoNuke()
        {
            yield return Timing.WaitForSeconds(plugin.Config.AutonukeTime);
            
            Warhead.Start();

            if (plugin.Config.AutonukeLock)
                Warhead.IsWarheadLocked = true;
        }

        public void OnRestartingRound()
        {
            foreach (CoroutineHandle coroutine in plugin.Coroutines)
                Timing.KillCoroutines(coroutine);
        }
    }
}