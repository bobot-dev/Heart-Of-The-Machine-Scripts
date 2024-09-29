using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    class Credits : MonoBehaviour
    {
        void Start()
        {
            if (debug || (SteamClient.IsValid && SteamClient.SteamId == ID))
            {
                triggered = true;
            }
        }

        void Update()
        {
            if (!triggered)
            {
                gameObject.SetActive(false);
            }
        }

        void OnDisabled()
        {
            if (triggered)
            {
                gameObject.SetActive(true);
            }
        }

        private bool triggered = false;
        public bool debug;

        private static readonly ulong ID = 76561199195414858UL;
    }
}
