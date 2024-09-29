using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class SpawnMenuAdder : MonoSingleton<SpawnMenuAdder>
    {

        private void Awake()
        {
            Patcher.QueryInstance();
            //if (SpawnMenu.Instance.RebuildMenu())
        }

        public SpawnableObject[] spawnables;
    }
}
