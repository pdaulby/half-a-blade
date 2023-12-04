using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class CaveGameFirstPersonKinematicBody : MonoBehaviour
    {
        [Header("Move Settings")]
        public float jump = default;
        public float gravity = default;

        [Header("References")]
        public Transform camUpDown = default;
        public Transform orientation = default;
        public KinematicBody3 characterController = default;

        private float verticalInput = 0;
        private float horzInput = 0;

        private bool jumpInput = false;
        private float jumpTime = float.MinValue;

        private float vertical;

        public float Speed = 4; 

        private bool Jump => Input.GetKeyDown(KeyCode.Space);

        void Update()
        {
            if (Pause.paused)
                return; 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            verticalInput = Input.GetAxisRaw("Vertical");
            horzInput = Input.GetAxisRaw("Horizontal");

            if (Jump)
            {
                jumpInput = true;
                jumpTime = Time.time;
            }
            if (Time.time - jumpTime > 0.15f)
            {
                jumpInput = false;
            }
        }

        void FixedUpdate()
        {
            if (characterController.isGrounded)
                vertical = Mathf.Max(0, vertical);

            if (jumpInput && characterController.isGrounded)
            {
                jumpInput = false;
                vertical = jump;
            }
            vertical -= gravity * Time.deltaTime;

            Vector3 horz =
                (orientation.forward * verticalInput + orientation.right * horzInput).normalized
                * Speed;


            characterController.Move(
                horz * Time.deltaTime + new Vector3(0, vertical, 0) * Time.deltaTime
            );
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