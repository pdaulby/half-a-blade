using Assets.Scripts.Movement.Inputs;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class AirGravity : ControllerComponent
    {
        public float gravity = 9.8f;

        private Vector3 gravityDirection = Vector3.down;

        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (controller.IsGrounded)
            {
                controller.Velocity = new(controller.Velocity.x, 0, controller.Velocity.z);
                // ActualVelocity
                //When grounded, take the Vector3.Project() of the intended velocity against gravity direction, and of actual velocity against gravity direction. 
                //Then take intended velocity -intendedProjected + actualProjected, and make that the actual velocity
            }

            controller.Velocity += gravity * Time.deltaTime * gravityDirection;
            //TODO move jumping el mao
        }

        public void SetGravityDirection(Vector3 direction)
        {
            gravityDirection = direction.normalized;
        }
    }
}