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

            if (!HandleConfigLogic(ccm.CurClass)) return true;

            __instance.MimicAs939 = value;

            return true;
        }

        private static bool HandleConfigLogic(RoleType role)
        {
            switch(role)
            {
                case RoleType.Scp096:
                    return Plugin.Singleton.Config.CanScp096Speak;
                case RoleType.Scp173:
                    return Plugin.Singleton.Config.CanScp173Speak;
                case RoleType.Scp106:
                    return Plugin.Singleton.Config.CanScp106Speak;
                case RoleType.Scp049:
                    return Plugin.Singleton.Config.CanScp0492Speak;
                default:
                    return true;
            }

        }
    }
}
