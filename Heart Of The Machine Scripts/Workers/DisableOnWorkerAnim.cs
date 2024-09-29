using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace HeartOfTheMachineScripts
{
    public class DisableOnWorkerAnim : MonoBehaviour
    {

        public void Trigger(string animation)
        {
            if (animation != this.animation)
                gameObject.SetActive(false);
        }


        [HideInInspector]
        public string animation;

        [HideInInspector]
        public int animationIndex;
    }

}
