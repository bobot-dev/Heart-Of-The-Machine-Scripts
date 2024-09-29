using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts.Workers
{
    public class WorkerWaterStuff : MonoBehaviour
    {
        public ParticleSystem part;
        public List<ParticleCollisionEvent> collisionEvents;

        private void Start()
        {
            collisionEvents = new List<ParticleCollisionEvent>();
        }

        private void OnParticleCollision(GameObject other)
        {
            //Debug.Log("hi");
            if (other.TryGetComponent(out WaterSurface bloodAbsorber))
            {
                Vector3 position = part.transform.position;
                part.GetCollisionEvents(other, collisionEvents);
                bloodAbsorber.ProcessWasherSpray(ref collisionEvents, position);
            }
            // else if (other.TryGetComponent(out BloodAbsorberChild bloodAbsorberChild))
            // {
            //     Vector3 position2 = this.part.transform.position;
            //     this.part.GetCollisionEvents(other, this.collisionEvents);
            //     bloodAbsorberChild.ProcessWasherSpray(ref this.collisionEvents, position2);
            //     SpinFromForce spinFromForce;
            //     if (other.TryGetComponent<SpinFromForce>(out spinFromForce))
            //     {
            //         spinFromForce.AddSpin(ref this.collisionEvents);
            //     }
            // }
        }
    }
}
