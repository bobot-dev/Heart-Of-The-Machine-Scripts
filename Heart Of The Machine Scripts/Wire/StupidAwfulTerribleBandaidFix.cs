using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class StupidAwfulTerribleBandaidFix : MonoBehaviour
    {
        private void Start()
        {
            Invoke("ThisIsStupid", 3);
        }

        private void ThisIsStupid()
        {
            GetComponent<Collider>().isTrigger = false;
        }
    }
}
