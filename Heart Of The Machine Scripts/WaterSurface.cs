using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;

namespace HeartOfTheMachineScripts
{
    public class WaterSurface : MonoBehaviour
    {
        public Mesh mesh;

        private void Start()
        {
            cb = new CommandBuffer
            {
                name = "PaintRenderer"
            };
            absorbers = GetComponentsInChildren<MeshRenderer>();
            absorberMFs = new MeshFilter[absorbers.Length];
            var array = new CombineInstance[absorbers.Length];
            for (var i = 0; i < absorbers.Length; i++)
            {
                var meshRenderer = absorbers[i];
                if (meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    absorberMFs[i] = meshFilter;
                    array[i].mesh = meshFilter.sharedMesh;
                }
                else
                {
                    Debug.LogError("No mesh found for: " + meshRenderer.gameObject.name);
                }
            }
            mesh = new Mesh();
            mesh.CombineMeshes(array);
            var num = Math.Min(Mathf.CeilToInt(Mathf.Sqrt(mesh.GetUVDistributionMetric(1)) * texelsPerWorldUnit), 4096);
            var num2 = Mathf.Log(num, 2f);
            num2 = Mathf.Round(num2);
            num2 = Mathf.Pow(2f, num2);
            var num3 = (int)num2;
            propBlock = new MaterialPropertyBlock();
            absMat = new Material(absorptionShader);
            paintBuffer = new RenderTexture(num, num, 0, RenderTextureFormat.R16, 0)
            {
                filterMode = FilterMode.Point
            };
            bloodMap = new RenderTexture(num, num, 0, RenderTextureFormat.R8, 0)
            {
                filterMode = FilterMode.Point
            };
            dilationMask = new RenderTexture(num, num, 0, RenderTextureFormat.R8, 0)
            {
                filterMode = FilterMode.Point
            };
            clampedMap = new RenderTexture(num3, num3, 0, RenderTextureFormat.RHalf)
            {
                filterMode = FilterMode.Point,
                useMipMap = true,
                autoGenerateMips = false
            };
            paintBuffer.Create();
            bloodMap.Create();
            clampedMap.Create();
            dilationMask.Create();
            InitializeRTs();
            InitializeDilationMask();
            propBlock.SetTexture("_BloodBuffer", paintBuffer);
            propBlock.SetTexture("_BloodTex", bloodMap);
            propBlock.SetTexture("_DilationMask", dilationMask);
            foreach (var absorber in absorbers)
            {
                absorber.SetPropertyBlock(propBlock);
            }
            if (visibilityMask == null)
            {
                visibilityMask = Texture2D.whiteTexture;
            }
            cbuff = new ComputeBuffer(100, 16);
            collisionDataList = new List<BloodAbsorber.CollisionData>(cbuff.count);
        }

        /*
        private IEnumerator ClearedAbsorber()
        {
            isCompleted = true;
            var time = 0f;
            while (time < 1f)
            {
                var value = Mathf.Lerp(0f, 0.06f, time);
                propBlock.SetFloat("_ReflectionStrength", value);
                var value2 = Color.Lerp(new Color(1f, 0.9f, 0.5f), Color.white, time);
                propBlock.SetColor("_EmissiveColor", value2);
                var value3 = Mathf.Lerp(2f, 1f, time);
                propBlock.SetFloat("_EmissiveIntensity", value3);
                var num = absorbers.Length;
                for (var i = 0; i < num; i++)
                {
                    absorbers[i].SetPropertyBlock(propBlock);
                }
                time += Time.deltaTime;
                yield return null;
            }
            yield break;
        }

        private void UnclearedAbsorber()
        {
            isCompleted = false;
            propBlock.SetFloat("_ReflectionStrength", 0f);
            propBlock.SetColor("_EmissiveColor", Color.white);
            propBlock.SetFloat("_EmissiveIntensity", 1f);
            var num = absorbers.Length;
            for (var i = 0; i < num; i++)
            {
                absorbers[i].SetPropertyBlock(propBlock);
            }
        }*/

        /*private void OnValidate()
        {
            if (clearBlood)
            {
                InitializeRTs();
                clearBlood = false;
            }
            if (fillBlood)
            {
                Graphics.Blit(visibilityMask, bloodMap, absMat, 7);
                fillBlood = false;
            }
            var num = visibilityMask.width * visibilityMask.height;
            var num2 = 0f;
            for (var i = 0; i < visibilityMask.width; i++)
            {
                for (var j = 0; j < visibilityMask.height; j++)
                {
                    num2 += visibilityMask.GetPixel(i, j).r;
                }
            }
            maxFill = num2 / num;
        }*/

        private void InitializeRTs()
        {
            cb.Clear();
            cb.SetRenderTarget(paintBuffer);
            cb.ClearRenderTarget(true, true, Color.black);
            cb.SetRenderTarget(bloodMap);
            cb.ClearRenderTarget(true, true, Color.black);
            Graphics.ExecuteCommandBuffer(cb);
        }

        private void InitializeDilationMask()
        {
            cb.Clear();
            cb.SetRenderTarget(dilationMask);
            for (var i = 0; i < absorbers.Length; i++)
            {
                var subMeshCount = absorberMFs[i].sharedMesh.subMeshCount;
                for (var j = 0; j < subMeshCount; j++)
                {
                    cb.DrawRenderer(absorbers[i], absMat, j, 6);
                }
            }
            Graphics.ExecuteCommandBuffer(cb);
        }

        private void Update()
        {
            if (!isSleeping && sleepTimer >= timeUntilSleep)
            {
                cb.Clear();
                cb.SetRenderTarget(paintBuffer);
                cb.ClearRenderTarget(true, true, Color.black);
                Graphics.ExecuteCommandBuffer(cb);
                clearBlood = false;
                isSleeping = true;
                return;
            }
            if (bloodTimer >= bloodUpdateRate)
            {
                Graphics.Blit(null, paintBuffer, absMat, 1);
                absMat.SetTexture("_VisibilityMask", visibilityMask);
                Graphics.Blit(paintBuffer, bloodMap, absMat, isWashing ? 3 : 2);
                bloodTimer = 0f;
            }
            bloodTimer += Time.deltaTime;
            sleepTimer += Time.deltaTime;
        }

        public void ProcessWasherSpray(ref List<ParticleCollisionEvent> pEvents, Vector3 position)
        {
            if (!isWashing)
            {
                cb.Clear();
                cb.SetRenderTarget(paintBuffer);
                cb.ClearRenderTarget(false, true, Color.black);
            }
            isWashing = true;
            collisionDataList.Clear();
            var num = 0;
            foreach (var particleCollisionEvent in pEvents)
            {
                if (num >= cbuff.count)
                {
                    break;
                }
                var item = new BloodAbsorber.CollisionData
                {
                    position = particleCollisionEvent.intersection,
                    distance = Vector3.Distance(particleCollisionEvent.intersection, position)
                };
                collisionDataList.Add(item);
                num++;
            }
            cbuff.SetData(collisionDataList);
            isSleeping = false;
            sleepTimer = 0f;
            absMat.SetFloat("_HitCount", num);
            absMat.SetBuffer("_HitData", cbuff);
            absMat.SetTexture("_MainTex", paintBuffer);
            absMat.SetTexture("_NoiseTex", noiseTex);
            cb.Clear();
            cb.SetRenderTarget(paintBuffer);
            propBlock.SetFloat("_IsWashing", 1f);
            var num2 = absorbers.Length;
            for (var i = 0; i < num2; i++)
            {
                var meshRenderer = absorbers[i];
                var subMeshCount = absorberMFs[i].sharedMesh.subMeshCount;
                for (var j = 0; j < subMeshCount; j++)
                {
                    cb.DrawRenderer(meshRenderer, absMat, j, 4);
                }
                meshRenderer.SetPropertyBlock(propBlock);
            }
            Graphics.ExecuteCommandBuffer(cb);
            absMat.SetTexture("_VisibilityMask", visibilityMask);
            Graphics.Blit(paintBuffer, bloodMap, absMat, 3);
        }

        public bool HandleBloodstainHit(ref RaycastHit hit)
        {
            if (isWashing)
            {
                cb.Clear();
                cb.SetRenderTarget(paintBuffer);
                cb.ClearRenderTarget(false, true, Color.black);
            }
            isWashing = false;
            isSleeping = false;
            sleepTimer = 0f;
            Vector3 point = hit.point;
            Vector3 normal = hit.normal;
            Vector3 v = normal * -1f;
            Quaternion lhs = Quaternion.LookRotation(normal, Vector3.up);
            Quaternion rhs = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), normal);
            Matrix4x4 value = Matrix4x4.Rotate(lhs * rhs);
            absMat.SetVector("_HitPos", point);
            absMat.SetVector("_HitNorm", v);
            absMat.SetMatrix("_RotMat", value);
            absMat.SetTexture("_MainTex", paintBuffer);
            absMat.SetTexture("_NoiseTex", noiseTex);
            absMat.SetTexture("_VisibilityMask", visibilityMask);
            cb.Clear();
            cb.SetRenderTarget(paintBuffer);
            propBlock.SetFloat("_IsWashing", 0f);
            int num = absorbers.Length;
            for (int i = 0; i < num; i++)
            {
                MeshRenderer meshRenderer = absorbers[i];
                int subMeshCount = absorberMFs[i].sharedMesh.subMeshCount;
                for (int j = 0; j < subMeshCount; j++)
                {
                    cb.DrawRenderer(meshRenderer, absMat, j, 0);
                }
                meshRenderer.SetPropertyBlock(propBlock);
            }
            Graphics.ExecuteCommandBuffer(cb);
            Graphics.Blit(paintBuffer, bloodMap, absMat, 2);
            return true;
        }

        public Shader absorptionShader;

        public Texture noiseTex;

        public Texture2D visibilityMask;

        public int texelsPerWorldUnit = 10;

        public float bloodUpdateRate = 0.02f;

        public float timeUntilSleep = 4f;

        public bool clearBlood;

        public bool fillBlood;

        private float sleepTimer;

        private bool isWashing;

        private float bloodTimer;

        private bool isSleeping = true;

        private CommandBuffer cb;

        private Material absMat;

        public RenderTexture paintBuffer;

        public RenderTexture bloodMap;

        public RenderTexture clampedMap;

        public RenderTexture dilationMask;

        private MeshRenderer[] absorbers;

        private MeshFilter[] absorberMFs;

        private MaterialPropertyBlock propBlock;

        private ComputeBuffer cbuff;

        private List<BloodAbsorber.CollisionData> collisionDataList;

        private Coroutine checkFillRoutine;

        private struct CollisionData
        {
            public Vector3 position;

            public float distance;
        }
    }
}
