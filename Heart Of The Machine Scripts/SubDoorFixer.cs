using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Door))]
    public class SubDoorFixer : MonoBehaviour
    {
        public SubDoor[] doors;

        private void Start()
        {
            var door = GetComponent<Door>();

            door.subdoors = doors;
        }
    }
}
