namespace Common_Utilities.Patches;

using Exiled.API.Features.Items;
using HarmonyLib;

// [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
public static class SpeechPatch
{
    public static bool Prefix(Radio instance, bool b)
    {
        // TODO: Re-implement this.
        return true;
    }
}