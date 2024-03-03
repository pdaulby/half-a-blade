using Assets.Scripts.Movement.Components;
using Assets.Scripts.Movement.Inputs;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public Transform orientation = default;
        public KinematicBody3 characterController = default;

        private ControllerComponent[] components;
        private InputReader inputReader = new();
        private FixedInput input = new();

        // velocity per second
        public Vector3 Velocity { get; set; } = new();
        public Vector3 PreviousVelocity { get; private set; } = new();
        public Vector3 PreviousPosition { get; private set; } = new();

        // stuff from controller, move I guess
        //public Vector3 ActualVelocity { get { return characterController.velocity; } }
        public bool IsGrounded { get { return characterController.isGrounded; } }
        //public Collider GroundedCollider { get { throw new NotImplementedException("GroundedCollider"); } }
        public Vector3 GroundedNormal { get { return characterController.groundedNormal; } }
        public float SlopeAngle { get { return characterController.slopeAngle; } }

        private void Awake()
        {
            components = GetComponents<ControllerComponent>();
        }

        void Update()
        {
            inputReader.Update();
        }

        void FixedUpdate()
        {
            PreviousPosition = characterController.transform.position;
            input = inputReader.GetNext();

            components.Where(c => c.CanUse(this)).ToList()
                .ForEach(c => c.DoUpdate(input, this));

            Debug.Log(Velocity);
            characterController.Move(Velocity * Time.deltaTime);
            PreviousVelocity = Velocity;
        }
    }
}