using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts.TitanBlood
{
    public class TitanBloodSplatter : MonoBehaviour
    {
        //private Bloodsplatter _origBloodsplatter;
        public ParticleSystem origParticleSystem;

        public Color origStartColour;
        public Color origEndColour;

        private void Start()
        {
            //_origBloodsplatter = GetComponent<Bloodsplatter>();
            origParticleSystem = GetComponent<ParticleSystem>();

            origStartColour = origParticleSystem.main.startColor.colorMax;
            origEndColour = origParticleSystem.main.startColor.colorMin;

            var main = origParticleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.6f, 0f, origStartColour.a), new Color(0.6f, 0.26f, 0f, origEndColour.a));
        }

        private void OnDestroy()
        {
            var main = origParticleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(origStartColour, origEndColour);
        }
    }
}
