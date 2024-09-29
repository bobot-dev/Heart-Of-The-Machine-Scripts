using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance)]
    public class StaticRefs : MonoSingleton<StaticRefs>
    {
        public Texture2D titanBloodTexture;
        public Material ichorBloodStain;

        public Bounds ichorBloodstainBounds;
        public NativeArray<BloodsplatterManager.InstanceProperties> ichorProps;
        public ComputeBuffer ichorParentBuffer;
        public NativeArray<float4x4> ichorParents;
        private int ichorPropIndex;
        public ComputeBuffer ichorInstanceBuffer;

        public override void Awake()
        {
            base.Awake();
            //bloodstainBounds = new Bounds(Vector3.zero, Vector3.one * 100000f);
            ichorProps = new NativeArray<BloodsplatterManager.InstanceProperties>((int)MonoSingleton<PrefsManager>.Instance.GetFloatLocal("bloodStainMax", 100000f), Allocator.Persistent, NativeArrayOptions.ClearMemory);
            //checkpointProps = new NativeArray<BloodsplatterManager.InstanceProperties>(ichorProps.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            ichorInstanceBuffer = new ComputeBuffer(ichorProps.Length, 32, ComputeBufferType.Structured);
            ichorInstanceBuffer.SetData<BloodsplatterManager.InstanceProperties>(ichorProps);
            //clearJob.ichorProps = ichorProps;
            ichorParents = new NativeArray<float4x4>(512, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            ichorParentBuffer = new ComputeBuffer(ichorParents.Length, 64, ComputeBufferType.Structured);
            ichorParents[0] = float4x4.identity;
            ichorBloodStain.SetBuffer("instanceBuffer", ichorInstanceBuffer);
            ichorBloodStain.SetBuffer("parentBuffer", ichorParentBuffer);
            
        }


        public int CreateIchorBloodstain(float3 pos, float3 norm, int parent = 0)
        {
            Debug.Log("ichor made at " + pos);
            ichorPropIndex = (ichorPropIndex + 1) % ichorProps.Length;
            ichorProps[ichorPropIndex] = new BloodsplatterManager.InstanceProperties
            {
                pos = pos,
                norm = norm,
                parentIndex = parent
            };
            //reuseStainIndex?(ichorPropIndex);
            ichorInstanceBuffer.SetData(ichorProps, ichorPropIndex, ichorPropIndex, 1);
            return ichorPropIndex;
        }

        public static void GetHealthOverheal(int health, bool silent, bool fromExplosion = false)
        {
            if (NewMovement.Instance && !NewMovement.Instance.dead && (!NewMovement.Instance.exploded || !fromExplosion))
            {
                float currentHealth = health;
                float maxHealth = 200;
                if (NewMovement.Instance.difficulty == 0)// || (NewMovement.Instance.difficulty == 1 && NewMovement.Instance.sameCheckpointRestarts > 2))
                {
                    maxHealth = 400f;
                }
                if (currentHealth < 1f)
                {
                    currentHealth = 1f;
                }
                if (NewMovement.Instance.hp <= maxHealth)
                {
                    if (NewMovement.Instance.hp + currentHealth < maxHealth - Mathf.RoundToInt(NewMovement.Instance.antiHp))
                    {
                        NewMovement.Instance.hp += Mathf.RoundToInt(currentHealth);
                    }
                    else if (NewMovement.Instance.hp != maxHealth - Mathf.RoundToInt(NewMovement.Instance.antiHp))
                    {
                        NewMovement.Instance.hp = Mathf.RoundToInt(maxHealth) - Mathf.RoundToInt(NewMovement.Instance.antiHp);
                    }
                    NewMovement.Instance.hpFlash.Flash(1f);
                    if (!silent && health > 5)
                    {
                        if (NewMovement.Instance.greenHpAud == null)
                        {
                            NewMovement.Instance.greenHpAud = NewMovement.Instance.hpFlash.GetComponent<AudioSource>();
                        }
                        NewMovement.Instance.greenHpAud.Play();
                    }
                }
                if (!silent && health > 5 && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
                {
                    Instantiate(NewMovement.Instance.scrnBlood, NewMovement.Instance.fullHud.transform);
                }
            }
        }
    }
}
