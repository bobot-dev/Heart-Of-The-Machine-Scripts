using HarmonyLib;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HeartOfTheMachineScripts.TitanBlood;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Yoga;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace HeartOfTheMachineScripts
{
    [HarmonyPatch(typeof(EnemyIdentifier))]
    public static class EnemyIdentifierPatches
    {
        /*[HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage)), HarmonyPrefix]
        private static void DeliverDamagePatch(EnemyIdentifier __instance, GameObject target, Vector3 hitPoint, float multiplier,
            float critMultiplier, GameObject sourceWeapon, bool ignoreTotalDamageTakenMultiplier, bool fromExplosion)
        {

            
            if (sourceWeapon)
            {
                if (!ignoreTotalDamageTakenMultiplier)
                {
                    multiplier *= __instance.totalDamageTakenMultiplier;
                }
                multiplier /= __instance.totalHealthModifier;
                if (__instance.isBoss && __instance.difficulty >= 4)
                {
                    if (__instance.difficulty == 5)
                    {
                        multiplier /= 2f;
                    }
                    else
                    {
                        multiplier /= 1.5f;
                    }
                }
                if (__instance.weaknesses.Length != 0)
                {
                    for (int i = 0; i < __instance.weaknesses.Length; i++)
                    {
                        if (__instance.hitter == __instance.weaknesses[i] ||
                            (__instance.hitterAttributes.Contains(HitterAttribute.Electricity) && __instance.weaknesses[i] == "electricity"))
                        {
                            multiplier *= __instance.weaknessMultipliers[i];
                        }
                    }
                }
                if (__instance.getFireDamageMultiplier && __instance.burners.Count > 0 && __instance.hitter != "fire" &&
                    __instance.hitter != "explosion" && __instance.hitter != "ffexplosion")
                {
                    multiplier *= 1.5f;
                }

                if (multiplier > 0)
                {

                }
            }
        }*/
        

        [HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DeliverDamagePatchIL(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            // IL_0362: ldloc.s 6
            // IL_0364: ldc.i4.s 9
            // IL_0366: beq IL_0454
            int insertionIndex = -1;
            bool labelDone = false;
            Label returnLabel = il.DefineLabel();
            
            int insertionPowerupIndex = -1;

            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (insertionIndex < 0 && 
                    codes[i].IsLdarg(0) &&
                    codes[i + 1].LoadsField(AccessTools.Field(typeof(EnemyIdentifier), nameof(EnemyIdentifier.enemyType))) &&
                    codes[i + 2].opcode == OpCodes.Stloc_S && (codes[i + 2].operand != null && ((LocalBuilder)codes[i + 2].operand).LocalIndex == 6))
                {
                    Debug.LogWarning($"found switch at {i} (fucking finally)");
                    insertionIndex = i;
                }

                if (!labelDone && 
                    codes[i].IsLdarg(0) &&
                    codes[i + 1].LoadsField(AccessTools.Field(typeof(EnemyIdentifier), nameof(EnemyIdentifier.enemyClass))) &&
                    codes[i + 2].IsStloc() && (codes[i + 2].operand != null && ((LocalBuilder)codes[i + 2].operand).LocalIndex == 7))
                {
                    Debug.LogWarning($"set label at {i} (fucking finally)");
                    codes[i].labels.Add(returnLabel);
                    labelDone = true;
                }

                // if (insertionPowerupIndex < 0 &&
                //     codes[i].IsLdarg(0) &&
                //     codes[i + 1].LoadsField(AccessTools.Field(typeof(EnemyIdentifier), nameof(EnemyIdentifier.beingZapped))) &&
                //     codes[i + 2].opcode == OpCodes.Brtrue)
                // {
                //     Debug.LogWarning($"found beingZapped at {i}");
                //     codes[i].labels.Add(returnPowerupLabel);
                //     insertionPowerupIndex = i;
                // }

                // if (insertionPowerupIndex < 0 &&
                //     codes[i].IsLdarg(4) &&
                //     codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 1.5f &&
                //     codes[i + 2].opcode == OpCodes.Mul &&
                //     codes[i + 3].IsStarg(4))
                // {
                //     Debug.LogWarning($"found beingZapped at {i}");
                //     
                //     insertionPowerupIndex = i + 3;
                // }

                if (insertionIndex >= 0 && labelDone)
                {
                    break;
                }
            }

            var instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(Transpilers.EmitDelegate<System.Func<EnemyIdentifier, bool>>(
                    eid => (eid.enemyType == EnemyType.Virtue && eid.TryGetComponent(out Machine machine) && MachinePatcher.IsCustom(machine)))),
                new CodeInstruction(OpCodes.Brtrue_S, returnLabel),
            };

            if (insertionIndex != -1)
            {
                codes.InsertRange(insertionIndex, instructionsToInsert);
            }

            Label cunt = il.DefineLabel();


            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].IsLdarg(0) &&
                    codes[i + 1].LoadsField(AccessTools.Field(typeof(EnemyIdentifier), nameof(EnemyIdentifier.beingZapped))) &&
                    codes[i + 2].opcode == OpCodes.Brtrue)
                {
                    Debug.LogWarning($"found beingZapped at {i}");
                    insertionPowerupIndex = i;
                    //codes[i].labels.Add(cunt);
                    break;
                }

            }

            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].LoadsField(AccessTools.Field(typeof(EnemyIdentifier), nameof(EnemyIdentifier.nails))) &&
                    codes[i + 1].opcode == OpCodes.Callvirt &&
                    codes[i + 2].opcode == OpCodes.Ldc_I4_S && codes[i + 2].operand.ToString() == "10")
                {
                    Debug.LogWarning($"found cunt at {i}");
                    codes[i + 3].operand = cunt;
                    break;
                }
            }

            var powerupInstructionsToInsert = new List<CodeInstruction>
            {
                //new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(new CodeInstruction(OpCodes.Ldarg_0) {labels = new List<Label> {cunt}, blocks = new List<ExceptionBlock>()}),
                new CodeInstruction(OpCodes.Ldarg_S, 3),
                new CodeInstruction(OpCodes.Ldarg_S, 4),
                new CodeInstruction(OpCodes.Ldarg_S, 7),
                new CodeInstruction(OpCodes.Ldarg_S, 9),
                new CodeInstruction(Transpilers.EmitDelegate<System.Action<EnemyIdentifier, Vector3, float, GameObject, bool>>(
                    delegate(EnemyIdentifier identifier, Vector3 hitPoint, float multiplier, GameObject sourceWeapon, bool fromExplosion)
                    {
                        if (StaticCharge.instance && StaticCharge.instance.Active && !identifier.dead && !identifier.blessed && multiplier > 0 && 
                            (sourceWeapon || identifier.hitter is "cannonball" or "punch" or "heavypunch"))
                        {
                            var damage = multiplier;

                            if (fromExplosion || identifier.hitter is "sawblade")// or "static charge")
                            {
                                damage /= 4;
                            }

                            //Debug.Log(damage);
                            StaticCharge.instance.AddCharge(damage, identifier, hitPoint);
                        }
                    })),
            };

            if (insertionPowerupIndex != -1)
            {
                codes.InsertRange(insertionPowerupIndex, powerupInstructionsToInsert);
            }

            for (var index = 0; index < codes.Count; index++)
            {
                //Debug.Log($"{index}: {codes[index]}");
            }

            return codes.AsEnumerable();
        }



        //fuck you

        /*[HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage)), HarmonyPrefix]
        private static void DeliverDamagePrePatch(EnemyIdentifier __instance)
        {
            if (__instance.TryGetComponent(out Machine machine) && MachinePatcher.IsCustom(machine))
            {
                Debug.Log("i hate you 1");
                __instance.enemyType = EnemyType.Mindflayer;
            }
        }

        [HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage)), HarmonyPostfix]
        private static void DeliverDamagePostPatch(EnemyIdentifier __instance)
        {
            if (__instance.TryGetComponent(out Machine machine) && MachinePatcher.IsCustom(machine))
            {
                Debug.Log("i hate you 2");
                __instance.enemyType = EnemyType.Virtue;
            }
        }*/


        [HarmonyPatch("ForceGetHealth"), HarmonyPrefix]
        private static bool ForceGetHealth(EnemyIdentifier __instance)
        {
            if (__instance.TryGetComponent(out Machine machine) && MachinePatcher.IsCustom(machine))
            {
                __instance.health = machine.health;
                return false;
            }

            return true;
        }

        [HarmonyPatch("InstaKill"), HarmonyPrefix]
        private static bool InstaKill(EnemyIdentifier __instance)
        {
            
            if (__instance.TryGetComponent(out Machine machine) && MachinePatcher.IsCustom(machine))
            {
                MachinePatcher.links[machine].Instakill();
                return false;
            }

            return true;
        }

        [HarmonyPatch("Explode"), HarmonyPrefix]
        private static bool Explode(EnemyIdentifier __instance)
        {
            if (__instance.TryGetComponent(out Overseer overseer))
            {
                overseer.Explode();
                return false;
            }

            return true;
        }

        [HarmonyPatch("Splatter"), HarmonyPrefix]
        private static bool Splatter(EnemyIdentifier __instance)
        {
            if (!InvincibleEnemies.Enabled && !__instance.blessed && __instance.TryGetComponent(out Overseer overseer))
            {
                overseer.GetHurt(__instance.gameObject, Vector3.zero, 999, 1, "Splatter");
                overseer.Explode();
                return false;
            }

            return true;
        }

        [HarmonyPatch("Start"), HarmonyPostfix]
        private static void StartPatch(EnemyIdentifier __instance)
        {
            if (__instance.TryGetComponent(out TitanBloodBuff titanBlood))
            {
                foreach (Renderer renderer in __instance.GetComponentsInChildren<Renderer>())
                {
                    if (!__instance.buffUnaffectedRenderers.Contains(renderer))
                    {
                        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(materialPropertyBlock);
                        materialPropertyBlock.SetFloat(EnemyIdentifier.HasSandBuff, 1f);
                        if (StaticRefs.sandTexture)
                        {
                            StaticRefs.sandTexture = materialPropertyBlock.GetTexture(Shader.PropertyToID("_SandTex"));
                        }
                        materialPropertyBlock.SetTexture(Shader.PropertyToID("_SandTex"), StaticRefs.Instance.titanBloodTexture);
                        renderer.SetPropertyBlock(materialPropertyBlock);
                    }
                }
            }
        }

        [HarmonyPatch("Sandify"), HarmonyPrefix]
        private static void SandifyPatch(EnemyIdentifier __instance)
        {
            if (!__instance.dead && __instance.TryGetComponent<TitanBloodBuff>(out var buff))
            {
                foreach (Renderer renderer in __instance.GetComponentsInChildren<Renderer>())
                {
                    if (!__instance.buffUnaffectedRenderers.Contains(renderer))
                    {
                        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(materialPropertyBlock);
                        if (StaticRefs.sandTexture)
                        {
                            materialPropertyBlock.SetTexture(Shader.PropertyToID("_SandTex"), StaticRefs.sandTexture);
                        }
                        renderer.SetPropertyBlock(materialPropertyBlock);
                    }
                }
                UnityEngine.Object.Destroy(buff);
            }
        }


        [HarmonyPatch(nameof(EnemyIdentifier.UpdateModifiers)), HarmonyPostfix]
        private static void UpdateModifiersPatch(EnemyIdentifier __instance)
        {
            if (__instance.TryGetComponent(out TitanBloodBuff titanBlood))
            {
                __instance.totalHealthModifier *= 1.5f;
                __instance.totalSpeedModifier *= 1.5f;
            }
        }

    }
}
