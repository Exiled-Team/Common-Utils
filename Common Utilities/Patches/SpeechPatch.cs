using Assets._Scripts.Dissonance;
using HarmonyLib;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
    public static class SpeechPatch
    {
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();

            if (!HandleConfigLogic(ccm.CurClass)) return true;

            __instance.MimicAs939 = value;

            return true;
        }

        private static bool HandleConfigLogic(RoleType role)
        {
            switch(role)
            {
                case RoleType.Scp096:
                    return Plugin.Singleton.Config.CanShyGuySpeak;
                case RoleType.Scp173:
                    return Plugin.Singleton.Config.CanPeanutSpeak;
                case RoleType.Scp106:
                    return Plugin.Singleton.Config.CanLarrySpeak;
                case RoleType.Scp0492:
                    return Plugin.Singleton.Config.CanZombieSpeak;
                case RoleType.Scp049:
                    return Plugin.Singleton.Config.CanDoctorSpeak;
                default:
                    return true;
            }

        }
    }
}
