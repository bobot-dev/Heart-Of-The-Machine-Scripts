using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;

namespace HeartOfTheMachineScripts
{
    public class ChainLightning : MonoBehaviour
    {
        public EnemyIdentifier target;
        public EnemyIdentifier self;
        public float delay;
        public float damage;

        private void Start()
        {
            Invoke("Trigger", delay);
        }
        public void Trigger()
        {
            if (!target)
                return;

            Vector3 selfPos = self.overrideCenter ? self.overrideCenter.position : self.transform.position;
            Vector3 otherPos = target.overrideCenter ? target.overrideCenter.position : target.transform.position;
            var midpoint = Vector3.Lerp(selfPos, otherPos, 0.5f);


            target.hitter = "static charge";
            //enemyIdentifier.hitterAttributes.Add(HitterAttribute.Electricity);
            target.DeliverDamage(target.gameObject, Vector3.zero, otherPos, damage, true);
            LineRenderer lineRenderer = Instantiate(DefaultReferenceManager.Instance.electricLine, midpoint, Quaternion.identity);
            lineRenderer.SetPosition(0, selfPos);
            lineRenderer.SetPosition(1, otherPos);


        }
    }
}
