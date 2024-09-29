using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public interface IEnemy
    {
        public void Instakill();
        public void GetHurt(HurtEventData hurtEventData);
        public void Knockback(Vector3 force);
        public float GetHealth();
        public bool IsAlive();
        public float GetLocationCritDamageMultiplier(string location);
        public EnemyIdentifier GetEnemyIdentifier();
    }

    public struct HurtEventData
    {

        public HurtEventData(GameObject target, Vector3 force, float multiplier, float critMultiplier, string hitter, GameObject sourceWeapon = null, bool fromExplosion = false)
        {
            this.target = target;
            this.force = force;
            this.multiplier = multiplier;
            this.critMultiplier = critMultiplier;
            this.sourceWeapon = sourceWeapon;
            this.fromExplosion = fromExplosion;
            this.hitter = hitter;
        }

        public GameObject target;
        public Vector3 force;
        public float multiplier;
        public float critMultiplier;
        public GameObject sourceWeapon;
        public string hitter;
        public bool fromExplosion;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("multiplier: ");
            sb.Append(multiplier);
            sb.Append("\ncrit: ");
            sb.Append(critMultiplier);
            sb.Append("\nhitter: ");
            sb.Append(hitter);
            sb.Append("\nforce: ");
            sb.Append(force);
            sb.Append("\nfromExplosion: ");
            sb.Append(fromExplosion);
            if (target != null)
            {
                sb.Append("\ntarget: ");
                sb.Append(target.name);
            }

            if (sourceWeapon != null)
            {
                sb.Append("\nsrcWep: ");
                sb.Append(sourceWeapon.name);
            }

            return sb.ToString();
        }
    }
}
