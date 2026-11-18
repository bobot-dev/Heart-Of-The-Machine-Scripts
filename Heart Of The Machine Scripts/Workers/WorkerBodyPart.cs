using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SettingsMenu.Components.Pages;
using UnityEngine;
using static HeartOfTheMachineScripts.Worker;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

namespace HeartOfTheMachineScripts.Workers
{
    public class WorkerBodyPart : MonoBehaviour
    {
        private void Awake()
        {
            eidid = GetComponent<EnemyIdentifierIdentifier>();
            worker = GetComponentInParent<Worker>();
        }

        private void Start()
        {
            gz = GoreZone.ResolveGoreZone(transform);
            health = maxHealth;
        }

        public void Damage(float damage)
        {
            health -= damage;

            if (health <= 0)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            SpawnGore(gameObject);

            if (!destroyChildrenOnDeath && transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
                if (componentsInChildren.Length != 0)
                {
                    foreach (CharacterJoint childJoint in componentsInChildren)
                    {
                        if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && childJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                        {
                            enemyIdentifierIdentifier.Invoke(nameof(EnemyIdentifierIdentifier.DestroyLimbIfNotTouchedBloodAbsorber), StockMapInfo.Instance.gibRemoveTime);
                        }

                        Destroy(childJoint);
                    }
                }

                if (TryGetComponent(out CharacterJoint joint))
                {
                    joint.connectedBody = null;
                    Destroy(joint);
                }

                transform.position = child.position;
                transform.SetParent(child);
                child.SetParent(gz.gibZone);
                Destroy(GetComponent<Rigidbody>());
            }

            if (TryGetComponent<Collider>(out var obj))
            {
                Destroy(obj);
            }

            transform.localScale = Vector3.zero;

            foreach (var transform1 in linked)
            {
                if (!transform1)
                    continue;

                if (!destroyChildrenOnDeath && transform1.childCount > 0)
                {
                    Transform child = transform1.GetChild(0);
                    CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
                    if (componentsInChildren.Length != 0)
                    {
                        foreach (CharacterJoint childJoint in componentsInChildren)
                        {
                            if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && childJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyIdentifierIdentifier))
                            {
                                enemyIdentifierIdentifier.Invoke(nameof(EnemyIdentifierIdentifier.DestroyLimbIfNotTouchedBloodAbsorber), StockMapInfo.Instance.gibRemoveTime);
                            }

                            Destroy(childJoint);
                        }
                    }

                    if (TryGetComponent(out CharacterJoint joint))
                    {
                        joint.connectedBody = null;
                        Destroy(joint);
                    }

                    transform1.position = child.position;
                    transform1.SetParent(child);
                    child.SetParent(gz.gibZone);
                    Destroy(GetComponent<Rigidbody>());
                }

                if (TryGetComponent(out obj))
                {
                    Destroy(obj);
                }

                transform1.localScale = Vector3.zero;

            }
        }

        private void SpawnGore(GameObject target)
        {
            for (var i = 0; i < gibTypes.Length; i++)
            {
                var gibData = gibTypes[i];
                int amount = Random.Range(gibMins[i], gibMaxes[i]);

                for (int j = 0; j < amount; j++)
                {
                    GameObject gib = worker.GetGib(gibData);
                    if (gib && gz && gz.gibZone)
                    {
                        ReadyGib(gib, target);
                    }
                }
            }
        }

        public void ReadyGib(GameObject tempGib, GameObject target)
        {
            tempGib.transform.SetPositionAndRotation(target.transform.position, Random.rotation);
            gz.SetGoreZone(tempGib);
            if (!GraphicsSettings.bloodEnabled)
            {
                tempGib.SetActive(false);
            }
        }
        //[SerializeField]
        public WorkerGibType[] gibTypes;
        public int[] gibMins;
        public int[] gibMaxes;
        public Transform[] linked;

        public bool destroyChildrenOnDeath;

        // [SerializeField]
        // public List<GibData> gibs;

        public float maxHealth;

        [HideInInspector]
        public float health;

        [HideInInspector]
        public EnemyIdentifierIdentifier eidid;

        [HideInInspector]
        public GoreZone gz;

        [HideInInspector]
        public Worker worker;
    }

    [Serializable]
    public class GibData
    {
        public GibData(WorkerGibType gibType, int min, int max)
        {
            this.gibType = gibType;
            this.min = min;
            this.max = max;
        }

        [SerializeField]
        public WorkerGibType gibType;
        [SerializeField]
        public int min;
        [SerializeField]
        public int max;
    }


    [Serializable]
    public enum WorkerGibType
    {
        Gear = 0,
        Wires = 1,
        Bolt = 2,
        Nut = 3,
        CircuitBoard = 4
    }
}
