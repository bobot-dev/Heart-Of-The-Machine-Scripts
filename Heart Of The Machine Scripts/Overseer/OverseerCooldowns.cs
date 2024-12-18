using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace HeartOfTheMachineScripts
{
    public class OverseerCooldowns : MonoSingleton<OverseerCooldowns>
    {
        private void Start()
        {
            SlowUpdate();
        }

        public int GetMaxAttackAmount(int difficulty)
        {
            int maxAttacks = 0;

             switch (difficulty)
             {
                default: maxAttacks = 2; break;
                case 2: maxAttacks = 3; break;
                case 3: maxAttacks = 4; break;
                case >= 4: maxAttacks = 5; break;
            }

            maxAttacks += currentOverseers.Count - 1;

            return maxAttacks;
        }

        private void Update()
        {
            if (overseerCooldown > 0f)
            {
                overseerCooldown = Mathf.MoveTowards(overseerCooldown, 0f, Time.deltaTime);
            }
        }

        private void SlowUpdate()
        {
            Invoke("SlowUpdate", 10f);
            for (int i = currentOverseers.Count - 1; i >= 0; i--)
            {
                if (currentOverseers[i] == null || !currentOverseers[i].gameObject.activeInHierarchy)
                {
                    currentOverseers.RemoveAt(i);
                }
            }

            for (int i = insignias.Count - 1; i >= 0; i--)
            {
                if (insignias[i] == null || !insignias[i].gameObject.activeInHierarchy)
                {
                    insignias.RemoveAt(i);
                }
            }
        }

        public void AddOverseer(Overseer drn)
        {
            if (currentOverseers.Count <= 0 || !currentOverseers.Contains(drn))
            {
                currentOverseers.Add(drn);
            }
        }

        public void RemoveOverseer(Overseer drn)
        {
            if (currentOverseers.Count > 0 && currentOverseers.Contains(drn))
            {
                currentOverseers.Remove(drn);
            }
        }

        public void RemoveOverseerInsignia(OverseerInsignia insignia)
        {
            if (insignias.Count > 0 && insignias.Contains(insignia))
            {
                insignias.Remove(insignia);
            }
        }


        public float overseerCooldown;

        public int overseerUsedBeams;

        public List<OverseerInsignia> insignias = new List<OverseerInsignia>();

        public List<Overseer> currentOverseers = new List<Overseer>();
    }

}
