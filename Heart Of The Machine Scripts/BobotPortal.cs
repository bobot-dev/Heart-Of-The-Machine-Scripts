using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class BobotPortal : MonoBehaviour
    {

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {

        }

        private Renderer _renderer;
        public RealtimeReflectionHelper reflectionHelper;
    }
}
