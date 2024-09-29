using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeartOfTheMachineScripts.Workers;
using Logic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem.PlaybackState;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace HeartOfTheMachineScripts
{
    public class Worker : MonoBehaviour, IEnemy, IAlter, IAlterOptions<string>
    {

        public float health;

        public float GetHealth()
        {
            return health;
        }

        public bool IsAlive()
        {
            return !_dead && health > 0f;
        }

        public float GetLocationCritDamageMultiplier(string location)
        {
            return 1;
        }

        public EnemyIdentifier GetEnemyIdentifier()
        {
            return _eid;
        }

        private bool _dead;

        private void Awake()
        {
            if (!_animator) 
                _animator = GetComponent<Animator>();

            _eid = GetComponent <EnemyIdentifier>();
            _aud = GetComponent <AudioSource>();
            _nma = GetComponent<NavMeshAgent>();

            _originalPitch = _aud.pitch;

            _toDisable = GetComponentsInChildren<DisableOnWorkerAnim>();
        }

        private void Start()
        {
            foreach (var disableOnWorkerAnim in _toDisable)
            {
                disableOnWorkerAnim.Trigger(animation);
            }

            _block = new MaterialPropertyBlock();

            mainRenderer.GetPropertyBlock(_block, 0);
            _gz = GoreZone.ResolveGoreZone(transform);
            _animator.Play(animation);


            _animator.SetLayerWeight(1, moving ? 1 : 0);
            _nma.enabled = moving;
            _nma.speed *= speed;

            unlockSetter?.SetVar();

            foreach (CustomEnemyInfoPage enemyInfoPage in ListComponent<CustomEnemyInfoPage>.InstanceList)
            {
                enemyInfoPage.UpdateInfo();
                enemyInfoPage.DisplayInfo();
            }
        }

        private void Update()
        {
            if (_dead)
                return;

            if (moving)
            {
                if (Vector3.Distance(transform.position, points[_currentPoint].position) < 1)
                {
                    _currentPoint++;
                    if (_currentPoint >= points.Length)
                    {
                        _currentPoint = 0;
                    } 
                }
                _nma.SetDestination(points[_currentPoint].position);
            }

            if (dancing)
            {
                _elapse += Time.deltaTime;
                
                if (_elapse > 1)
                {
                    _elapse = 0;
                }
                
                _block.SetColor(UKShaderProperties.EmissiveColor, gay.Evaluate(_elapse));
                _block.SetFloat(UKShaderProperties.EmissiveIntensity, 1);
                mainRenderer.SetPropertyBlock(_block, 0);
            }

        }


        public void Instakill()
        {
            Die();
        }

        public void GetHurt(HurtEventData hurtEventData)
        {
            GetHurt(hurtEventData.target, hurtEventData.force, hurtEventData.multiplier, hurtEventData.critMultiplier, hurtEventData.hitter,
                hurtEventData.sourceWeapon, hurtEventData.fromExplosion);

        }

        public void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, string hitter, GameObject sourceWeapon = null, bool fromExplosion = false)
        {

            Debug.Log($"got hit for {multiplier} by {hitter}");


            if (!_eid.blessed && !InvincibleEnemies.Enabled)
            {
                health -= 1f * multiplier;
            }
            else
            {
                multiplier = 0f;
            }

            if (multiplier <= 0)
                return;

            health = (float)Math.Round(health, 4);
            if (health <= 0.001)
            {
                health = 0f;
            }

            if (!target.CompareTag("Head") && BloodsplatterManager.Instance.goreOn)
            {
                GameObject sparks = GetGoreType(target.gameObject.tag);
                sparks.transform.position = target.transform.position;


                if (IsAlive())
                {
                    if (_aud && hurtSounds.Length > 0)
                    {
                        _aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
                        _aud.pitch = _originalPitch + Random.Range(-0.1f, 0.1f);
                        _aud.Play();
                    }
                }
            }

            if (health <= 0f)
            {
                if (!_dead)
                {
                    Die();
                    return;
                }
            }

            if (!IsAlive() && target.TryGetComponent(out WorkerBodyPart bodyPart))
            {
                if (!BloodsplatterManager.Instance.goreOn)
                    return;

                float goreAmount = 1f;
                if (hitter is "shotgun" or "shotgunzone" or "explosion")
                {
                    goreAmount = 0.5f;
                }

                if (hat && target == hat.gameObject)
                {
                    hat.AddForce(force * 2);
                }

                //string bodyPart = target.gameObject.tag;

                bodyPart.Damage(multiplier);

                /*
                switch (bodyPart)
                {
                    case "Limb":
                    case "EndLimb":

                        SpawnGore(target, "Bolt", Random.Range(0, 3) * goreAmount);
                        SpawnGore(target, "Nut", Random.Range(0, 3) * goreAmount);
                        SpawnGore(target, "Gear", Random.Range(0, 2) * goreAmount);

                        break;

                    case "Body":
                        SpawnGore(target, "Bolt", Random.Range(4, 6) * goreAmount);
                        SpawnGore(target, "Nut", Random.Range(4, 6) * goreAmount);
                        SpawnGore(target, "Gear", Random.Range(4, 6) * goreAmount);

                        GameObject blood = GetGoreType("Head");
                        blood.transform.position = target.transform.position;
                        blood.SetActive(true);
                        blood.transform.SetParent(_gz.goreZone, true);

                        break;
                }*/

                /*if (target.transform.childCount > 0)
                {
                    Transform child = target.transform.GetChild(0);
                    CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
                    if (componentsInChildren.Length != 0)
                    {
                        foreach (CharacterJoint childJoint in componentsInChildren)
                        {
                            if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && childJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                            {
                                enemyIdentifierIdentifier.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
                            }

                            Destroy(childJoint);
                        }
                    }

                    if (target.TryGetComponent(out CharacterJoint joint))
                    {
                        joint.connectedBody = null;
                        Destroy(joint);
                    }

                    target.transform.position = child.position;
                    target.transform.SetParent(child);
                    child.SetParent(_gz.gibZone);
                    Destroy(target.GetComponent<Rigidbody>());
                }*/

                // if (target.TryGetComponent<Collider>(out var obj))
                // {
                //     Destroy(obj);
                // }
                //
                // target.transform.localScale = Vector3.zero;
            }
            else if (!IsAlive() && hat)
            {
                hat.AddForce(force, ForceMode.Impulse);
            }
        }

        public void Die()
        {
            if (_dead) 
                return;

            if (_aud && deathSounds.Length > 0)
            {
                _aud.clip = deathSounds[Random.Range(0, deathSounds.Length)];
                _aud.pitch = _originalPitch + Random.Range(-0.1f, 0.1f);
                _aud.Play();
            }

            if (hat)
            {

                hat.transform.SetParent(_gz.goreZone, true);

                hat.isKinematic = false;
                hat.useGravity = true;
                if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && hat.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                {
                    enemyIdentifierIdentifier.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
                }
                if (ComponentsDatabase.Instance && ComponentsDatabase.Instance.scrollers.Count > 0)
                {
                    CheckForScroller checkForScroller = hat.gameObject.AddComponent<CheckForScroller>();
                    checkForScroller.checkOnStart = false;
                    checkForScroller.checkOnCollision = true;
                    checkForScroller.asRigidbody = true;
                }

                hat.AddForce((hat.position - _eid.overrideCenter.transform.position).normalized * Random.Range(20f, 30f), ForceMode.VelocityChange);
                hat.AddTorque(Random.onUnitSphere * 360f, ForceMode.VelocityChange);
                
            }

            WorkerGoLimp();
            //_animator?.SetTrigger("Die");
            _dead = true;
        }

        public void WorkerGoLimp()
        {
            if (_limp)
            {
                return;
            }
            if (mainRenderer)
            {
                ((SkinnedMeshRenderer)mainRenderer).updateWhenOffscreen = true;

                _block.SetFloat(UKShaderProperties.EmissiveIntensity, 0);
                mainRenderer.SetPropertyBlock(_block, 0);
            }

            if (_animator)
            {
                _animator.StopPlayback();
                Destroy(_animator);
            }

            health = 0f;
            killCount?.SetVar();

            foreach (var simplifier in GetComponentsInChildren<EnemySimplifier>())
            {
                simplifier.Begone();
            }
            if (gameObject.TryGetComponent(out Collider c))
                Destroy(c);

            if (idleAudio)
                Destroy(idleAudio.gameObject);



            /*if (_deathSound != null)
            {
                _aud.clip = _deathSound;
                if (_eid.hitter != "fire")
                {
                    _aud.volume = _deathSoundVol;
                }
                else
                {
                    _aud.volume = 0.5f;
                }
                _aud.pitch = Random.Range(0.85f, 1.35f);
                _aud.priority = 11;
                _aud.Play();
            }*/

            if (!_limp)
            {
                foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
                {
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                    if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                    {
                        enemyIdentifierIdentifier.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
                    }
                    if (ComponentsDatabase.Instance && ComponentsDatabase.Instance.scrollers.Count > 0)
                    {
                        CheckForScroller checkForScroller = rigidbody.gameObject.AddComponent<CheckForScroller>();
                        checkForScroller.checkOnStart = false;
                        checkForScroller.checkOnCollision = true;
                        checkForScroller.asRigidbody = true;
                    }
                    if (hat && rigidbody == hat)
                    {
                        
                        rigidbody.AddForce((hat.position - _eid.overrideCenter.transform.position).normalized * Random.Range(20f, 30f), ForceMode.VelocityChange);
                        rigidbody.AddTorque(Random.onUnitSphere * 360f, ForceMode.VelocityChange);
                        rigidbody.transform.SetParent(_gz.goreZone, true);
                    }
                }
            }
            if (!_limp)
            {
                if (_gz && _gz.transform && _gz.gibZone)
                {
                    transform.SetParent(_gz.transform, true);
                }

                

                //hat.AddForce(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) * 10, ForceMode.Impulse);
            }

            _limp = true;
        }

        public GameObject GetGoreType(string tag)
        {
            GameObject gib;
            switch (tag)
            {
                case "Head":
                    gib = head;
                    break;

                case "Body":
                    gib = body;
                    break;

                default:
                    gib = limb;
                    break;
            }

            return Instantiate(gib, _gz.goreZone);
        }

        
        public void Knockback(Vector3 force)
        {
            //they cant take knockback....
        }

        public GameObject GetGib(WorkerGibType type)
        {
            GameObject gib = null;
            switch (type)
            {
                case WorkerGibType.Bolt:
                    gib = bolt;
                    break;

                case WorkerGibType.Gear:
                    gib = gear;
                    break;

                case WorkerGibType.Nut:
                    gib = nut;
                    break;

                case WorkerGibType.CircuitBoard:
                    gib = circuitBoard;
                    break;
            }

            return Instantiate(gib, _gz.goreZone);
        }

        [Header("Components")] 
        public Rigidbody hat;
        public Renderer mainRenderer;
        public MapBoolSetter unlockSetter;
        public MapIntSetter killCount;

        private AudioSource _aud;
        private EnemyIdentifier _eid;
        private GoreZone _gz;
        private NavMeshAgent _nma;

        [Header("Audio")]
        public AudioSource idleAudio;

        public AudioClip[] hurtSounds;
        public AudioClip[] idleSounds;
        public AudioClip[] deathSounds;
        private float _originalPitch;

        [Header("Gibs")] 
        public GameObject gear;
        public GameObject nut;
        public GameObject bolt;
        public GameObject circuitBoard;

        [Header("Sparks")]
        public GameObject head;
        public GameObject body;
        public GameObject limb;

        [Header("Movement")]
        public bool moving;
        public Transform[] points;
        public float speed;
        private int _currentPoint;
        
        [Header("Animation Stuff")]

        [HideInInspector]
        public string animation;

        [HideInInspector] //this is needed for the editor script
        public int animationIndex;

        [SerializeField]
        private Animator _animator;
        private DisableOnWorkerAnim[] _toDisable;

        private bool _limp;


        [Header("Fun")]

        public Gradient gay;
        public bool dancing = false;
        private MaterialPropertyBlock _block;
        private float _elapse;




        public string alterKey { get; }
        public string alterCategoryName { get; }
        public AlterOption<string>[] options { get; }

    }

}
