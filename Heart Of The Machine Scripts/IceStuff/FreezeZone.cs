using UnityEngine;

namespace HeartOfTheMachineScripts.IceStuff
{
    public class FreezeZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Enemy") && other.TryGetComponent(out EnemyIdentifier eid) && !eid.blessed)
            {
                GameObject holder = new GameObject("frozen enemy holder")
                {
                    transform =
                    {
                        position = eid.transform.position
                    },
                    layer = 8
                };
                holder.transform.SetParent(GoreZone.ResolveGoreZone(eid.transform).gibZone);

                GameObject fakeEnemy = Instantiate(eid.gameObject, eid.transform.position, eid.transform.rotation);
                fakeEnemy.transform.localScale = eid.transform.lossyScale;
                fakeEnemy.transform.SetParent(holder.transform, true);


                eid.gameObject.SetActive(false);
                eid.hitter = "freeze";
                eid.InstaKill();


                Destroy(eid.gameObject);
                FreezeLogic.StripForFreeze(fakeEnemy.transform);

                if (freezeSound)
                    Instantiate(freezeSound, fakeEnemy.transform.position, Quaternion.identity, fakeEnemy.transform);


                Rigidbody rigidbody = holder.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;
                rigidbody.mass = 30;

                if (Random.Range(0f, 1f) < 0.2)
                    rigidbody.velocity = new Vector3(Random.Range(0f, 1f), 0, Random.Range(0f, 1f)).normalized * 5;



                if (fakeEnemy.TryGetComponent(out CapsuleCollider collider))
                {
                    CapsuleCollider capsuleCollider = holder.AddComponent<CapsuleCollider>();

                    capsuleCollider.center = collider.center;
                    capsuleCollider.radius = collider.radius / 2;
                    capsuleCollider.direction = collider.direction;
                    capsuleCollider.height = collider.height;
                    //capsuleCollider.sharedMaterial = collider.sharedMaterial;
                }

                if (fakeEnemy.TryGetComponent(out Animator animator))
                {
                    animator.enabled = false;
                }

                //Debug.Log("froze animator");

                var frozenLogic = fakeEnemy.gameObject.AddComponent<FrozenEnemy>();
                frozenLogic.holder = holder.transform;

            }
        }

        public GameObject freezeSound;
    }
}
