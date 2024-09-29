using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class LightningPowerupPickup : MonoBehaviour, IAlter, IAlterOptions<float>, IAlterOptions<bool>
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PickedUp();
            }
        }

        private void PickedUp()
        {
            if (GunControl.Instance)
            {
                Instantiate(pickUpEffect, transform.position, Quaternion.identity);
                CameraController.Instance.CameraShake(0.35f);
                if (!infiniteUses)
                {
                    gameObject.SetActive(false);
                }

                if (PlayerTracker.Instance.playerType == PlayerType.Platformer)
                {
                    PlatformerMovement.Instance.AddExtraHit(3);
                    return;
                }

                StaticCharge.instance.StartEffect(juiceAmount);

                /*
                var gameObject = new GameObject();
                gameObject.transform.SetParent(MonoSingleton<GunControl>.Instance.transform, true);
                gameObject.transform.localRotation = Quaternion.identity;
                var componentsInChildren = GunControl.Instance.GetComponentsInChildren<DualWield>();
                if (componentsInChildren != null && componentsInChildren.Length % 2 == 0)
                {
                    gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    gameObject.transform.localScale = Vector3.one;
                }

                if (componentsInChildren == null || componentsInChildren.Length == 0)
                {
                    gameObject.transform.localPosition = Vector3.zero;
                }
                else if (componentsInChildren.Length % 2 == 0)
                {
                    gameObject.transform.localPosition = new Vector3(componentsInChildren.Length / 2 * -1.5f, 0f, 0f);
                }
                else
                {
                    gameObject.transform.localPosition = new Vector3((componentsInChildren.Length + 1) / 2 * 1.5f, 0f, 0f);
                }

                var dualWield = gameObject.AddComponent<DualWield>();
                dualWield.delay = 0.05f;
                dualWield.juiceAmount = juiceAmount;
                if (componentsInChildren != null && componentsInChildren.Length != 0)
                {
                    dualWield.delay += componentsInChildren.Length / 20f;
                }*/
            }
        }

        public string alterKey => "lightning-pickup";

        public string alterCategoryName => "Static Charge Pickup";

        AlterOption<float>[] IAlterOptions<float>.options
        {
            get
            {
                return new AlterOption<float>[]
                {
                    new AlterOption<float>
                    {
                        name = "Juice",
                        key = "juice",
                        value = juiceAmount,
                        callback = delegate(float value)
                        {
                            juiceAmount = value;
                        },
                        constraints = new SliderConstraints
                        {
                            min = 0f,
                            max = 100f,
                            step = 1f
                        }
                    }
                };
            }
        }

        AlterOption<bool>[] IAlterOptions<bool>.options
        {
            get
            {
                return new AlterOption<bool>[]
                {
                new AlterOption<bool>
                {
                    name = "Infinite Uses",
                    key = "infinite-uses",
                    value = infiniteUses,
                    callback = delegate(bool value)
                    {
                        infiniteUses = value;
                    }
                }
                };
            }
        }

        public bool infiniteUses;

        public float juiceAmount = 30f;

        public GameObject pickUpEffect;
    }

}
