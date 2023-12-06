using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class CaveGameFirstPersonKinematicBody : MonoBehaviour
    {
        [Header("References")]
        public Transform camUpDown = default;
        public Transform orientation = default;
        public KinematicBody3 characterController = default;

        void Update()
        {
            if (Pause.paused)
                return; 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            orientation.localPosition = Vector3.zero;

            characterController.ResetCollider();
        }

        public void SnapToGroundFar(float distance)
        {
            characterController.CapsuleSweep(KinematicBody3.SweepType.VERTICAL, Vector3.down, distance);
        }
    }
}