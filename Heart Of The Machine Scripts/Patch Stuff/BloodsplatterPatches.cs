using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HeartOfTheMachineScripts.TitanBlood;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;
using UnityEngine.ProBuilder;

namespace HeartOfTheMachineScripts
{
    [HarmonyPatch(typeof(Bloodsplatter))]
    public static class BloodsplatterPatches
    {
        [HarmonyPatch(nameof(Bloodsplatter.Collide)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CollideIL(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            Label returnLabel = il.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(AccessTools.PropertyGetter(typeof(NewMovement), nameof(NewMovement.Instance))))
                {
                    Debug.LogWarning($"found NewMovement.Instance at {i}");
                    codes[i + 6].labels.Add(returnLabel);
                    insertionIndex = i;
                    break;
                }
            }

            var instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(Transpilers.EmitDelegate<Func<Bloodsplatter, bool>>(
                    delegate(Bloodsplatter bloodsplatter)
                    {
                        if (bloodsplatter.GetComponent<TitanBloodSplatter>())
                        {
                            StaticRefs.GetHealthOverheal(Mathf.RoundToInt(bloodsplatter.hpAmount * 1.5f), false, bloodsplatter.fromExplosion);
                            //bloodsplatter.DisableCollider();

                            return true;
                        }

                        return false;

                    })),
                new CodeInstruction(OpCodes.Brfalse_S, returnLabel),
            };

            if (insertionIndex != -1)
            {
                codes.InsertRange(insertionIndex, instructionsToInsert);
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(nameof(Bloodsplatter.CreateBloodstain)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CreateBloodstainIL(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            Label returnLabel = il.DefineLabel();

            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].Calls(AccessTools.PropertyGetter(typeof(NewMovement), nameof(NewMovement.Instance))))
                {
                    Debug.LogWarning($"found NewMovement.Instance at {i}");
                    codes[i + 6].labels.Add(returnLabel);
                    insertionIndex = i;
                    break;
                }
            }

            var instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(Transpilers.EmitDelegate<Func<Bloodsplatter, bool>>(
                    delegate(Bloodsplatter bloodsplatter)
                    {
                        if (bloodsplatter.GetComponent<TitanBloodSplatter>())
                        {
                            StaticRefs.GetHealthOverheal(5, false, bloodsplatter.fromExplosion);

                            return true;
                        }

                        return false;

                    })),
                new CodeInstruction(OpCodes.Brtrue_S, returnLabel),
            };

            if (insertionIndex != -1)
            {
                codes.InsertRange(insertionIndex, instructionsToInsert);
            }

            insertionIndex = -1;
            returnLabel = il.DefineLabel();
            var overrideLabel = il.DefineLabel();

            for (int i = 0; i < codes.Count - 7; i++)
            {
                if (codes[i].IsLdarg(0) &&
                    codes[i + 1].LoadsField(AccessTools.Field(typeof(Bloodsplatter), nameof(Bloodsplatter.bsm))) &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 4].opcode == OpCodes.Ldloc_S &&
                    codes[i + 6].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i + 7].Calls(AccessTools.Method(typeof(BloodsplatterManager), nameof(BloodsplatterManager.CreateBloodstain)))

                   )
                {
                    Debug.LogWarning($"found CreateBloodstain at {i}");
                    codes[i + 9].labels.Add(returnLabel);
                    insertionIndex = i;
                    //break;
                }

                if (codes[i].opcode == OpCodes.Ldloc_2 &&
                    codes[i + 1].operand == "GlassFloor" &&
                    codes[i + 2].Calls(AccessTools.Method(typeof(GameObject), nameof(GameObject.CompareTag))) &&
                    codes[i + 3].opcode == OpCodes.Brfalse &&
                    codes[i + 4].opcode == OpCodes.Ldloc_2
                   )
                {
                    Debug.LogWarning($"found stupid label at {i}");
                    codes[i + 3].operand = overrideLabel;
                }
            }

            instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0) {labels = new List<Label> {overrideLabel}, blocks = new List<ExceptionBlock>()},
                new CodeInstruction(OpCodes.Ldloc_S, 8),
                new CodeInstruction(OpCodes.Ldloc_S, 7),
                new CodeInstruction(Transpilers.EmitDelegate<Func<Bloodsplatter, Vector3, Vector3, bool>>(
                    delegate(Bloodsplatter bloodsplatter, Vector3 v, Vector3 normal)
                    {

                        return false;
                        if (bloodsplatter.GetComponent<TitanBloodSplatter>())
                        {
                            StaticRefs.Instance.CreateIchorBloodstain(v, normal);

                            return true;
                        }

                        return false;

                    })),
                new CodeInstruction(OpCodes.Brtrue_S, returnLabel),
            };

            if (insertionIndex != -1)
            {
                codes.InsertRange(insertionIndex, instructionsToInsert);
            }

            for (var index = 0; index < codes.Count; index++)
            {
                Debug.Log($"{index}: {codes[index]}");
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(nameof(Bloodsplatter.Repool)), HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        private static void PatchRepool(Bloodsplatter __instance)
        {
            if (__instance.TryGetComponent(out TitanBloodSplatter titanBlood))
            {
                Object.Destroy(titanBlood);
            }
        }
    }

    [HarmonyPatch(typeof(BloodsplatterManager))]
    public static class BloodsplatterManagerPatches
    {
        [HarmonyPatch(nameof(BloodsplatterManager.PrepareGore)), HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        private static void PatchPrepareGore(BloodsplatterManager __instance, GameObject gob, EnemyIdentifier eid)
        {
            if (eid.TryGetComponent(out TitanBloodBuff titanBlood))
            {
                var splatter = gob.AddComponent<TitanBloodSplatter>();

                splatter.origParticleSystem = splatter.GetComponent<ParticleSystem>();

                splatter.origStartColour = splatter.origParticleSystem.main.startColor.colorMax;
                splatter.origEndColour = splatter.origParticleSystem.main.startColor.colorMin;

                var main = splatter.origParticleSystem.main;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.6f, 0f, splatter.origStartColour.a), new Color(0.6f, 0.26f, 0f, splatter.origEndColour.a));
            }
        }

        [HarmonyPatch(nameof(BloodsplatterManager.LateUpdate)), HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void PatchLateUpdate(BloodsplatterManager __instance)
        {
            StaticRefs.Instance.ichorParentBuffer.SetData(StaticRefs.Instance.ichorParents);
            Graphics.DrawMeshInstancedProcedural(__instance.stainMesh, 0, StaticRefs.Instance.ichorBloodStain, __instance.bloodstainBounds,
                StaticRefs.Instance.ichorProps.Length);
        }
    }
}
