using Assets._Scripts.Dissonance;
using HarmonyLib;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
    public class Scp0492Speak
    {
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            if (!Plugin.Singleton.Config.Scp0492Speech)
                return true;
			
            CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();

            if (ccm.CurClass == RoleType.Scp0492 || ccm.CurClass.Is939()) 
                __instance.MimicAs939 = value;

            return true;
        }
    }
}