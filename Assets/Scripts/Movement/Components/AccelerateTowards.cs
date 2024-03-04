using Assets.Scripts.Movement.Components;
using Assets.Scripts.Movement.Inputs;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class AccelerateTowards : ControllerComponent
    {
        public float Speed = 8;
        public float Acceleration = 8;
        public float Decceleration = 8;
        public float DeccelerateCuttoff = 0.8f;

        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            var targetSpeed = Mathf.Max(controller.Velocity.magnitude, Speed);

            Vector3 horz = (controller.orientation.forward * input.Forward + controller.orientation.right * input.Right) * targetSpeed;

            controller.Velocity = new Vector3(horz.x, controller.Velocity.y, horz.z);
            //todo handle rotated up uh oh
        }
    }
}
