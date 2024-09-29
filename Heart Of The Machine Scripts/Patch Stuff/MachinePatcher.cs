using GameConsole;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    [HarmonyPatch(typeof(Machine))]
    public static class MachinePatcher
    {
        //shamelessly stolen from https://github.com/Hydraxous/Carcass/blob/master/CarcassEnemy/Carcass/Carcass.cs

        private static HashSet<Machine> machineRelays = new HashSet<Machine>();
        public static Dictionary<Machine, IEnemy> links = new Dictionary<Machine, IEnemy>();

        public static void ClearCache()
        {
            machineRelays.Clear();
            links.Clear();
            eidLinks.Clear();
        }


        [HarmonyPatch("Start"), HarmonyPrefix]
        private static bool Start(Machine __instance)
        {
            Debug.Log("trying to link...");
            if (!__instance.TryGetComponent<IEnemy>(out IEnemy enemy))
                return true;

            if (!machineRelays.Contains(__instance))
            {
                machineRelays.Add(__instance);
                links.Add(__instance, enemy);
                if (__instance.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier eid))
                    eidLinks.Add(__instance, eid);

                __instance.health = enemy.GetHealth();

                Debug.Log("linked");
                //__instance.enabled = false; Dunno what effect this will cause, it works fine without doing this so...
                return false;
            }

            return true;
        }

        public static bool IsCustom(Machine machine)
        {
            return machineRelays.Contains(machine);
        }

        [HarmonyPatch("Update"), HarmonyPrefix]
        private static bool Update(Machine __instance)
        {
            if (!IsCustom(__instance))
                return true;

            __instance.health = links[__instance].GetHealth();

            return false;
        }

        [HarmonyPatch("FixedUpdate"), HarmonyPrefix]
        private static bool FixedUpdate(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        private static Dictionary<Machine, EnemyIdentifier> eidLinks = new Dictionary<Machine, EnemyIdentifier>();

        [HarmonyPatch(nameof(Machine.GetHurt)), HarmonyPrefix]
        private static bool GetHurt(Machine __instance, GameObject target, Vector3 force, float multiplier, float critMultiplier, GameObject sourceWeapon, bool fromExplosion)
        {
            if (!IsCustom(__instance))
                return true;

            string hitter = null;
            if (eidLinks.TryGetValue(__instance, out var link))
                hitter = link.hitter;

            //Pass logic to linked IEnemy, and clean up the parameters
            links[__instance]?.GetHurt(new HurtEventData(target, force, multiplier, critMultiplier, hitter, sourceWeapon, fromExplosion));

            return false;
        }

        [HarmonyPatch(nameof(Machine.GoLimp), typeof(bool)), HarmonyPrefix]
        private static bool GoLimp(Machine __instance)
        {
            if (!IsCustom(__instance))
                return true;

            //Pass logic to linked IEnemy
            links[__instance]?.Instakill();

            return false;
        }

        [HarmonyPatch(nameof(Machine.KnockBack)), HarmonyPrefix]
        private static bool KnockBack(Machine __instance, Vector3 force)
        {
            if (!IsCustom(__instance))
                return true;

            //Pass logic to linked IEnemy
            links[__instance]?.Knockback(force);

            return false;
        }

        [HarmonyPatch(nameof(Machine.CanisterExplosion)), HarmonyPrefix]
        private static bool CanisterExplosion(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("ReadyGib"), HarmonyPrefix]
        private static bool ReadyGib(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("StartHealing"), HarmonyPrefix]
        private static bool StartHealing(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("StopHealing"), HarmonyPrefix]
        private static bool StopHealing(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("StopKnockBack"), HarmonyPrefix]
        private static bool StopKnockBack(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("OnEnable"), HarmonyPrefix]
        private static bool OnEnable(Machine __instance)
        {
            return !IsCustom(__instance);
        }
    }
}
