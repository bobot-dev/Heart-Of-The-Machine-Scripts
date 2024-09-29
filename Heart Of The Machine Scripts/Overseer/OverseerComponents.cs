using System;
using System.Collections.Generic;
using System.Text;
using Logic;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    //[RequireComponent(typeof(Overseer))]
    public class OverseerComponents : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private Transform rootTransform;
        public Transform RootTransform => rootTransform;

        [SerializeField] private Transform centerMass;
        public Transform CenterMass => centerMass;

        [SerializeField] private Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        [SerializeField] private Animator animator;
        public Animator Animator => animator;

        [SerializeField] private Machine machine;
        public Machine Machine => machine;

        [SerializeField] private MapBoolSetter unlockSetter;
        public MapBoolSetter UnlockSetter => unlockSetter;

        [SerializeField] private MapIntSetter killCount;
        public MapIntSetter KillCount => killCount;

        [SerializeField] private EnemyIdentifier eid;
        public EnemyIdentifier EnemyIdentifier => eid;

        [SerializeField] private KeepInBounds kib;
        public KeepInBounds KeepInBounds => kib;

        [SerializeField] private AudioSource aud;
        public AudioSource AudioSource => aud;

        [SerializeField] private AudioSource passiveAud;
        public AudioSource PassiveAudioSource => passiveAud;

        [SerializeField] private Rigidbody[] rbToGibOnDeath;
        public Rigidbody[] RigidbodiesToGibOnDeath => rbToGibOnDeath ?? Array.Empty<Rigidbody>();

        [SerializeField] private Rigidbody[] rbToGibOnCrash;
        public Rigidbody[] RigidbodiesToGibOnCrash => rbToGibOnCrash ?? Array.Empty<Rigidbody>();

        [SerializeField] private ParticleSystem[] eyeParticleSystems;
        public ParticleSystem[] EyeParticleSystems => eyeParticleSystems ?? Array.Empty<ParticleSystem>();


        [Header("Asset References")]

        [SerializeField] private GameObject insignia;
        public GameObject Insignia => insignia;

        [SerializeField] private GameObject gasoline;
        public GameObject GasolineProjectile => gasoline;

        [SerializeField] private int gasolineAmount;
        public int GasolineProjectileAmount => gasolineAmount;

        [SerializeField] private int gasolineAmountVariance;
        public int GasolineProjectileAmountVariance => gasolineAmountVariance;


        [SerializeField] private GameObject shatterEffect;
        public GameObject ShatterEffect => shatterEffect;

        [SerializeField] private GameObject gib;
        public GameObject Gib => gib;

        [SerializeField] private GameObject knockbackExplosion;
        public GameObject KnockbackExplosion => knockbackExplosion;

        [SerializeField] private GameObject knockbackReady;
        public GameObject KnockbackReady => knockbackReady;


        [SerializeField] private AudioClip hurtSound;
        public AudioClip HurtSound => hurtSound;

        [SerializeField] private AudioClip deathSound;
        public AudioClip DeathSound => deathSound;

        [SerializeField] private AudioClip windUpSound;
        public AudioClip WindUpSound => windUpSound;

        [SerializeField] private AudioClip readyBeamsSound;
        public AudioClip ReadyBeamsSound => readyBeamsSound;
    }
}
