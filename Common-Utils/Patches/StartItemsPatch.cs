using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using Harmony;
using static Common_Utils.Plugin;

namespace Common_Utils
{
	[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
	class StartItemsPatch
	{
		public static bool everRun = false;

		[HarmonyPriority(500)]
		public static void Prefix(CharacterClassManager __instance, RoleType classid)
		{
			try
			{
				List<ItemType> Inventory = new List<ItemType>();

				switch (classid)
				{
					case RoleType.ClassD:
						if (Instance.EnableRandomInv && Instance.Inventories.ClassDRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.ClassDRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.ClassD;

						break;
					case RoleType.ChaosInsurgency:
						if (Instance.EnableRandomInv && Instance.Inventories.ChaosRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.ChaosRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.Chaos;

						break;
					case RoleType.NtfCadet:
						if (Instance.EnableRandomInv && Instance.Inventories.NtfCadetRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.NtfCadetRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.NtfCadet;

						break;
					case RoleType.NtfCommander:
						if (Instance.EnableRandomInv && Instance.Inventories.NtfCmdRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.NtfCmdRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.NtfCommander;

						break;
					case RoleType.NtfLieutenant:
						if (Instance.EnableRandomInv && Instance.Inventories.NtfLtRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.NtfLtRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.NtfLieutenant;

						break;
					case RoleType.NtfScientist:
						if (Instance.EnableRandomInv && Instance.Inventories.NtfSciRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.NtfSciRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.NtfScientist;

						break;
					case RoleType.Scientist:
						if (Instance.EnableRandomInv && Instance.Inventories.ScientistRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.ScientistRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.Scientist;

						break;
					case RoleType.FacilityGuard:
						if (Instance.EnableRandomInv && Instance.Inventories.GuardRan.Any())
						{
							foreach (KeyValuePair<ItemType, int> item in Instance.Inventories.GuardRan)
							{
								if (Instance.Gen.Next(100) <= item.Value)
									Inventory.Add(item.Key);
							}
						}
						else
							Inventory = Instance.Inventories.Guard;

						break;
				}

				if (Inventory != null)
				{
					__instance.Classes.SafeGet(classid).startItems = Inventory.ToArray();
				}
			}
			catch (Exception e)
			{
				Log.Error($"StartItemsPatch error: {e}");
			}
		}
	}
}
