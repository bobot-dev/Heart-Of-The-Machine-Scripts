using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    [RequireComponent(typeof(Camera))]
    public class RealtimeReflectionHelper : MonoBehaviour
    {
        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            _cc = CameraController.Instance;

            renderTexture.Release();
            renderTexture.width = Screen.width;
            renderTexture.height = Screen.height;
            renderTexture.Create();
        }

        private void Update()
        {
            if (_cc && _camera)
            {
                _camera.fieldOfView = _cc.cam.fieldOfView;
            }
        }

        public RenderTexture renderTexture;
        private Camera _camera;
        private CameraController _cc;
    }
}
