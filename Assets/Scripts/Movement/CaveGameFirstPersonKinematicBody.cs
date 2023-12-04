using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class CaveGameFirstPersonKinematicBody : MonoBehaviour
    {
        [Header("Move Settings")]
        public float jump = default;
        public float gravity = default;

        [Header("Input Settings (consider moving!)")]
        public float mouseSensitivityVert = default;
        public float mouseSensitivityHorz = default;

        [Header("References")]
        public Transform camUpDown = default;
        public Transform orientation = default;
        public KinematicBody3 characterController = default;
        private float vertical;
        public bool lockMovement;

        private float verticalInput = 0;
        private float horzInput = 0;

        private bool jumpInput = false;
        private float jumpTime = float.MinValue;

        private float cameraVerticalAngle = 0;

        private float Speed => 4; 

        private bool Jump => Input.GetKeyDown(KeyCode.Space);

        void Start() 
        {
        }

        void Update()
        {
            if (Pause.paused)
                return; 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float turnLeftRight = Input.GetAxisRaw("Mouse X") * mouseSensitivityHorz;
            float lookUpDown = Input.GetAxisRaw("Mouse Y") * mouseSensitivityVert;

            cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle - lookUpDown, -90, 90);
            camUpDown.transform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0, 0);

            orientation.Rotate(0, turnLeftRight, 0);

            verticalInput = Input.GetAxisRaw("Vertical");
            horzInput = Input.GetAxisRaw("Horizontal");

            if (!lockMovement && Jump)
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
            if (!lockMovement)
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

                // If on slope, no inputs for the player :(
                //if (characterController.isOnSlope)
                //    horz = Vector3.zero;

                characterController.Move(
                    horz * Time.deltaTime + new Vector3(0, vertical, 0) * Time.deltaTime
                );
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            orientation.localPosition = Vector3.zero;

            characterController.ResetCollider();
            characterController.Depenetrate();
        }

        public void SnapToGroundFar(float distance)
        {
            characterController.CapsuleSweep(KinematicBody3.SweepType.VERTICAL, Vector3.down, distance);
        }
    }
}