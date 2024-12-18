using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    [ExecuteInEditMode]
    public class ParticleTeleporter : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        List<ParticleSystem.Particle> _enter = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> _exit = new List<ParticleSystem.Particle>();
        public float distance = -100;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }


        void OnParticleTrigger()
        {
            // get
            int numEnter = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enter);
            int numExit = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, _exit);

            // iterate
            for (int i = 0; i < numEnter; i++)
            {
                ParticleSystem.Particle p = _enter[i];

                var cColour = p.GetCurrentColor(_particleSystem);

                p.startColor = new Color32(cColour.r, cColour.g, cColour.b, 0);

                _enter[i] = p;
            }

            for (int i = 0; i < numExit; i++)
            {
                ParticleSystem.Particle p = _exit[i];

                var cColour = p.GetCurrentColor(_particleSystem);

                p.startColor = new Color32(cColour.r, cColour.g, cColour.b, 1);

                _exit[i] = p;
            }

            // set
            _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enter);
            _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, _exit);
        }
    }

    
}
