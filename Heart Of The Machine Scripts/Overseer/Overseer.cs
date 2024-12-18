using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

namespace HeartOfTheMachineScripts
{
    public class Overseer : MonoBehaviour, IEnrage, IEnemy, IAlter, IAlterOptions<bool>, IAlterOptions<float>
    {
        private EnemyTarget Target => Components.EnemyIdentifier.target;
        public bool Dead => Components.EnemyIdentifier.dead;
        public float GetHealth()
        {
            return health;
        }

        public EnemyIdentifier GetEnemyIdentifier()
        {
            return Components.EnemyIdentifier;
        }

        public bool IsAlive() => !Components.EnemyIdentifier.dead;


        public OverseerComponents Components
        {
            get
            {
                if (_components == null)
                    _components = GetComponent<OverseerComponents>();
                return _components;
            }
        }

        private OverseerComponents _components;

        public float GetLocationCritDamageMultiplier(string location)
        {
            return 0f;
        }


        public float health;

        private void Awake()
        {
            Patcher.QueryInstance();

            
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            _block = new MaterialPropertyBlock();

            _oc = OverseerCooldowns.Instance;

            if (!chargeParticle)
            {
                chargeParticle = GetComponentInChildren<ParticleSystem>();
            }
        }

        private void Start()
        {
            _bsm = BloodsplatterManager.Instance;


            //components.Animator = GetComponent<Animator>();

            _dodgeCooldown = Random.Range(0.5f, 3f);
            _attackCooldown = 1.5f;

            if (!dontStartAware)
            {
                targetSpotted = true;
            }


            SlowUpdate();
            if (!musicRequested && !Components.EnemyIdentifier.dead)
            {
                musicRequested = true;
                MusicManager.Instance.PlayBattleMusic();
            }

            _gz = GoreZone.ResolveGoreZone(transform);
            if (enraged)
            {
                Enrage();
            }
            _difficulty = Components.EnemyIdentifier.difficultyOverride >= 0 ? Components.EnemyIdentifier.difficultyOverride : PrefsManager.Instance.GetInt("difficulty");


            switch (_difficulty)
            {
                default: _knockbacksBeforeEnrage = -1; break;
                case 3: _knockbacksBeforeEnrage = 3; break;
                case 4: _knockbacksBeforeEnrage = 2; break;
                case 5: _knockbacksBeforeEnrage = 1; break;
            }

            _startingHealth = health;
            if (_difficulty < 2)
            {
                knockbackIndicator.gameObject.SetActive(false);
            }

            _knockbackCooldown = 1.5f;
        }

        private void UpdateBuff()
        {
            if (Components.Animator)
            {
                Components.Animator.speed = Components.EnemyIdentifier.totalSpeedModifier;
            }
        }

        private void OnDisable()
        {
            if (musicRequested)
            {
                musicRequested = false;
                var instance = MusicManager.Instance;
                if (instance)
                {
                    instance.PlayCleanMusic();
                }
            }

            if (_oc)
            {
                _oc.RemoveOverseer(this);
            }
        }

        private void OnEnable()
        {
            if (!musicRequested && !Components.EnemyIdentifier.dead)
            {
                musicRequested = true;
                MusicManager.Instance.PlayBattleMusic();
            }

            if (_oc)
            {
                _oc.AddOverseer(this);
            }
        }

        private void UpdateRigidbodySettings()
        {
            if (!Components.Rigidbody)
                return;

            if (Target == null && !crashing)
            {
                Components.Rigidbody.drag = 3f;
                Components.Rigidbody.angularDrag = 3f;
                return;
            }

            Components.Rigidbody.drag = 0f;
            Components.Rigidbody.angularDrag = 0f;
        }

        private void Update()
        {
            if (!crashing)
            {
                UpdateRigidbodySettings();
                if (Target == null)
                {
                    return;
                }

                if (targetSpotted)
                {
                    _viewTarget = Target.position;
                    var num = (_difficulty / 2f);
                    if (num == 0f)
                    {
                        num = 0.25f;
                    }

                    num *= Components.EnemyIdentifier.totalSpeedModifier;
                    if (_dodgeCooldown > 0f)
                    {
                        _dodgeCooldown = Mathf.MoveTowards(_dodgeCooldown, 0f, Time.deltaTime * num);
                    }
                    else if (!stationary && !lockPosition)
                    {
                        _dodgeCooldown = Random.Range(1f, 3f);
                        RandomDodge();
                    }
                }
                if (_knockbackCooldown > 0f)
                {
                    _knockbackCooldown = Mathf.MoveTowards(_knockbackCooldown, 0f, Time.deltaTime * Components.EnemyIdentifier.totalSpeedModifier);

                    var i = Mathf.Lerp(eyeIntensityMin, eyeIntensityMax, 1 - _knockbackCooldown / knockbackCooldown);
                    //Debug.Log(i);

                    eyeRenderer.GetPropertyBlock(_block);
                    _block.SetFloat(UKShaderProperties.EmissiveIntensity, i);
                    eyeRenderer.SetPropertyBlock(_block);
                    if (_knockbackCooldown <= 0f)
                    {
                        eyeRenderer.GetPropertyBlock(_block);
                        _block.SetFloat(UKShaderProperties.EmissiveIntensity, eyeIntensityMax);
                        eyeRenderer.SetPropertyBlock(_block);
                        Instantiate(Components.KnockbackReady, transform.position, Quaternion.identity);
                        knockbackIndicator.Play();
                    }
                }

                if (_difficulty >= 2 && _knockbackCooldown <= 0f && (Vector3.Distance(transform.position, Components.EnemyIdentifier.target.position) < 10f || Vector3.Distance(transform.position, Components.EnemyIdentifier.target.position) < knockbackDistance))
                {
                    DoKnockback();
                }


                if ((!Target.isPlayer || !NewMovement.Instance.levelOver) &&  (Vector3.Distance(transform.position, Target.position) < 150f || stationary))
                {
                    float difficultyCooldown = _difficulty / 2f;
                    switch (_difficulty)
                    {
                        case >= 4:
                            difficultyCooldown = 1.2f;
                            break;
                        case 1:
                            difficultyCooldown = 0.75f;
                            break;
                        case 0:
                            difficultyCooldown = 0.5f;
                            break;
                    }

                    difficultyCooldown *= Components.EnemyIdentifier.totalSpeedModifier;
                    if (_attackCooldown > 0f)
                    {
                        _attackCooldown = Mathf.MoveTowards(_attackCooldown, 0f, Time.deltaTime * difficultyCooldown);
                    }
                    else if (Components.Insignia != null && (!_oc || _oc.overseerCooldown == 0f))
                    {
                        if (_oc)
                        {
                            _oc.overseerCooldown = 1f / Components.EnemyIdentifier.totalSpeedModifier;
                        }

                        _parryable = true;

                        chargeParticle?.Play();
                        PlaySound(Components.WindUpSound, Mathf.Lerp(0.8f, 1.2f, _oc.overseerUsedBeams > 0 ? ((float)_oc.overseerUsedBeams) / _oc.GetMaxAttackAmount(_difficulty) : 0));
                        _attackCooldown = Mathf.Max(1f, (3f - _oc.overseerUsedBeams) / (enraged ? 2 : 1));
                        Components.Animator?.SetTrigger("Attack");
                        Components.PassiveAudioSource.pitch = 0.75f;
                    }

                    if (_parryFramesLeft > 0)
                    {
                        Components.EnemyIdentifier.hitter = "punch";
                        Components.EnemyIdentifier.DeliverDamage(gameObject, CameraController.Instance.transform.forward * 25000f, transform.position, 5f, false);
                        _parryFramesLeft = 0;
                    }
                }
            }

            if (Components.EnemyIdentifier && Components.EnemyIdentifier.hooked && !_hooked)
            {
                Hooked();
                return;
            }

            if (Components.EnemyIdentifier && !Components.EnemyIdentifier.hooked && _hooked)
            {
                Unhooked();
            }
        }

        private void SlowUpdate()
        {

            if (Dead)
                return;

            if (!crashing && Target != null)
            {
                if (targetSpotted)
                {
                    if (Physics.Raycast(transform.position, Target.position - transform.position,
                            Vector3.Distance(transform.position, Target.position) - 1f,
                            RelevantSightBlockMask))
                    {
                        targetSpotted = false;
                        //PlaySound(loseSound);
                        _lastKnownPos = Target.position;
                        blockCooldown = 0f;
                        checkCooldown = 0f;
                        toLastKnownPos = true;
                    }
                }
                else if (!Physics.Raycast(transform.position, Target.position - transform.position,
                             Vector3.Distance(transform.position, Target.position) - 1f,
                             RelevantSightBlockMask))
                {
                    //PlaySound(spotSound);
                    targetSpotted = true;
                }
            }

            Invoke("SlowUpdate", 0.25f);
        }

        /*private void LateUpdate()
        {

            if (Dead)
                return;

            if (Components.EnemyIdentifier.target is not { isValid: true })// || BlindEnemies.Blind)
            {
                return;
            }
            float speed = eyeSpeed * Components.EnemyIdentifier.totalSpeedModifier;

            
            foreach (var eye in _eyes)
            {
                Vector3 position = Components.EnemyIdentifier.target.position;

                Quaternion quaternion = Quaternion.LookRotation(position - eye.position, Vector3.down);
                if (Quaternion.Angle(Components.EnemyIdentifier.target.targetTransform.rotation, quaternion) > 90)
                {
                    //Debug.LogWarning("more than 90");
                    //return;
                }
                if (eyeSpeed == 0f)
                {
                    eye.rotation = quaternion;
                }
                eye.rotation = Quaternion.RotateTowards(eye.rotation, quaternion, Time.deltaTime * speed);
            }
        }*/

        private void FixedUpdate()
        {

            if (!Components.Rigidbody)
                return;

            if (_parryFramesLeft > 0)
            {
                _parryFramesLeft--;
            }

            if (Components.Rigidbody.velocity.magnitude < 1f && Components.Rigidbody.collisionDetectionMode != CollisionDetectionMode.Discrete)
            {
                Components.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }

            if (crashing)
            {
                if (_parried)
                {
                    Components.Rigidbody.useGravity = false;
                    Components.Rigidbody.velocity = transform.forward * 120f * Components.EnemyIdentifier.totalSpeedModifier;
                }
            }
            else if (targetSpotted && Target != null)
            {
                Components.Rigidbody.velocity *= 0.975f;
                if (!stationary && Vector3.Distance(transform.position, Target.position) > 15f)
                {
                    Components.Rigidbody.AddForce(transform.forward * 10f * Components.EnemyIdentifier.totalSpeedModifier,
                        ForceMode.Acceleration);
                }
            }
            else if (toLastKnownPos && !stationary && !lockPosition && Target != null)
            {
                if (blockCooldown == 0f)
                {
                    _viewTarget = _lastKnownPos;
                }
                else
                {
                    blockCooldown = Mathf.MoveTowards(blockCooldown, 0f, 0.01f);
                }

                Components.Rigidbody.AddForce(transform.forward * 10f * Components.EnemyIdentifier.totalSpeedModifier, ForceMode.Acceleration);
                if (checkCooldown == 0f && Vector3.Distance(transform.position, _lastKnownPos) > 5f)
                {
                    checkCooldown = 0.1f;
                    if (Physics.BoxCast(transform.position - (_viewTarget - transform.position).normalized, Vector3.one,
                            _viewTarget - transform.position, transform.rotation, 4f,LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
                    {
                        blockCooldown = Random.Range(1.5f, 3f);
                        var a = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                        _viewTarget = transform.position + a * 100f;
                    }
                }
                else if (Vector3.Distance(transform.position, _lastKnownPos) <= 3f)
                {
                    Physics.Raycast(transform.position, Random.onUnitSphere, out var raycastHit,
                        float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies));
                    _lastKnownPos = raycastHit.point;
                }

                if (checkCooldown != 0f)
                {
                    checkCooldown = Mathf.MoveTowards(checkCooldown, 0f, 0.01f);
                }
            }

            if (crashing) return;

            if (!lockRotation && Target != null)
            {
                var b = Quaternion.LookRotation(_viewTarget - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, b,
                    0.075f + 0.00025f * Quaternion.Angle(transform.rotation, b) * Components.EnemyIdentifier.totalSpeedModifier);
            }

            Components.Rigidbody.velocity = Vector3.ClampMagnitude(Components.Rigidbody.velocity, 50f * Components.EnemyIdentifier.totalSpeedModifier);
            if (Components.KeepInBounds)
            {
                Components.KeepInBounds.ValidateMove();
            }
        }

        public void DoKnockback()
        {
            knockbackIndicator.Stop();
            _knockbackCooldown = knockbackCooldown;

            eyeRenderer.GetPropertyBlock(_block);
            _block.SetFloat(UKShaderProperties.EmissiveIntensity, eyeIntensityMin);
            eyeRenderer.SetPropertyBlock(_block);

            if (Components.EnemyIdentifier.hooked)
            {
                HookArm.Instance.StopThrow(1f, true);
            }

            var explosionObj = Instantiate(Components.KnockbackExplosion, transform.position, Quaternion.identity);
            explosionObj.transform.SetParent(_gz.transform, true);
            foreach (var explosion in explosionObj.GetComponentsInChildren<Explosion>())
            {
                if (Components.EnemyIdentifier.totalDamageModifier == 1f) continue;

                explosion.maxSize *= Components.EnemyIdentifier.totalDamageModifier;
                explosion.speed *= Components.EnemyIdentifier.totalDamageModifier;
            }

            if (!enraged && _difficulty > 2)
            {
                _knockbacksBeforeEnrage -= 1;

                if (_knockbacksBeforeEnrage <= 0)
                {
                    Enrage();
                }
            }

            Dodge(Components.EnemyIdentifier.target.position - transform.position, enraged ? -900 : -600);

            if (_difficulty >= 3)
            {
                Invoke("SpawnKnockbackBeam", 0.1f);
            }
        }

        private void SpawnKnockbackBeam()
        {


            var targetPos = Components.EnemyIdentifier.target.PredictTargetPosition(0.25f);

            if (Physics.Raycast(transform.position,
                    targetPos - transform.position,
                    out RaycastHit hitInfo,
                    Vector3.Distance(transform.position, targetPos) - 1f,
                    RelevantSightBlockMask))
            {
                targetPos = hitInfo.point;
            }


            var gameObject = Instantiate(Components.Insignia, targetPos, Quaternion.identity);
            var overseerInsignia = gameObject.GetComponent<OverseerInsignia>();
            overseerInsignia.target = Target;
            overseerInsignia.parentOverseer = this;
            overseerInsignia.hadParent = true;

            //overseerInsignia.noTracking = true;


            if (PlayerTracker.Instance.playerType == PlayerType.Platformer)
            {
                gameObject.transform.localScale *= 0.75f;
                overseerInsignia.windUpSpeedMultiplier *= 0.875f;
            }

            overseerInsignia.windUpSpeedMultiplier = 4f;
            //overseerInsignia.windUpSpeedMultiplier *= components.EnemyIdentifier.totalSpeedModifier;
            overseerInsignia.damage = Mathf.RoundToInt(overseerInsignia.damage * Components.EnemyIdentifier.totalDamageModifier);

        }

        public void RandomDodge()
        {
            if ((_difficulty == 1 && Random.Range(0f, 1f) > 0.75f) || _difficulty == 0)
            {
                return;
            }

            Dodge(transform.up * Random.Range(-5f, 5f) + transform.right * Random.Range(-5f, 5f));
        }

        public void Dodge(Vector3 direction, float speed = 150)
        {
            speed *= Components.EnemyIdentifier.totalSpeedModifier;
            Components.Rigidbody.AddForce(direction.normalized * speed, ForceMode.Impulse);
        }

        //public void GetHurt(Vector3 force, float multiplier, GameObject sourceWeapon = null, bool fromExplosion = false)
        public void Instakill()
        {
            GetHurt(gameObject, Vector3.zero, 999f, 1, "instakill", null, false);
            Explode();
        }



        public void ReadyGib(GameObject tempGib, GameObject target)
        {
            tempGib.transform.SetPositionAndRotation(target.transform.position, Random.rotation);
            _gz.SetGoreZone(tempGib);
            if (!OptionsMenuToManager.bloodEnabled)
            {
                tempGib.SetActive(false);
            }
        }

        private void SpawnGore(GameObject target, BSType type, float amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject gib = _bsm.GetGib(type);
                if (gib && _gz && _gz.gibZone)
                {
                    ReadyGib(gib, target);
                }
            }
        }

        public void GetHurt(HurtEventData hurtEventData)
        {
            //Debug.Log($"Hurt {hurtEventData}");
            GetHurt(hurtEventData.target, hurtEventData.force, hurtEventData.multiplier, hurtEventData.critMultiplier, hurtEventData.hitter,
                hurtEventData.sourceWeapon, hurtEventData.fromExplosion);

        }

        public void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, string hitter, GameObject sourceWeapon = null, bool fromExplosion = false)
        {
            // var multiplier = hurtEventData.multiplier;
            // var force = hurtEventData.force;
            // var fromExplosion = hurtEventData.fromExplosion;
            // var sourceWeapon = hurtEventData.sourceWeapon;
            //var hitter = hurtEventData.hitter;

            if ((Dead || crashing) && multiplier > 0)
            {

                if (BloodsplatterManager.Instance.goreOn)// && !target.gameObject.CompareTag("EndLimb"))
                {
                    float goreAmount = 1f;
                    if (Components.EnemyIdentifier.hitter is "shotgun" or "shotgunzone" or "explosion")
                    {
                        goreAmount = 0.5f;
                    }
                    string bodyPart = target.gameObject.tag;

                    switch (bodyPart)
                    {
                        case "Head":

                            //SpawnGore(target, BSType.eyeball, 500 * goreAmount);
                            SpawnGore(target, BSType.skullChunk, 6 * goreAmount);
                            SpawnGore(target, BSType.brainChunk, 4 * goreAmount);
                            SpawnGore(target, BSType.jawChunk, 2 * goreAmount);
                            break;

                        case "Limb":
                        case "EndLimb":

                            SpawnGore(target, BSType.gib, 4 * goreAmount);

                            if (target.transform.childCount > 0)
                            {
                                Transform child = target.transform.GetChild(0);
                                HingeJoint[] componentsInChildren = target.GetComponentsInChildren<HingeJoint>();
                                if (componentsInChildren.Length != 0)
                                {
                                    foreach (HingeJoint HingeJoint in componentsInChildren)
                                    {
                                        if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && HingeJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                                        {
                                            enemyIdentifierIdentifier.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
                                        }
                                        Destroy(HingeJoint);
                                    }
                                }
                                HingeJoint component = target.GetComponent<HingeJoint>();
                                if (component != null)
                                {
                                    component.connectedBody = null;
                                    Destroy(component);
                                }
                                target.transform.position = child.position;
                                target.transform.SetParent(child);
                                child.SetParent(_gz.gibZone);
                                Destroy(target.GetComponent<Rigidbody>());
                            }
                            break;
                        case "Body":
                            SpawnGore(target, BSType.skullChunk, 4 * goreAmount);
                            SpawnGore(target, BSType.brainChunk, 3 * goreAmount);
                            SpawnGore(target, BSType.jawChunk, 2 * goreAmount);

                            GameObject gore = _bsm.GetGore(GoreType.Head, Components.EnemyIdentifier, fromExplosion);
                            gore.transform.position = target.transform.position;
                            gore.transform.SetParent(_gz.goreZone, true);

                            break;
                    }
                    if (target.TryGetComponent<Collider>(out var obj))
                    {
                        Destroy(obj);
                    }
                    target.transform.localScale = Vector3.zero;
                }
            }

            if (multiplier > 0)
            {
                GameObject blood = _bsm.GetGore(GetGoreType(target.gameObject.tag), Components.EnemyIdentifier, fromExplosion);
                blood.transform.position = target.transform.position;
                blood.SetActive(true);
                blood.transform.SetParent(_gz.goreZone, true);


                if (hitter == "drill")
                    blood.transform.localScale *= 2f;

                Bloodsplatter bloodsplatter = blood.GetComponent<Bloodsplatter>();

                if (!crashing && !Dead)
                    bloodsplatter?.GetReady();

                if (!Components.EnemyIdentifier.blessed)
                {
                    Knockback(force);
                }

                if (multiplier >= 1f)
                {
                    if (bloodsplatter)
                    {
                        bloodsplatter.hpAmount = 30;
                    }

                    if (Components.Gib != null && !crashing && !Dead && target.gameObject.CompareTag("Body"))
                    {
                        for (int i = 0; i <= multiplier; i++)
                        {
                            Instantiate(Components.Gib, transform.position, Random.rotation).transform.SetParent(_gz.gibZone, true);
                        }
                    }
                }


                if (BloodsplatterManager.Instance.goreOn && blood && blood.TryGetComponent<ParticleSystem>(out var particleSystem))
                {
                    particleSystem.Play();
                }
            }

            var hasDied = false;

            if (!crashing)
            {
                if (hitter is "shotgunzone" or "hammerzone" && !_parryable && health - multiplier > 0f)
                {
                    return;
                }

                if (hitter is "shotgunzone" or "hammerzone" or "punch" && _parryable)
                {
                    if (_parryable)
                    {
                        if (!InvincibleEnemies.Enabled && !Components.EnemyIdentifier.blessed)
                        {
                            multiplier = _parryFramesLeft > 0 ? 3 : 4;
                        }

                        FistControl.Instance.currentPunch.Parry(false, Components.EnemyIdentifier);
                        _parryable = false;
                    }
                    else
                    {
                        _parryFramesLeft = FistControl.Instance.currentPunch.activeFrames;
                    }
                }

                if (!Components.EnemyIdentifier.blessed && !InvincibleEnemies.Enabled)
                {
                    health -= 1f * multiplier;
                }
                else
                {
                    multiplier = 0f;
                }

                health = (float)Math.Round(health, 4);
                if (health <= 0.001)
                {
                    health = 0f;
                }

                if (health <= 0f)
                {
                    hasDied = true;
                }

                if (_homeRunnable && !Components.EnemyIdentifier.puppet && hasDied && hitter is "punch" or "heavypunch" or "hammer")
                {
                    StyleHUD.Instance.AddPoints(100, "ultrakill.homerun", sourceWeapon, Components.EnemyIdentifier);
                    StyleCalculator.Instance.AddToMultiKill();
                }
                else if (hitter != "enemy" && !Components.EnemyIdentifier.puppet && multiplier != 0f)
                {
                    if (_scalc == null)
                        _scalc = StyleCalculator.Instance;

                    if (_scalc)
                        _scalc.HitCalculator(hitter, "drone", "", hasDied, Components.EnemyIdentifier, sourceWeapon);
                }

                if (health <= 0f && !crashing)
                {
                    _parryable = false;
                    Death(fromExplosion);
                    if (hitter != "punch" && hitter != "heavypunch" && hitter != "hammer")
                    {
                        if (Target != null)
                            _crashTarget = Target.position;
                    }
                    else
                    {
                        transform.position += force.normalized;
                        _crashTarget = transform.position + force;
                        if (Components.Rigidbody)
                            Components.Rigidbody.velocity = force.normalized * 40f;
                    }

                    transform.LookAt(_crashTarget);

                    PlaySound(Components.DeathSound);

                    Invoke("CanInterruptCrash", 0.5f);
                    Invoke("Explode", 5);

                    return;
                }


                PlaySound(Components.HurtSound);

                //if (hitter == "fire") return;

            }
            else if (hitter is "punch" or "hammer" && !_parried)
            {
                _parried = true;
                if (Components.Rigidbody)
                    Components.Rigidbody.velocity = Vector3.zero;

                transform.rotation = CameraController.Instance.transform.rotation;

                if (hitter == "punch")
                {
                    FistControl.Instance.currentPunch.GetComponent<Animator>().Play("Hook", -1, 0.065f);
                    FistControl.Instance.currentPunch.Parry(false, Components.EnemyIdentifier);
                }

                if (TryGetComponent<Collider>(out var collider))
                {
                    collider.isTrigger = true;
                }
            }
            else if (multiplier >= 1f || _canInterruptCrash)
            {
                //Debug.Log($"exploded from damage {hitter} {multiplier}");
                Explode();
            }
        }

        public GoreType GetGoreType(string tag)
        {
            switch (tag)
            {
                case "Head":
                    return GoreType.Head;

                case "Body":
                    return GoreType.Body;

                case "Limb":
                case "EndLimb":
                    return GoreType.Small;

                default:
                    return GoreType.Small;
            }
        }

        public void Knockback(Vector3 force)
        {

            if (!Components.Rigidbody)
                return;

            Components.Rigidbody.velocity /= 10f;
            Components.Rigidbody.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);
            Components.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (Components.Rigidbody.velocity.magnitude > 50f)
            {
                Components.Rigidbody.velocity = Vector3.ClampMagnitude(Components.Rigidbody.velocity, 50f);
            }
        }


        public void PlaySound(AudioClip clip, float pitch = 1)
        {
            if (!clip)
            {
                return;
            }
            Components.AudioSource.Stop();


            Components.AudioSource.clip = clip;
            Components.AudioSource.priority = 12;
            Components.AudioSource.pitch = pitch;
            Components.AudioSource.Play();
        }

        public void Explode()
        {
            if (!_exploded && gameObject.activeInHierarchy && (!cantInstaExplode || _canInterruptCrash))
            {
                _exploded = true;

                foreach (var skinnedMeshRenderer in _renderers)
                {
                    skinnedMeshRenderer.updateWhenOffscreen = true;
                }

                foreach (var t in GetComponentsInChildren<EnemySimplifier>())
                {
                    t.Begone();
                }



                foreach (Rigidbody rigidbody in Components.RigidbodiesToGibOnCrash)
                {
                    if (rigidbody != null)
                    {
                        rigidbody.isKinematic = false;
                        rigidbody.useGravity = true;
                        if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                        {
                            enemyIdentifierIdentifier.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
                        }
                        rigidbody.transform.SetParent(_gz.goreZone, true);
                    }
                }

                if (enraged)
                    UnEnrage();

                var shatterEffect = Instantiate(Components.ShatterEffect, transform.position, Quaternion.identity);

                shatterEffect.transform.SetParent(_gz.transform, true);
                foreach (var explosion in shatterEffect.GetComponentsInChildren<Explosion>())
                {
                    if (Components.EnemyIdentifier.totalDamageModifier != 1f)
                    {
                        explosion.damage = Mathf.RoundToInt(explosion.damage * Components.EnemyIdentifier.totalDamageModifier);
                        explosion.maxSize *= Components.EnemyIdentifier.totalDamageModifier;
                        explosion.speed *= Components.EnemyIdentifier.totalDamageModifier;
                    }

                    if (_killedByPlayer)
                    {
                        explosion.friendlyFire = true;
                    }
                }

                var componentInChildren = GetComponentInChildren<DoubleRender>();
                if (componentInChildren)
                {
                    componentInChildren.RemoveEffect();
                }

                if (!crashing)
                {
                    Death(true);
                }
                else if (Components.EnemyIdentifier.drillers.Count > 0)
                {
                    for (var j = Components.EnemyIdentifier.drillers.Count - 1; j >= 0; j--)
                    {
                        Destroy(Components.EnemyIdentifier.drillers[j].gameObject);
                    }
                }

                foreach (var toDestroy in destroyOnCrash)
                {
                    if (toDestroy.activeInHierarchy)
                    {
                        Transform transform = toDestroy.GetComponentInParent<Rigidbody>().transform;

                        if (!transform) continue;

                        toDestroy.transform.SetParent(transform);
                        toDestroy.transform.position = transform.position;
                        toDestroy.transform.localScale = Vector3.zero;
                    }
                    //Destroy(toDestroy);
                }
                Destroy(Components.Rigidbody);




                if (musicRequested)
                    MusicManager.Instance?.PlayCleanMusic();
            }
        }



        public void Death(bool fromExplosion = false)
        {
            if (crashing)
            {
                return;
            }

            knockbackIndicator?.gameObject.SetActive(false);

            Components.UnlockSetter?.SetVar();
            Components.KillCount?.SetVar();

            foreach (CustomEnemyInfoPage enemyInfoPage in ListComponent<CustomEnemyInfoPage>.InstanceList)
            {
                enemyInfoPage.UpdateInfo();
                enemyInfoPage.DisplayInfo();
            }

            Components.EnemyIdentifier.dead = true;

            Debug.Log(Components.EnemyIdentifier.dead);


            if (Components.Animator)
                Destroy(Components.Animator);

            if (Components.PassiveAudioSource)
                Destroy(Components.PassiveAudioSource.gameObject);

            //eididsToGibOnDeath
            //var rbs = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigidbody in Components.RigidbodiesToGibOnDeath)
            {
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                    if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                    {
                        enemyIdentifierIdentifier.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
                    }
                    rigidbody.transform.SetParent(_gz.goreZone, true);
                }
            }

            crashing = true;
            UpdateRigidbodySettings();
            if (Components.Rigidbody && Components.Rigidbody.isKinematic)
            {
                Components.Rigidbody.isKinematic = false;
            }

            Components.Rigidbody.velocity = Vector3.zero;
            Components.Rigidbody.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
            Components.Rigidbody.useGravity = true;

            if (_oc)
            {
                _oc.RemoveOverseer(this);
            }


            if (_oc.insignias.Count > 0 && _oc.currentOverseers.Count <= 0)
            {
                for (var i = 0; i < _oc.insignias.Count; i++)
                {
                    if (_oc.insignias[i] != null && _oc.insignias[i].gameObject)
                    {
                        _oc.insignias[i].explosionLength = 0.5f;
                        _oc.insignias[i].Invoke("Activating", i * 0.5f / _oc.insignias[i].windUpSpeedMultiplier);

                    }
                }
                _oc.insignias.Clear();
                _oc.overseerUsedBeams = 0;
            }

            if (Components.EnemyIdentifier.hitter != "enemy")
            {
                _killedByPlayer = true;
            }

            if (!Components.EnemyIdentifier.dontCountAsKills)
            {
                if (_gz != null && _gz.checkpoint != null)
                {
                    _gz.AddDeath();
                    _gz.checkpoint.sm.kills++;
                }
                else
                {
                    StatsManager.Instance.kills++;
                }
            }

            if (Components.EnemyIdentifier.hitter != "fire")
            {
                var gore = _bsm.GetGore(GoreType.Head, Components.EnemyIdentifier, fromExplosion);
                if (gore)
                {
                    gore.transform.position = transform.position;
                    if (BloodsplatterManager.Instance.goreOn && gore.TryGetComponent<ParticleSystem>(out var particleSystem))
                    {
                        particleSystem.Play();
                    }

                    gore.transform.SetParent(_gz.goreZone, true);
                    if (Components.EnemyIdentifier.hitter == "drill")
                    {
                        gore.transform.localScale *= 2f;
                    }

                    if (gore.TryGetComponent<Bloodsplatter>(out var bloodsplatter))
                    {
                        bloodsplatter.GetReady();
                    }
                }
            }

            if (!Components.EnemyIdentifier.dontCountAsKills)
            {
                var componentInParent = GetComponentInParent<ActivateNextWave>();
                if (componentInParent != null)
                {
                    componentInParent.AddDeadEnemy();
                }
            }
        }
        public void SpawnInsignia()
        {
            if (!crashing && Target != null)
            {
                _parryable = false;
                var insigniaObj = Instantiate(Components.Insignia, Target.position, Quaternion.identity);
                var overseerInsignia = insigniaObj.GetComponent<OverseerInsignia>();
                overseerInsignia.target = Target;
                overseerInsignia.parentOverseer = this;
                overseerInsignia.hadParent = true;



                _oc.insignias.Add(overseerInsignia);

        

                chargeParticle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                if (enraged)
                {
                    overseerInsignia.windUpSpeedMultiplier *= 1.5f;
                }
                overseerInsignia.noTracking = true;
                overseerInsignia.triggerTogether = true;

                switch (_difficulty)
                {
                    case 0:
                    case 1:
                        overseerInsignia.explosionLength = 1.5f;
                        insigniaObj.transform.localScale *= 0.75f;
                        break;

                    case 2:
                    case 3:
                        overseerInsignia.explosionLength = 2f;
                        insigniaObj.transform.localScale *= 1f;
                        break;

                    case 4:
                        overseerInsignia.explosionLength = 2.5f;
                        insigniaObj.transform.localScale *= 1.25f;
                        break;

                    case 5:
                        overseerInsignia.explosionLength = 3f;
                        insigniaObj.transform.localScale *= 1.25f;
                        break;
                }

                if (PlayerTracker.Instance.playerType == PlayerType.Platformer)
                {
                    insigniaObj.transform.localScale *= 0.75f;
                    overseerInsignia.windUpSpeedMultiplier *= 0.875f;
                }

                overseerInsignia.windUpSpeedMultiplier *= Components.EnemyIdentifier.totalSpeedModifier;
                overseerInsignia.damage = Mathf.RoundToInt(overseerInsignia.damage * Components.EnemyIdentifier.totalDamageModifier);
                //_usedAttacks++;

                _oc.overseerUsedBeams++;

                //if (_usedAttacks >= _oc.GetMaxAttackAmount(_difficulty))
                if (_oc.overseerUsedBeams >= _oc.GetMaxAttackAmount(_difficulty))
                {
                    PlaySound(Components.ReadyBeamsSound);
                    Components.Animator?.SetTrigger("WhateverGoByInsignias");
                    _attackCooldown = Random.Range(6f, 4f);
                    Components.PassiveAudioSource.pitch = 0.5f;
                }
            }
        }

        public void TriggerInsignias()
        {

            foreach (var eyeParticleSystem in Components.EyeParticleSystems)
            {
                eyeParticleSystem.Emit(5);
            }

            for (var i = 0; i < _oc.insignias.Count; i++)
            {
                //insignias[i].noTracking = true;

                _oc.insignias[i].Invoke("Activating", (0.25f + i / 2f) / _oc.insignias[i].windUpSpeedMultiplier);
            }
            _oc.insignias.Clear();


            _oc.overseerUsedBeams = 0;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (crashing && (collision.gameObject.layer is 0 or 8 or 24 or 11 or 12 or 26 || collision.gameObject.CompareTag("Player")))
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!crashing) return;


            if (!other.isTrigger && (other.gameObject.layer is 0 or 8 or 24 or 26 || other.gameObject.CompareTag("Player")))
            {
                Explode();
                return;
            }

            if (other.gameObject.layer is not (10 or 11 or 12) || _checkingForCrash) return;


            _checkingForCrash = true;
            var component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
            EnemyIdentifier eid;
            if (component && component.eid)
            {
                eid = component.eid;
            }
            else
            {
                eid = other.gameObject.GetComponent<EnemyIdentifier>();
            }

            if (eid)
            {
                eid.hitter = "cannonball";
                eid.DeliverDamage(other.gameObject,
                    (other.transform.position - transform.position).normalized * 100f, transform.position, 5f * eid.totalDamageModifier, true);
                if (!eid || eid.dead)
                {
                    if (!eid.dead)
                    {
                        StyleHUD.Instance.AddPoints(50, "ultrakill.cannonballed", null, eid);
                    }

                    if (eid)
                    {
                        eid.Explode();
                    }

                    _checkingForCrash = false;
                    return;
                }

                Explode();
            }
            else
            {
                _checkingForCrash = false;
            }
        }

        private void CanInterruptCrash()
        {
            _canInterruptCrash = true;
        }

        public void Hooked()
        {
            _hooked = true;
            lockPosition = true;
            _homeRunnable = true;
            CancelInvoke("DelayedUnhooked");
        }

        public void Unhooked()
        {
            _hooked = false;
            Invoke("DelayedUnhooked", 0.25f);
        }

        private void DelayedUnhooked()
        {
            if (!crashing)
            {
                Invoke("NoMoreHomeRun", 0.5f);
            }

            lockPosition = false;
        }

        private void NoMoreHomeRun()
        {
            if (!crashing)
            {
                _homeRunnable = false;
            }
        }

        public void Enrage()
        {
            if (isEnraged)
            {
                return;
            }


            isEnraged = true;
            _currentEnrageEffect = Instantiate(enrageEffect, transform.position, transform.rotation);
            _currentEnrageEffect.transform.SetParent(transform, true);
            enraged = true;
            var enemySimplifiers = GetComponentsInChildren<EnemySimplifier>();
            foreach (var simplifier in enemySimplifiers)
            {
                simplifier.enraged = true;
            }
        }

        public void UnEnrage()
        {
            if (!isEnraged)
            {
                return;
            }

            isEnraged = false;
            var enemySimplifiers = GetComponentsInChildren<EnemySimplifier>();
            if (enemySimplifiers == null || enemySimplifiers.Length == 0)
            {
                enemySimplifiers = GetComponentsInChildren<EnemySimplifier>();
            }

            Destroy(_currentEnrageEffect);

            foreach (var simplifier in enemySimplifiers)
            {
                simplifier.enraged = false;
            }
        }

        private int RelevantSightBlockMask => LayerMaskDefaults.Get(Target.isPlayer ? LMD.EnvironmentAndBigEnemies : LMD.Environment);

        public bool isEnraged { get; private set; }


        [Header("Knockback")]
        public float knockbackCooldown;
        public float knockbackDistance;
        public ParticleSystem knockbackIndicator;

        private int _knockbacksBeforeEnrage;

        public bool dontStartAware;
        public bool stationary;
        //public float health;
        public bool crashing;

        public float eyeIntensityMax;
        public float eyeIntensityMin;
        public Renderer eyeRenderer;
        private SkinnedMeshRenderer[] _renderers;
        private Transform[] _eyes;
        //private Rigidbody[] _chains;
        public float eyeSpeed;
        private MaterialPropertyBlock _block;

        //private Transform _modelTransform;

        public bool targetSpotted;
        public bool toLastKnownPos;

        private Vector3 _lastKnownPos;


        public float checkCooldown;
        public float blockCooldown;

        public GameObject[] destroyOnCrash;

        private int _attackLimit;
        
        public ParticleSystem chargeParticle;

        public bool enraged;
        public GameObject enrageEffect;

        private Vector3 _crashTarget;
        private bool _canInterruptCrash;


        public bool cantInstaExplode;

        private float _knockbackCooldown;
        private float _dodgeCooldown;
        private float _attackCooldown;

        private float _startingHealth;

        private bool _killedByPlayer;
        private bool _parried;
        private bool _exploded;
        private bool _parryable;
        private Vector3 _viewTarget;

        [HideInInspector] public bool musicRequested;

        private int _difficulty;
        //private int _usedAttacks;
        //[HideInInspector] public List<OverseerInsignia> childVi = new List<OverseerInsignia>();
        //[HideInInspector] public List<OverseerInsignia> insignias = new List<OverseerInsignia>();

        private bool _checkingForCrash;

        [HideInInspector] public bool lockRotation;
        [HideInInspector] public bool lockPosition;

        private bool _hooked;
        private bool _homeRunnable;
        private GameObject _currentEnrageEffect;
        private int _parryFramesLeft;

        private GoreZone _gz;
        private BloodsplatterManager _bsm;
        private StyleCalculator _scalc;

        private AlterOption<bool>[] _options;

        private AlterOption<float>[] _options1;

        private OverseerCooldowns _oc;
        //private Drone _drone;

        public string alterKey => "overseer";
        public string alterCategoryName => "overseer";

        AlterOption<bool>[] IAlterOptions<bool>.options => _options;

        AlterOption<float>[] IAlterOptions<float>.options => _options1;
    }
}
