using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace HeartOfTheMachineScripts
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance)]
    public class StaticCharge : MonoSingleton<StaticCharge>
    {

        public void StartEffect(float juiceAmount)
        {
            if (!_meter)
            {
                _meter = PowerUpMeter.Instance;
            }

            if (juiceAmount == 0f)
            {
                juiceAmount = 30f;
            }

            if (_meter.juice < juiceAmount)
            {
                _meter.latestMaxJuice = juiceAmount;
                _meter.juice = juiceAmount;
            }
            _meter.powerUpColor = new Color(0f, 0.5f, 0.8f);

            amount++;
            _active = true;
        }

        
        private void Update()
        {

            if (_active && _remainingCooldown > 0)
            {
                _remainingCooldown = Mathf.MoveTowards(_remainingCooldown, 0, Time.deltaTime);
            }

            if (_active && _meter.juice <= 0f)
            {
                EndPowerUp();
            }
        }


        public void EndPowerUp()
        {
            amount = 0;
            _charge = 0;
            _storedCooldownDamage = 0;
            _remainingCooldown = 0;
            _active = false;
        }

        public void AddCharge(float damage, EnemyIdentifier eid, Vector3 hitPoint)
        {
            _charge += damage * amount;
            Vector3 vector = hitPoint == Vector3.zero ? eid.overrideCenter ? eid.overrideCenter.position : eid.transform.position : hitPoint;

            _storedCooldownDamage += damage;

            if (_remainingCooldown <= 0 || damage >= 1)
            {
                var zap = Instantiate(DefaultReferenceManager.Instance.zapImpactParticle, vector, Quaternion.identity);
                zap.transform.localScale *= 0.3f;

                HandleChainLightning(eid, new List<EnemyIdentifier>(), baseLength + amount, _storedCooldownDamage / 2);

                _remainingCooldown = cooldown;
                _storedCooldownDamage = 0;
            }

            if (_charge >= damageThreshold)
            {
                Instantiate(lightning, vector, Quaternion.identity);
                _charge = 0;
            }



        }

        public void HandleChainLightning(EnemyIdentifier chainOriginTarget, List<EnemyIdentifier> chainMembers, int length, float damage)
        {

            //Debug.Log(length);

            chainMembers.Add(chainOriginTarget);
            var position = chainOriginTarget.overrideCenter ? chainOriginTarget.overrideCenter.position : chainOriginTarget.transform.position;
            float distance = float.PositiveInfinity;
            EnemyIdentifier target = null;

            foreach (EnemyIdentifier enemyIdentifier in EnemyTracker.Instance.GetCurrentEnemies())
            {
                if (!chainMembers.Contains(enemyIdentifier) && !enemyIdentifier.blessed)
                {
                    var vector = enemyIdentifier.overrideCenter ? enemyIdentifier.overrideCenter.position : enemyIdentifier.transform.position;
                    var magnitude = (vector - position).magnitude;

                    if (magnitude < distance
                        && !Physics.Raycast(position, vector - position, Vector3.Distance(position, vector), LayerMaskDefaults.Get(LMD.Environment)))
                    {
                        target = enemyIdentifier;
                        distance = magnitude;
                    }
                }
            }

            if (target)
            {
                var origLength = baseLength + amount;

                var chainLightning = chainOriginTarget.gameObject.AddComponent<ChainLightning>();

                chainLightning.damage = damage;
                chainLightning.self = chainOriginTarget;
                chainLightning.target = target;
                chainLightning.delay = (origLength - length) * 0.1f;


                if (length > 0)
                    HandleChainLightning(target, chainMembers, length - 1, damage);
            }
        }

        private PowerUpMeter _meter;
        [HideInInspector]
        public int amount;
        private bool _active;
        private float _charge;

        public float cooldown = 0.1f;
        public int baseLength = 1;

        private float _storedCooldownDamage;
        private float _remainingCooldown;

        public float damageThreshold = 30;
        public GameObject lightning;
        public GameObject lightningSmall;
        public bool Active => _active;
    }

}
