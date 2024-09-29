using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.ProBuilder;
using UnityEngine;
using Random = UnityEngine.Random;
using RaycastHit = UnityEngine.RaycastHit;

namespace HeartOfTheMachineScripts
{
    public class OverseerInsignia  : MonoBehaviour
    {
        private void Start()
        {
            _offset = Random.Range(-0.1f, 0.1f);
            _sprites = GetComponentsInChildren<SpriteRenderer>();
            _explosionWidth = explosion.transform.localScale.x;
            if (parentOverseer)
            {
                //parentOverseer.childVi.Add(this);
                hadParent = true;
            }
            if (otherParent)
            {
                hadParent = true;
            }
            for (int i = 1; i < _sprites.Length; i++)
            {
                _sprites[i].gameObject.SetActive(false);
                _sprites[i].transform.localPosition = Vector3.zero;
            }
            if (predictiveVersion && predictive)
            {
                foreach (SpriteRenderer spriteRenderer in _sprites)
                {
                    spriteRenderer.sprite = predictiveVersion;
                }
            }
            _lit = GetComponent<Light>();

            if (triggerTogether) return;

            if (!noTracking)
            {
                Invoke("Activating", 2f / windUpSpeedMultiplier);
                return;
            }
            
            Activating();
        }

        private void Update()
        {

            if (triggerTogether && _idle)
            {

                transform.Rotate(Vector3.up, Time.deltaTime * 720f, Space.Self);
                _currentDistance = Mathf.MoveTowards(_currentDistance, 1f, Time.deltaTime * (4 * (1f - _currentDistance)));
                for (int i = 1; i < _sprites.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        _sprites[i].transform.localPosition = Vector3.up * 3f * Mathf.Lerp(0f, (i / 2 * -1), _currentDistance);
                    }
                    else
                    {
                        _sprites[i].transform.localPosition = Vector3.up * 3f * Mathf.Lerp(0f, (i + 1) / 2, _currentDistance);
                    }
                }

                if (!noTracking)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.position + Vector3.up * _offset, Time.deltaTime * 50f + Time.deltaTime * Vector3.Distance(transform.position, target.position) * 100f);
                    transform.Rotate(Vector3.up, Time.deltaTime * 180f, Space.Self);
                }

                return;
            }

            if (!_activating)
            {
                if (_activationTime < 1f)
                {
                    if (hadParent && !parentOverseer && !otherParent)
                    {
                        Destroy(gameObject);
                    }
                    transform.position = Vector3.MoveTowards(transform.position, target.position + Vector3.up * _offset, Time.deltaTime * 50f + Time.deltaTime * Vector3.Distance(transform.position, target.position) * 100f);
                    transform.Rotate(Vector3.up, Time.deltaTime * 180f, Space.Self);
                    return;
                }
                explosionLength = Mathf.MoveTowards(explosionLength, 0f, Time.deltaTime);
                if (explosionLength <= 1f)
                {
                    explosion.transform.localScale = new Vector3(explosionLength * _explosionWidth, explosion.transform.localScale.y, explosionLength * _explosionWidth);
                    _explAud.pitch = explosionLength;
                    if (explosionLength <= 0f)
                    {
                        Destroy(gameObject);
                    }
                }
            }
            else
            {
                if (noTracking && hadParent && !parentOverseer && (!otherParent || !otherParent.gameObject.activeSelf))
                {
                    Destroy(gameObject);
                }
                transform.Rotate(Vector3.up, Time.deltaTime * 720f, Space.Self);
                _activationTime = Mathf.MoveTowards(_activationTime, 1f, Time.deltaTime * windUpSpeedMultiplier);
                
                for (int i = 1; i < _sprites.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        _sprites[i].transform.localPosition = Vector3.up * 3f * Mathf.Lerp(0f, (float)(i / 2 * -1), _currentDistance);
                    }
                    else
                    {
                        _sprites[i].transform.localPosition = Vector3.up * 3f * Mathf.Lerp(0f, (float)((i + 1) / 2), _currentDistance);
                    }
                }
                if (_activationTime >= 1f)
                {
                    _activating = false;
                    explosionLength += 1f;
                    Explode();
                }
            }
        }

        private void Activating()
        {
            _idle = false;
            if (!noTracking && charges > 1)
            {
                charges--;
                Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<OverseerInsignia>().target = target;
            }
            _activating = true;
            if (predictive)
            {
                if (target != null)
                {
                    float num = 1f / windUpSpeedMultiplier;
                    Vector3 velocity = target.GetVelocity();
                    if (target.isPlayer)
                    {
                        if (NewMovement.Instance.ridingRocket)
                        {
                            transform.position = target.PredictTargetPosition(num);
                        }
                        else
                        {
                            transform.position = target.position + Vector3.up * _offset + new Vector3(velocity.x, 0f, velocity.z).normalized * 16.45f * (num - (transform.localScale.x - 1f) / 20f * num);
                        }
                    }
                    else
                    {
                        transform.position = target.position + Vector3.up * _offset + new Vector3(velocity.x, 0f, velocity.z) * (num - (transform.localScale.x - 1f) / 20f * num);
                    }
                }
                else
                {
                    transform.position = target.position + Vector3.up * _offset;
                }
            }
            for (int i = 1; i < _sprites.Length; i++)
            {
                _sprites[i].gameObject.SetActive(true);
                _sprites[i].transform.localPosition = Vector3.zero;
            }
            _currentDistance = 0f;
            _activationTime = 0f;
        }

        private void Explode()
        {
            if (noTracking && charges > 1 && (!hadParent || parentOverseer || (otherParent && otherParent.gameObject.activeSelf)))
            {
                charges--;
                OverseerInsignia component = Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<OverseerInsignia>();
                component.target = target;
                component.hadParent = hadParent;
                if (parentOverseer)
                {
                    component.parentOverseer = parentOverseer;
                }
                if (otherParent)
                {
                    component.otherParent = otherParent;
                }
            }
            foreach (var t in _sprites)
            {
                t.gameObject.SetActive(false);
            }

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit ray, 100,
                    LayerMaskDefaults.Get(LMD.Environment)))
            {
                //Instantiate(burn, ray.point, Quaternion.identity);
            }

            _explAud = explosion.GetComponent<AudioSource>();
            explosion.SetActive(true);
            CameraController.Instance.CameraShake(1f);
            if (_lit)
            {
                _lit.enabled = false;
            }
            if (parentOverseer)
            {
                //parentOverseer.childVi.Remove(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (target != null && !_hasHitTarget && (!target.isPlayer || other.gameObject.CompareTag("Player")))
            {
                if (target.isPlayer)
                {
                    _hasHitTarget = true;
                    if (PlayerTracker.Instance.playerType == PlayerType.Platformer)
                    {
                        PlatformerMovement.Instance.Burn();
                    }
                    else
                    {
                        NewMovement.Instance.LaunchFromPoint(NewMovement.Instance.transform.position, 100f, 5f);
                        NewMovement.Instance.GetHurt(damage, true);
                    }
                }
                /*else
                {
                    EnemyIdentifier enemyIdentifier = other.GetComponent<EnemyIdentifier>();
                    if (enemyIdentifier == null)
                    {
                        EnemyIdentifierIdentifier eidid = other.GetComponent<EnemyIdentifierIdentifier>();
                        if (eidid != null)
                        {
                            enemyIdentifier = eidid.eid;
                        }
                    }

                    if (enemyIdentifier != null && target.enemyIdentifier == enemyIdentifier && other.TryGetComponent(out Rigidbody rigidbody))
                    {
                        _hasHitTarget = true;
                        rigidbody.AddExplosionForce(1000f, transform.position, 10f);
                        enemyIdentifier.SimpleDamage((float)damage);
                    }
                }*/
            }
            if (other.TryGetComponent(out Flammable flammable) && !flammable.playerOnly)
            {
                flammable.Burn(10f);
            }
        }

        public bool triggerTogether;
        private bool _idle = true;


        public EnemyTarget target;

        public GameObject burn;
        public GameObject explosion;


        public int damage;

        private bool _hasHitTarget;

        private float _offset;

        private SpriteRenderer[] _sprites;

        private bool _activating;

        private float _activationTime;

        private float _currentDistance;

        public float windUpSpeedMultiplier = 1f;

        public float explosionLength;

        public int charges;

        private float _explosionWidth;

        private AudioSource _explAud;

        private Light _lit;

        [HideInInspector]
        public Overseer parentOverseer;

        [HideInInspector]
        public Transform otherParent;

        public bool hadParent;

        public bool predictive;

        public bool noTracking;

        public Sprite predictiveVersion;
    }

}
