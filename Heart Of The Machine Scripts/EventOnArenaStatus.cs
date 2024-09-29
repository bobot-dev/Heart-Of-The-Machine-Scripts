using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class EventOnArenaStatus : MonoBehaviour
    {
        public void Update()
        {
            if (_prevStatus != status.currentStatus)
            {
                if (status.currentStatus >= targetStatus && !_hasTrigger)
                {
                    onStatus?.Invoke();
                    _hasTrigger = true;
                }
                else if (status.currentStatus < targetStatus && _hasTrigger)
                {
                    onStatus?.Revert();
                    _hasTrigger = false;
                }
            }

            _prevStatus = status.currentStatus;
        }

        private bool _hasTrigger;
        private int _prevStatus;
        public int targetStatus;
        public UltrakillEvent onStatus;
        public ArenaStatus status;
    }
}
