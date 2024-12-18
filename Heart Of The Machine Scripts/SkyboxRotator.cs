using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class SkyboxRotator : MonoBehaviour
    {
        private void Update()
        {
            if (RenderSettings.skybox)
            {
                if (!_tempSkybox)
                {
                    UpdateSkyboxMaterial();
                }
                else
                {
                    _skyboxRotation += Time.deltaTime * speed;
                    if (_skyboxRotation >= 360f)
                    {
                        _skyboxRotation -= 360f;
                    }
                    RenderSettings.skybox.SetFloat("_Rotation", _skyboxRotation);
                }
            }
        }

        public void UpdateSkyboxMaterial()
        {
            if (!_skyboxMaterial)
            {
                _skyboxMaterial = RenderSettings.skybox;
            }
            _tempSkybox = new Material(_skyboxMaterial);
            RenderSettings.skybox = _tempSkybox;
        }

        public float speed;

        private Material _skyboxMaterial;
        private Material _tempSkybox;
        private float _skyboxRotation;
    }
}
