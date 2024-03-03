using Assets.Scripts.Movement.Inputs;
using System;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class GroundedCollisionSmoothing : ControllerComponent
    {
        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (!controller.IsGrounded) throw new Exception("Only when grounded");
            if (Vector3.Dot(controller.Velocity, controller.GroundedNormal) < 0)
            {
                controller.Velocity -= Vector3.Dot(controller.Velocity, controller.GroundedNormal) * controller.GroundedNormal;
                //controller.Velocity = Vector3.ProjectOnPlane(controller.Velocity, controller.GroundedNormal);
            }
        }
    }
}