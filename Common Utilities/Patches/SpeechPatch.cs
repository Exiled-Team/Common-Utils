using Exiled.API.Features.Items;
using HarmonyLib;

namespace Common_Utilities.Patches
{
    //[HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static class SpeechPatch
    {
        public static bool Prefix(Radio __instance, bool b)
        {
            //TODO: Re-implement this.
            return true;
        }
    }
}
