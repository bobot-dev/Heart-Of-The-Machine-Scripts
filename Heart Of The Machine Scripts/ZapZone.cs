using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeartOfTheMachineScripts
{
    public class ZapZone : MonoBehaviour
    {
        private void Start()
        {
            _col = GetComponent<Collider>();
        }

        private void OnDisable()
        {
            _enemies.Clear();
        }

        private void FixedUpdate()
        {
            if (!enabled)
            {
                return;
            }


            if (_enemies.Count <= 0) return;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null || _enemies[i].target == null || !_enemies[i].HasLimbs(_col))
                {
                    _enemies.RemoveAt(i);
                }
                else
                {
                    float num = _enemies[i].timer;
                    num -= Time.deltaTime;
                    if (num <= 0f)
                    {
                        if (!DamageEnemy(_enemies[i].target, i))
                        {
                            break;
                        }
                        num = hurtCooldown;
                    }

                    _enemies[i].timer = num;
                }
            }
        }

        private bool DamageEnemy(EnemyIdentifier eid, int i)
        {
            eid.hitter = "zapper";
            eid.hitterAttributes.Add(HitterAttribute.Electricity);

            GameObject gameObject = _enemies[i].limbs[_enemies[i].limbs.Count - 1].gameObject;

            if (eid.dead)
            {
                if (eid.enemyClass == EnemyClass.Demon || (eid.enemyClass == EnemyClass.Machine && eid.machine && !eid.machine.dismemberment))
                {
                    _enemies.RemoveAt(i);
                    return false;
                }

                if (gameObject == eid.gameObject || gameObject.layer == 12 || gameObject.layer == 20 || gameObject.CompareTag("Body"))
                {
                    _enemies[i].limbs.RemoveAt(_enemies[i].limbs.Count - 1);
                }
            }

            Vector3 vector = eid.transform.position;
            
            if (hurtParticle && !eid.dead)
            {
                Instantiate(hurtParticle, vector, Quaternion.identity);
            }

            eid.DeliverDamage(gameObject, Vector3.zero, vector, setDamage / 2f, false);
            

            return true;
        }

        private HurtZoneEnemyTracker EnemiesContains(EnemyIdentifier eid)
        {
            if (_enemies.Count == 0)
            {
                return null;
            }

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null || _enemies[i].target == null)
                {
                    _enemies.RemoveAt(i);
                }
                else if (_enemies[i].target == eid)
                {
                    return _enemies[i];
                }
            }

            return null;
        }

        private void Enter(Collider other)
        {

            if (other.gameObject.layer is 10 or 11 or 12 or 20)
            {
                EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                if (other.gameObject.layer == 12)
                {
                    enemyIdentifierIdentifier = other.gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>();
                }
                else if (other.gameObject.layer == 20 && other.transform.parent)
                {
                    enemyIdentifierIdentifier =
                        other.transform.parent.GetComponentInChildren<EnemyIdentifierIdentifier>();
                }
                else
                {
                    enemyIdentifierIdentifier = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
                }

                if (enemyIdentifierIdentifier != null && enemyIdentifierIdentifier.eid != null &&
                    (ignoredEnemyTypes.Count == 0 || !ignoredEnemyTypes.Contains(enemyIdentifierIdentifier.eid.enemyType)) &&
                    (!enemyIdentifierIdentifier.eid.dead || (enemyIdentifierIdentifier.eid.enemyClass != EnemyClass.Demon &&
                    other.gameObject.layer != 12 && other.gameObject.layer != 20)) && 
                    enemyIdentifierIdentifier.transform.localScale != Vector3.zero)
                {
                    

                    HurtZoneEnemyTracker hurtZoneEnemyTracker = EnemiesContains(enemyIdentifierIdentifier.eid);
                    if (hurtZoneEnemyTracker == null)
                    {
                        _enemies.Add(new HurtZoneEnemyTracker(enemyIdentifierIdentifier.eid, other, hurtCooldown));
                        if (enabled)
                        {
                            DamageEnemy(enemyIdentifierIdentifier.eid, _enemies.Count - 1);
                        }

                    }
                    else
                    {
                        hurtZoneEnemyTracker.limbs.Add(other);
                    }
                }
            }
        }

        private void Exit(Collider other)
        {

            
            if (other.gameObject.layer is 10 or 11 or 12 or 20)
            {
                EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                if (other.gameObject.layer == 12)
                {
                    enemyIdentifierIdentifier = other.gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>();
                }
                else if (other.gameObject.layer == 20 && other.transform.parent)
                {
                    enemyIdentifierIdentifier =
                        other.transform.parent.GetComponentInChildren<EnemyIdentifierIdentifier>();
                }
                else
                {
                    enemyIdentifierIdentifier = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
                }

                if (enemyIdentifierIdentifier != null && enemyIdentifierIdentifier.eid != null)
                {
                    HurtZoneEnemyTracker hurtZoneEnemyTracker = EnemiesContains(enemyIdentifierIdentifier.eid);
                    if (hurtZoneEnemyTracker == null || !hurtZoneEnemyTracker.limbs.Contains(other))
                    {
                        return;
                    }

                    hurtZoneEnemyTracker.limbs.Remove(other);
                    if (!hurtZoneEnemyTracker.HasLimbs(_col))
                    {
                        _enemies.Remove(hurtZoneEnemyTracker);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (trigger || other.gameObject.layer == 20)
            {
                Enter(other);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!trigger)
            {
                Enter(other.collider);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (trigger || other.gameObject.layer == 20)
            {
                Exit(other);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (!trigger)
            {
                Exit(other.collider);
            }
        }

        public bool trigger;

        private Collider _col;

        public float hurtCooldown = 1f;

        public float setDamage;

        private List<HurtZoneEnemyTracker> _enemies = new List<HurtZoneEnemyTracker>();

        public GameObject hurtParticle;

        public List<EnemyType> ignoredEnemyTypes = new List<EnemyType>();
    }
}
