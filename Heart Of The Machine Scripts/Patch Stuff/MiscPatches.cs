using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using HeartOfTheMachineScripts.TitanBlood;
using UnityEngine;

namespace HeartOfTheMachineScripts.Patch_Stuff
{
    [HarmonyPatch(typeof(ItemPlaceZone))]
    public static class ItemPlaceZonePatches
    {
        [HarmonyPatch("CheckItem"), HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        public static void CheckItemPatch(ItemPlaceZone __instance)
        {
            if (__instance.TryGetComponent(out ItemPlaceZoneThingForHotmChallenge zone) && !zone.acceptedTypes.Contains(__instance.acceptedItemType))
            {
                zone.Check(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(DeathZone))]
    public static class DeathZonePatches
    {
        [HarmonyPatch("OnTriggerEnter"), HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void OnTriggerEnterPatch(DeathZone __instance, Collider other)
        {
            if (other.gameObject.layer == 22 &&
                (other.attachedRigidbody ? other.attachedRigidbody.TryGetComponent(out ItemIdentifier itemIdentifier) : other.TryGetComponent(out itemIdentifier)) &&
                itemIdentifier.gameObject.name.ToLower().Contains("plushie") )
            {
                var objName = itemIdentifier.gameObject.name.ToLower();
                other.gameObject.SetActive(false);
                
                StaticRefs.plushiesKilled++;

                if (
                    (objName.Contains("bobot") && !StaticRefs.bobotKilled) ||
                    (objName.Contains("dz") && !StaticRefs.dzKilled) ||
                    (objName.Contains("bobilka") && !StaticRefs.bobilkaKilled) ||
                    (objName.Contains("oxblood") && !StaticRefs.oxbloodKilled)
                )
                {
                    if (objName.Contains("bobot"))
                        StaticRefs.bobotKilled = true;
                    if (objName.Contains("dz"))
                        StaticRefs.dzKilled = true;
                    if (objName.Contains("bobilka"))
                        StaticRefs.bobilkaKilled = true;
                    if (objName.Contains("oxblood"))
                        StaticRefs.oxbloodKilled = true;

                    HudMessageReceiver.Instance.SendHudMessage("<color=red>With this character's death, the thread of prophecy is severed. Restart the level to restore the weave of fate, or persist in the doomed world you have created.</color>");

                }

                if (StaticRefs.bobotKilled && StaticRefs.bobilkaKilled && StaticRefs.oxbloodKilled && StaticRefs.dzKilled)
                {
                    OptionsManager.forceRadiance = true;
                    OptionsManager.forceSand = true;

                    HudMessageReceiver.Instance.SendHudMessage("ok wow i see how it is :)");

                }
            }
        }
    }

    [HarmonyPatch(typeof(FistControl))]
    public static class FistControlPatches
    {
        [HarmonyPatch("ResetHeldItemPosition"), HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void ResetHeldItemPositionPatch(FistControl __instance)
        {
            if (__instance.heldObject.TryGetComponent(out UniquePickupHoldPos uniqueHoldPos) && uniqueHoldPos.requiredArm == __instance.currentPunch.type)
            {
                __instance.heldObject.transform.localPosition = uniqueHoldPos.putDownPosition;
                __instance.heldObject.transform.localRotation = Quaternion.Euler(uniqueHoldPos.putDownRotation);
            }
        }

    }

    //[HarmonyPatch(typeof(EnemySimplifier))]
    public static class EnemySimplifierPatches
    {
        [HarmonyPatch("SetOutline"), HarmonyTranspiler]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<CodeInstruction> ResetHeldItemPositionIL(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            object returnLabel = null;

            for (int i = 0; i < codes.Count - 2; i++)
            {

                if (codes[i].IsLdarg(0) && codes[i + 1].IsLdloc() && codes[i + 2].StoresField(AccessTools.Field(typeof(EnemySimplifier), nameof(EnemySimplifier.lastSandified))))
                {
                    Debug.LogWarning($"found EnemySimplifier.lastSandified at {i}");
                    insertionIndex = i;

                    //break;
                }
            }

            var instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),

                new CodeInstruction(Transpilers.EmitDelegate<Action<EnemySimplifier>>(
                    (self) =>
                    {
                        if (self.eid.GetComponent<TitanBloodBuff>())
                        {
                            self.lastSandified = true;
                        }
                    })),
            };

            if (insertionIndex != -1)
            {
                codes.InsertRange(insertionIndex + 3, instructionsToInsert);
            }

            for (var index = 0; index < codes.Count; index++)
            {
                Debug.Log($"{index}: {codes[index]}");
            }

            return codes.AsEnumerable();
        }


    }



    [HarmonyPatch(typeof(Punch))]
    public static class PunchPatches
    {
        [HarmonyPatch("ResetHeldItemPosition"), HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void ResetHeldItemPositionPatch(Punch __instance)
        {
            if (__instance.heldItem.gameObject.TryGetComponent(out UniquePickupHoldPos uniqueHoldPos) && uniqueHoldPos.requiredArm == __instance.type)
            {
                __instance.heldItem.transform.localPosition = uniqueHoldPos.putDownPosition;
                __instance.heldItem.transform.localRotation = Quaternion.Euler(uniqueHoldPos.putDownRotation);
            }
        }
    }

    [HarmonyPatch(typeof(WeaponPos))]
    public static class WeaponPosPatches
    {
        [HarmonyPatch("CheckPosition"), HarmonyTranspiler]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<CodeInstruction> CheckPositionIL(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            object returnLabel = null;

            for (int i = 0; i < codes.Count - 2; i++)
            {

                if (codes[i].Calls(AccessTools.PropertyGetter(typeof(PowerUpMeter), nameof(PowerUpMeter.Instance))))
                {
                    Debug.LogWarning($"found PowerUpMeter.Instance at {i}");
                    returnLabel ??= codes[i + 2].operand;

                    //break;
                }

                if (codes[i].LoadsField(AccessTools.Field(typeof(PowerUpMeter), nameof(PowerUpMeter.juice))))
                {
                    Debug.LogWarning($"found PowerUpMeter.juice at {i}");
                    //codes[i + 3].labels.Add(returnLabel);
                    insertionIndex = i - 4;
                    break;
                }
            }

            var instructionsToInsert = new List<CodeInstruction>
            {
                //new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(Transpilers.EmitDelegate<Func<bool>>(
                    () =>
                    {
                        Debug.Log(StaticCharge.Instance.Active);
                        return !(PowerUpMeter.Instance?.juice > 0f && StaticCharge.Instance.Active);
                    })),
                new CodeInstruction(OpCodes.Brfalse_S, returnLabel),
            };

            if (insertionIndex != -1)
            {
                codes.InsertRange(insertionIndex, instructionsToInsert);
            }

            for (var index = 0; index < codes.Count; index++)
            {
                //Debug.Log($"{index}: {codes[index]}");
            }

            return codes.AsEnumerable();
        }

    }
}