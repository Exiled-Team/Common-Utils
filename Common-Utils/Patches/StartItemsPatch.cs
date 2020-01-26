using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			List<ItemType> Inventory;
			
			switch (classid)
			{
				case RoleType.ClassD:
					Inventory = Instance.Inventories.ClassD;
					break;
				case RoleType.ChaosInsurgency:
					Inventory = Instance.Inventories.Chaos;
					break;
				case RoleType.NtfCadet:
					Inventory = Instance.Inventories.NtfCadet;
					break;
				case RoleType.NtfCommander:
					Inventory = Instance.Inventories.NtfCommander;
					break;
				case RoleType.NtfLieutenant:
					Inventory = Instance.Inventories.NtfLieutenant;
					break;
				case RoleType.NtfScientist:
					Inventory = Instance.Inventories.NtfScientist;
					break;
				case RoleType.Scientist:
					Inventory = Instance.Inventories.Scientist;
					break;
				case RoleType.FacilityGuard:
					Inventory = Instance.Inventories.Guard;
					break;
				default:
					Inventory = CustomInventory.ConvertToItemList(KConf.ExiledConfiguration.GetListStringValue(EXILED.Plugin.Config.GetString($"util_{classid.ToString().ToLowerInvariant()}_inventory", null)));
					break;
			}
			if (Inventory != null)
			{
				__instance.Classes.SafeGet(classid).startItems = Inventory.ToArray();
			}
		}
	}
}
