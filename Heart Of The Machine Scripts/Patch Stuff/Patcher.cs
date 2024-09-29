using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class Patcher : MonoBehaviour
    {
        private static Harmony harmony;

        private void Patch()
        {
            Debug.Log("Trying to Patch...!");
            harmony = new Harmony("bobot.ultrakill.hotm.harmony");
            harmony.PatchAll();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            Debug.Log("Patched!");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            harmony.UnpatchSelf();
            instance = null;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0 != SceneManager.GetActiveScene())
                return;

            MachinePatcher.ClearCache();
        }

        private static Patcher instance;

        public static void QueryInstance()
        {
            if (instance != null)
                return;

            instance = new GameObject("OverseerPatcher").AddComponent<Patcher>();
            instance.Patch();
        }

    }
}
