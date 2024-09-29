using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeartOfTheMachineScripts
{
    [HarmonyPatch(typeof(SpawnMenu))]
    public static class SpawnMenuPatches
    {
        [HarmonyPatch("RebuildMenu"), HarmonyPrefix]
        private static void RebuildMenu(SpawnMenu __instance)
        {
            if (SpawnMenuAdder.instance)
            {
                var enemiesList = __instance.objects.enemies.ToList();
                var objectsList = __instance.objects.objects.ToList();
                foreach (var spawnable in SpawnMenuAdder.instance.spawnables)
                {
                    spawnable.sandboxOnly = false;
                    if (spawnable.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy && !__instance.objects.enemies.Contains(spawnable))
                    {
                        enemiesList.Add(spawnable);
                    }

                    if (spawnable.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Object && !__instance.objects.objects.Contains(spawnable))
                    {
                        objectsList.Add(spawnable);
                    }
                }
                __instance.objects.enemies = enemiesList.ToArray();
                __instance.objects.objects = objectsList.ToArray();
            }
        }
    }
}
