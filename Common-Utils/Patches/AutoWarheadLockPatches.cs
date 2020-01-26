using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace Common_Utils.Patches
{
	// CancelDetonation(GameObject disabler)
	[HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation))]
	class AutoWarheadLockPatches
	{
		public static bool AutoLocked;
		public static bool Prefix()
		{
			return !AutoLocked;
		}
	}
}
