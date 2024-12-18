using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class ReactorHeatDisplay : MonoBehaviour
    {
        private TMP_Text _text;

        public string suffix = "<sup>o</sup>C";
        public float temperature = 500;
        public float speed = 1;

        [SerializeField]
        private bool _fucked;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();

            _text.text = $"{Mathf.FloorToInt(temperature)}{suffix}";
        }

        public void FuckShitUp()
        {
            _fucked = true;
        }

        private void Update()
        {
            if (_fucked)
            {
                temperature += Time.deltaTime * speed;
                _text.text = $"{Mathf.FloorToInt(temperature)}{suffix}";
            }
        }
    }
}
