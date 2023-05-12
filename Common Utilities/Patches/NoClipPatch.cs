using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;

namespace Common_Utilities.Patches
{
    [HarmonyPatch(typeof(FpcNoclipToggleMessage), nameof(FpcNoclipToggleMessage.ProcessMessage))]
    public class NoClipPatch
    {
        public static void SendActivateMessage(ReferenceHub hub)
        {
            var ply = Player.Get(hub);
            ply.ShowHint(Plugin.Singleton.Config.EnableHint);
        }

        public static void SendDeactivateMessage(ReferenceHub hub)
        {
            var ply = Player.Get(hub);
            ply.ShowHint(Plugin.Singleton.Config.DisableHint);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label add = generator.DefineLabel();
            Label skip = generator.DefineLabel();
            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Brfalse_S) - 1;

            newInstructions[index].WithLabels(skip);

            newInstructions.InsertRange(index, new List<CodeInstruction>()
        {
            new (OpCodes.Ldsfld, AccessTools.Field(typeof(Plugin), nameof(Plugin.cfg))),
            new (OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Config), nameof(Config.ScpSpeech))),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Ldfld, AccessTools.Field(typeof(ReferenceHub), nameof(ReferenceHub.roleManager))),
            new (OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole))),
            new (OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PlayerRoleBase), nameof(PlayerRoleBase.RoleTypeId))),
            new (OpCodes.Callvirt, AccessTools.Method(typeof(HashSet<RoleTypeId>), nameof(HashSet<RoleTypeId>.Contains))),
            new (OpCodes.Brfalse_S, skip),

            new (OpCodes.Call, AccessTools.PropertyGetter(typeof(Plugin), nameof(Plugin.ProximityToggled))),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Call, AccessTools.Method(typeof(List<ReferenceHub>), nameof(List<ReferenceHub>.Contains))),
            new (OpCodes.Brfalse_S, add),

            new (OpCodes.Call, AccessTools.PropertyGetter(typeof(Plugin), nameof(Plugin.ProximityToggled))),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Call, AccessTools.Method(typeof(NoClipPatch), nameof(NoClipPatch.SendDeactivateMessage))),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Callvirt, AccessTools.Method(typeof(List<ReferenceHub>), nameof(List<ReferenceHub>.Remove))),
            new (OpCodes.Pop),
            new (OpCodes.Br, skip),

            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Plugin), nameof(Plugin.ProximityToggled))).WithLabels(add),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Call, AccessTools.Method(typeof(NoClipPatch), nameof(NoClipPatch.SendActivateMessage))),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Callvirt, AccessTools.Method(typeof(List<ReferenceHub>), nameof(List<ReferenceHub>.Add)))
        });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}