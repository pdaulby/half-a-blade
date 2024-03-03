using Assets.Scripts.Movement.Inputs;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class GroundedCollisionSmoothing : ControllerComponent
    {
        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (controller.IsGrounded)
            {
                //TODO only if its upwards, not downwards
                controller.Velocity -= Vector3.Dot(controller.Velocity, controller.GroundedNormal) * controller.GroundedNormal;
                //controller.Velocity = Vector3.ProjectOnPlane(controller.Velocity, controller.GroundedNormal);
            }
        }
    }
}