using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;

namespace HeartOfTheMachineScripts
{
    public class MapVarToggle : MonoBehaviour
    {
        public string variableName;
        public bool defaultValue;


        private void Start()
        {
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(SetVar);

            if (MapVarManager.Instance.GetBool(variableName) == null)
                MapVarManager.Instance.SetBool(variableName, defaultValue, true);

            toggle.isOn = MapVarManager.Instance.GetBool(variableName).Value;
        }

        public void SetVar(bool state)
        {
            MapVarManager.Instance.SetBool(variableName, state, true);
        }
    }
}
