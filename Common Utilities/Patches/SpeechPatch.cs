using Assets._Scripts.Dissonance;
using HarmonyLib;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.UserCode_CmdAltIsActive))]
    public static class SpeechPatch
    {
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();

            if (!Plugin.Singleton.Config.ScpSpeech.Contains(ccm.NetworkCurClass))
                return true;

            __instance.MimicAs939 = value;

            return true;
        }
    }
}
