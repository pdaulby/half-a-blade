using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class CameraLook : MonoBehaviour
    {
        [Header("References")]
        public Transform camUpDown = default;
        public Transform orientation = default;

        [Header("Input Settings (move!)")]
        public float mouseSensitivityVert = 3;
        public float mouseSensitivityHorz = 3;

        private float cameraVerticalAngle = 0;

        void Update()
        {
            float turnLeftRight = Input.GetAxisRaw("Mouse X") * mouseSensitivityHorz;
            float lookUpDown = Input.GetAxisRaw("Mouse Y") * mouseSensitivityVert;

            cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle - lookUpDown, -90, 90);
            camUpDown.transform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0, 0);

            orientation.Rotate(0, turnLeftRight, 0);
            // consider a max turn speed.
        }
    }
}