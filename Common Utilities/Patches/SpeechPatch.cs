using HarmonyLib;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static class SpeechPatch
    {
        public static bool Prefix(Radio __instance, bool b)
        {
            if (Plugin.Singleton.Config.ScpSpeech.Contains(__instance._hub.characterClassManager.NetworkCurClass))
                __instance._dissonanceSetup.MimicAs939 = b;
            return true;
        }
    }
}
