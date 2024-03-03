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

            Debug.DrawLine(transform.position, transform.position + controller.Velocity, Color.red, 1);
            if (controller.IsGrounded)
            {
                //hmm
            }

            controller.Velocity += gravity * Time.deltaTime * gravityDirection;
        }

        public void SetGravityDirection(Vector3 direction)
        {
            gravityDirection = direction.normalized;
        }
    }
}