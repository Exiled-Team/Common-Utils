using HarmonyLib;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static class SpeechPatch
    {
        public static bool Prefix(Radio instance, bool b)
        {
            if (Plugin.Singleton.Config.ScpSpeech.Contains(instance._hub.characterClassManager.NetworkCurClass))
                instance._dissonanceSetup.MimicAs939 = b;
            return true;
        }
    }
}
