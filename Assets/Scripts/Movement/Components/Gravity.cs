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
                //controller.Velocity = new(controller.Velocity.x, 0, controller.Velocity.z);
                var c1 = Vector3.Cross(controller.GroundedNormal, -controller.Velocity);
                var tangent = Vector3.Cross(controller.GroundedNormal, c1).normalized;
                var dot = Vector3.Dot(controller.Velocity.normalized, tangent);
                Debug.DrawRay(transform.position, c1.normalized, Color.red, 1);
                Debug.DrawRay(transform.position, tangent*controller.Velocity.magnitude, Color.green, 1);
                Debug.Log(dot);
                Vector3 landedDirection = controller.Velocity.magnitude * dot * tangent;
                controller.Velocity = landedDirection;
                //ActualVelocity
                //When grounded, take the Vector3.Project() of the intended velocity against gravity direction, and of actual velocity against gravity direction. 
                //Then take intended velocity -intendedProjected + actualProjected, and make that the actual velocity
            }

            controller.Velocity += gravity * Time.deltaTime * gravityDirection;
        }

        public void SetGravityDirection(Vector3 direction)
        {
            gravityDirection = direction.normalized;
        }
    }
}