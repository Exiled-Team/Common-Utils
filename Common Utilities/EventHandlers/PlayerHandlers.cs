namespace Common_Utilities.EventHandlers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Exiled.API.Features;
	using Exiled.Events.EventArgs;
	using MEC;
	using Respawning;
	using UnityEngine;
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
				if (ev.Player.Role == RoleType.ClassD && Player.Get(RoleType.ClassD).IsEmpty()) 
					RespawnEffectsController.PlayCassieAnnouncement("Attention . all classd personnel are either dead or have escaped the facility", false, true);
				
				if (ev.Player.Role == RoleType.Scientist && Player.Get(RoleType.Scientist).IsEmpty()) 
					RespawnEffectsController.PlayCassieAnnouncement("Attention . all science personnel are either dead or have escaped the facility", false, true);
			}
		}

		public void OnPlayerDied(DiedEventArgs ev)
		{
			if (plugin.Config.HealOnKill.ContainsKey(ev.Killer.Role))
			{
				if (ev.Killer.Health + plugin.Config.HealOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)  ev.Killer.Health += plugin.Config.HealOnKill[ev.Killer.Role];
				else ev.Killer.Health = ev.Killer.MaxHealth;
			}
		}

		public void OnPlayerHurt(HurtingEventArgs ev)
		{
			if (ev.Target != ev.Attacker)
			{
				if (plugin.Config.Scp049Damage != 4949 && ev.Attacker.Role == RoleType.Scp049) ev.Amount = plugin.Config.Scp049Damage;
				else if (plugin.Config.Scp0492Damage != 40 && ev.Attacker.Role == RoleType.Scp0492) ev.Amount = plugin.Config.Scp0492Damage;
				else if (plugin.Config.Scp096Damage != 9696 && ev.Attacker.Role == RoleType.Scp096) ev.Amount = plugin.Config.Scp096Damage;
				else if (plugin.Config.Scp106Damage != 40 && ev.Attacker.Role == RoleType.Scp106) ev.Amount = plugin.Config.Scp106Damage;
				else if (plugin.Config.Scp173Damage != 999990 && ev.Attacker.Role == RoleType.Scp173) ev.Amount = plugin.Config.Scp173Damage;
				else if (ev.Attacker.Role.Is939())
					if (plugin.Config.Scp939Damage != 65) ev.Amount = plugin.Config.Scp939Damage;
					if (plugin.Config.ExtraAmnesia > 0) ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(plugin.Config.ExtraAmnesia);
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
