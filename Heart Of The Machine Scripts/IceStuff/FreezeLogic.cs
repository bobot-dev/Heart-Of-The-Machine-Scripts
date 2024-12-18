using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;

namespace HeartOfTheMachineScripts.IceStuff
{
    public static class FreezeLogic
    {
        public static void StripForFreeze(Transform target, Material newMaterial = null, bool first = true)
        {
            Component[] components = target.GetComponents<Component>();
            if (first)
            {
                Canvas[] componentsInChildren = target.GetComponentsInChildren<Canvas>(true);

                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    Object.Destroy(componentsInChildren[i].gameObject);
                }


                List<Component> list = new List<Component>();
                list.AddRange(target.GetComponentsInChildren<Joint>(true));
                list.AddRange(target.GetComponentsInChildren<Sisyphus>(true));
                list.AddRange(target.GetComponentsInChildren<AudioDistortionFilter>(true));
                list.AddRange(target.GetComponentsInChildren<AudioHighPassFilter>(true));
                list.AddRange(target.GetComponentsInChildren<AudioLowPassFilter>(true));
                list.AddRange(target.GetComponentsInChildren<ProBuilderMesh>(true));

                foreach (Component obj in list)
                {
                    Object.Destroy(obj);
                }
            }


            if (!first && (target.GetComponent<RemoveOnTime>() || target.GetComponent<SpawnEffect>() || 
                           (target.TryGetComponent(out BoxCollider boxCollider) && boxCollider.isTrigger)))
            {
                Object.Destroy(target.gameObject);
                return;
            }


            foreach (Component component in components)
            {
                Renderer renderer;
                if (newMaterial && (renderer = (component as Renderer)) != null)
                {
                    renderer.materials = SandboxUtils.ToMaterialArray(newMaterial, renderer.materials.Length);
                }
                else if (!(component as Transform) && !(component as MeshFilter) && !(component as MeshRenderer) && !(component as SkinnedMeshRenderer) &&
                         (
                             (first && !allowedFirstTypes.Contains(component.GetType()) || 
                           (!first && !allowedOtherTypes.Contains(component.GetType())))))
                {
                    Object.Destroy(component);
                }
            }
            foreach (Transform child in target.transform)
            {
                StripForFreeze(child, newMaterial, false);
            }
        }

        private static Type[] allowedFirstTypes = new Type[] { typeof(Animator) };
        private static Type[] allowedOtherTypes = new Type[] { typeof(BoxCollider), typeof(CapsuleCollider), typeof(SphereCollider), typeof(Rigidbody) };
    }

    
}
