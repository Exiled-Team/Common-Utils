namespace Common_Utilities.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using MEC;
    using Respawning;
    using Player = Exiled.API.Features.Player;

    public class PlayerHandlers
    {
        private readonly Plugin plugin;
        public PlayerHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnPlayerJoined(JoinedEventArgs ev)
        {
            string message = FormatJoinMessage(ev.Player);
            if (!string.IsNullOrEmpty(message))
                ev.Player.Broadcast(plugin.Config.JoinMessageDuration, message);
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (plugin.Config.Inventories.ContainsKey(ev.NewRole))
            {
                ev.Items.Clear();
                ev.Items.AddRange(StartItems(ev.NewRole));
            }

            if (plugin.Config.Health.ContainsKey(ev.NewRole))
                Timing.CallDelayed(1.5f, () =>
                {
                    ev.Player.Health = plugin.Config.Health[ev.NewRole];
                    ev.Player.MaxHealth = plugin.Config.Health[ev.NewRole];
                });

			if (plugin.Config.AnnounceDclassScientistsElimination)
			{
				if (ev.Player.Role == RoleType.ClassD && Player.Get(RoleType.ClassD) != null) 
					RespawnEffectsController.PlayCassieAnnouncement("Attention . all classd personnel are either dead or have escaped the facility", false, true);
				
				if (ev.Player.Role == RoleType.Scientist && Player.Get(RoleType.Scientist) != null) 
					RespawnEffectsController.PlayCassieAnnouncement("Attention . all science personnel are either dead or have escaped the facility", false, true);
			}
        }

        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (plugin.Config.HealOnKill.ContainsKey(ev.Killer.Role))
            {
                if (ev.Killer.Health + plugin.Config.HealOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)
                    ev.Killer.Health += plugin.Config.HealOnKill[ev.Killer.Role];
                else
                    ev.Killer.Health = ev.Killer.MaxHealth;
            }
        }

        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target != ev.Attacker)
            {
                if (plugin.Config.ExtraAmnesia > 0 && ev.Attacker.Role.Is939()) ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(plugin.Config.ExtraAmnesia);
                if (plugin.Config.Scp106Damage != 40 && ev.Attacker.Role == RoleType.Scp106) ev.Amount = plugin.Config.Scp106Damage;
            }
        }

        public List<ItemType> StartItems(RoleType role)
        {
            List<ItemType> items = new List<ItemType>();

            if (plugin.Config.Inventories[role] == default)
                return items;

            foreach (Tuple<ItemType, int> itemChance in plugin.Config.Inventories[role])
            {
                if (plugin.Gen.Next(100) <= itemChance.Item2)
                    items.Add(itemChance.Item1);
            }

            return items;
        }

        private string FormatJoinMessage(Player player) =>
            string.IsNullOrEmpty(plugin.Config.JoinMessage) ? string.Empty : plugin.Config.JoinMessage.Replace("%player%", player.Nickname).Replace("%server%", Server.Name).Replace("%count%", $"{Player.Dictionary.Count}");
    }
}
