using System;
using Harmony;

namespace Common_Utils.Patches
{
	[HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), new Type[] { typeof(UnityEngine.GameObject) })]
	class AutoWarheadLockPatches
	{
		public static bool AutoLocked;
		public static bool Prefix()
		{
			return !AutoLocked;
		}
	}
}
