using Assets.Scripts.Movement.Inputs;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class Gravity : ControllerComponent
    {
        public float gravity = 9.8f;

        private Vector3 gravityDirection = Vector3.down;

        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (controller.IsGrounded)
            {
                //TODO handle ground collisions elsewhere perhaps?
                controller.Velocity -= Vector3.Dot(controller.Velocity, controller.GroundedNormal) * controller.GroundedNormal;
            }

            controller.Velocity += gravity * Time.deltaTime * gravityDirection;
        }

        public void SetGravityDirection(Vector3 direction)
        {
            gravityDirection = direction.normalized;
        }
    }
}