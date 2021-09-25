using HarmonyLib;
using Mirror;
using PlayableScps;
using PlayableScps.Messages;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(Scp939), nameof(Scp939.ServerReceivedVoiceMsg))]
    public static class SpeechPatch
    {
        public static bool Prefix(NetworkConnection conn, Scp939VoiceMessage msg)
        {
            if (!ReferenceHub.TryGetHubNetID(conn.identity.netId, out ReferenceHub hub))
                return false;
            
            if (!Plugin.Singleton.Config.ScpSpeech.Contains(hub.characterClassManager.NetworkCurClass))
                return true;

            hub.dissonanceUserSetup.MimicAs939 = msg.IsMimicking;
            return true;
        }
    }
}