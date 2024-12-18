using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UIR.BestFitAllocator;
using Random = UnityEngine.Random;

namespace HeartOfTheMachineScripts.IceStuff
{

    public class FrozenEnemy : MonoBehaviour
    {
        private void Start()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _blocks = new MaterialPropertyBlock[_renderers.Length];

            _rigidbodies = GetComponentsInChildren<Rigidbody>();

            _breakable = holder.gameObject.AddComponent<Breakable>();

            _breakable.activateOnBreak = new GameObject[0];
            _breakable.destroyOnBreak = new GameObject[0];

            _breakable.weak = true;
            _breakable.particleAtBoundsCenter = true;
            

            _breakable.destroyEvent = new UltrakillEvent
            {
                onActivate = new UnityEvent()
            };

            _breakable.destroyEvent.onActivate.AddListener(Gib);

            gameObject.tag = "Breakable";

            for (int i = 0; i < _blocks.Length; i++)
            {
                _blocks[i] = new MaterialPropertyBlock();

                _renderers[i].sharedMaterial = StaticRefs.Instance.frozenMaterial;

                // _renderers[i].GetPropertyBlock(_blocks[i], 0);
                // _blocks[i].SetColor("_Color", Color.blue);
                // _renderers[i].SetPropertyBlock(_blocks[i]);
            }

            //SandboxUtils.SetLayerDeep(transform, 8);

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            //if (canGib)

        }


        private void Gib()
        {
            CancelInvoke("Gib");

            
        




            var gz = GoreZone.ResolveGoreZone(transform);
            if (gz)
            {
                var renderers = holder.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (renderers[0])
                    Instantiate(StaticRefs.Instance.shatterVFX, renderers[0].bounds.center, transform.rotation);

                transform.SetParent(gz.gibZone, true);

                foreach (var renderer in renderers)
                {
                    renderer.updateWhenOffscreen = true;
                }

                foreach (var rigidbody in _rigidbodies)
                {
                    if (!rigidbody)
                        continue;

                    rigidbody.gameObject.tag = "Untagged";
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                    rigidbody.transform.SetParent(gz.gibZone, true);


                    var direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
   
                    rigidbody.AddForce(direction * Random.Range(5, 10), ForceMode.VelocityChange);

                    if (rigidbody.TryGetComponent(out Collider collider))
                        collider.enabled = true;
                }
            }
            Destroy(holder.gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 14 || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
            {
                Gib();
            }
        }


        private Renderer[] _renderers;
        private MaterialPropertyBlock[] _blocks;

        public Transform holder;

        public bool canGib;
        private Breakable _breakable;
        private Rigidbody[] _rigidbodies;
    }
}
