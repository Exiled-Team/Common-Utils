using System.Collections.Generic;
using Exiled.API.Features;
using Scp914;

namespace Common_Utilities.EventHandlers
{
	using System;
	using Exiled.API.Extensions;
	using Exiled.Events.EventArgs;
	using System.Linq;
	using MEC;
	using Respawning;
	using UnityEngine;
	using Player = Exiled.API.Features.Player;
	public class Handlers
	{
		private readonly Plugin plugin;
		public Handlers(Plugin plugin) => this.plugin = plugin;
		public void OnScp914UpgradingItems(UpgradingItemsEventArgs ev)
		{
			if (plugin.Config.Scp914Configs.ContainsKey(ev.KnobSetting))
			{
				foreach (Pickup item in ev.Items)
				{
					foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[
						ev.KnobSetting])
					{
						if (sourceItem != item.ItemId)
							continue;

						if (plugin.Gen.Next(100) <= chance)
						{
							UpgradeItem(item, destinationItem);
							break;
						}
					}
				}

				if (Exiled.API.Features.Scp914.ConfigMode.Value.HasFlagFast(Scp914Mode.Inventory))
				{
					foreach (Player player in ev.Players)
					{
						if (Exiled.API.Features.Scp914.ConfigMode.Value.HasFlagFast(Scp914Mode.Held))
						{
							ItemType original = player.CurrentItem.id;

							foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[ev.KnobSetting])
							{
								if (sourceItem != original)
									continue;

								if (plugin.Gen.Next(100) <= chance)
								{
									player.RemoveItem(player.CurrentItem);
									player.AddItem(destinationItem);
									break;
								}
							}
						}
						else
						{
							foreach (var item in player.Inventory.items)
							{
								foreach ((ItemType sourceItem, ItemType destinationItem, int chance) in plugin.Config.Scp914Configs[ev.KnobSetting])
								{
									if (sourceItem != item.id)
										continue;

									if (plugin.Gen.Next(100) <= chance)
									{
										player.RemoveItem(item);
										player.AddItem(destinationItem);
										break;
									}
								}
							}
						}
					}
				}
			}

			if (plugin.Config.Scp914RoleChanges.ContainsKey(ev.KnobSetting))
			{
				foreach (Player player in ev.Players)
				{
					foreach ((RoleType originalRole, RoleType newRole, int chance) in plugin.Config.Scp914RoleChanges[ev.KnobSetting])
					{
						if (player.Role != originalRole)
							continue;

						if (plugin.Gen.Next(100) <= chance)
						{
							player.SetRole(newRole, true);
							break;
						}
					}
				}
			}
		}

		internal void UpgradeItem(Pickup oldItem, ItemType newItem)
		{
			oldItem.Delete();
			newItem.Spawn(default, Exiled.API.Features.Scp914.OutputBooth.position);
		}
		
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

			var escapestring = "";
			if (plugin.Config.AnnounceClassdElimination && ev.Player.Role == RoleType.ClassD)
			{
				if (ev.Player.IsCuffed) escapestring = "Attention . a member of classd personnel has been escorted outside the site";
				else escapestring = "Attention . a member of classd personnel has escaped the site";
				Timing.CallDelayed(0.5f, () =>
				{
					if (Player.Get(RoleType.ClassD).IsEmpty()) escapestring += " . no classd personnel remain within";
					RespawnEffectsController.PlayCassieAnnouncement(escapestring, false, true);
				});
			}
			
			else if (plugin.Config.AnnounceScientistsElimination && ev.Player.Role == RoleType.Scientist)
			{
				if (ev.Player.IsCuffed) escapestring = "Attention . a member of science personnel has been escorted outside the site";
				else escapestring = "Attention . a member of science personnel has escaped the site";
				Timing.CallDelayed(0.5f, () =>
				{
					if (Player.Get(RoleType.Scientist).IsEmpty()) escapestring += " . no science personnel remain within";
					RespawnEffectsController.PlayCassieAnnouncement(escapestring, false, true);
				});
			}
			
			else if (plugin.Config.AnnounceGuardsElimination && ev.Player.Role == RoleType.FacilityGuard)
			{
				Timing.CallDelayed(0.5f, () =>
				{
					if (Player.Get(RoleType.FacilityGuard).IsEmpty()) RespawnEffectsController.PlayCassieAnnouncement("Attention . no security personnel remain within the site", false, true);
				});
			}
		}

		public void OnPlayerDied(DiedEventArgs ev)
		{
			if (plugin.Config.HealOnKill.ContainsKey(ev.Killer.Role))
			{
				if (ev.Killer.Health + plugin.Config.HealOnKill[ev.Killer.Role] <= ev.Killer.MaxHealth)  ev.Killer.Health += plugin.Config.HealOnKill[ev.Killer.Role];
				else ev.Killer.Health = ev.Killer.MaxHealth;
			}

			if (plugin.Config.AnnounceClassdElimination && ev.Target.Role == RoleType.ClassD)
			{
				Timing.CallDelayed(0.5f, () =>
				{
					if (Player.Get(RoleType.ClassD).IsEmpty()) RespawnEffectsController.PlayCassieAnnouncement("Attention . no classd personnel remain within the site", false, true);
				});
			}
			
			else if (plugin.Config.AnnounceScientistsElimination && ev.Target.Role == RoleType.Scientist)
			{
				Timing.CallDelayed(0.5f, () =>
				{
					if (Player.Get(RoleType.Scientist).IsEmpty()) RespawnEffectsController.PlayCassieAnnouncement("Attention . no science personnel remain within the site", false, true);
				});
			}

			else if (plugin.Config.AnnounceGuardsElimination && ev.Target.Role == RoleType.FacilityGuard)
			{
				Timing.CallDelayed(0.5f, () =>
				{
					if (Player.Get(RoleType.FacilityGuard).IsEmpty()) RespawnEffectsController.PlayCassieAnnouncement("Attention . no security personnel remain within the site", false, true);
				});
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
				{
					if (plugin.Config.Scp939Damage != 65) ev.Amount = plugin.Config.Scp939Damage;
					if (plugin.Config.ExtraAmnesia > 0) ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(plugin.Config.ExtraAmnesia);
				}
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
					foreach (Door door in Map.Doors)
					{
						door.SetStateWithSound(false);
						door.Networklocked = true;
					}
					Timing.CallDelayed(plugin.Config.NukeLockdownDuration, () =>
					{
						foreach (Door door in Map.Doors)
						{
							if (door.DoorName != "NUKE_SURFACE")
							{
								door.SetStateWithSound(false);
								door.Networklocked = false;
							}
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
			if (plugin.Config.AutonukeTime > 0)
				plugin.Coroutines.Add(Timing.RunCoroutine(AutoNuke()));
			
			if (plugin.Config.RagdollCleanupDelay > 0)
				plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));
			
			if (plugin.Config.ItemCleanupDelay > 0)
				plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));

			if (plugin.Config.DestroyDoors)
			{
				foreach (Door door in Map.Doors)
				{
					if (plugin.Config.DestroyedDoors.Contains(door.DoorName))
					{
						door.SetStateWithSound(false);
						door.Networkdestroyed = true;
					}
				}
			}
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
			for (; ; )
			{
				yield return Timing.WaitForSeconds(plugin.Config.ItemCleanupDelay);

				foreach (Pickup item in UnityEngine.Object.FindObjectsOfType<Pickup>())
					if ((plugin.Config.ItemCleanupOnlyPocket && item.position.y < -1500f) || (plugin.Config.ItemCleanupNotPocket && item.position.y > -1500f))
						item.Delete();
			}
		}

		private IEnumerator<float> RagdollCleanup()
		{
			for (; ; )
			{
				yield return Timing.WaitForSeconds(plugin.Config.RagdollCleanupDelay);

				foreach (Ragdoll ragdoll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if ((plugin.Config.RagdollCleanupOnlyPocket && ragdoll.transform.position.y < -1500f) || (plugin.Config.RagdollCleanupNotPocket && ragdoll.transform.position.y > -1500f))
						UnityEngine.Object.Destroy(ragdoll);
			}
		}

		private IEnumerator<float> AutoNuke()
		{
			yield return Timing.WaitForSeconds(plugin.Config.AutonukeTime-30);
			RespawnEffectsController.PlayCassieAnnouncement(plugin.Config.AutonukeCassieMessage, false, true);
			yield return Timing.WaitForSeconds(30);
			Warhead.Start();
			if (plugin.Config.AutonukeLock) Warhead.IsLocked = true;
		}

		public void OnRestartingRound()
		{
			foreach (CoroutineHandle coroutine in plugin.Coroutines)
				Timing.KillCoroutines(coroutine);
		}
	}
}
