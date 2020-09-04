using Assets._Scripts.Dissonance;
using HarmonyLib;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
    public class Scp049Speak
    {
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            if (!Plugin.Singleton.Config.Scp049Speech)
                return true;
			
            CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();

            if (ccm.CurClass == RoleType.Scp049 || ccm.CurClass.Is939()) 
                __instance.MimicAs939 = value;

            return true;
        }
    }
}