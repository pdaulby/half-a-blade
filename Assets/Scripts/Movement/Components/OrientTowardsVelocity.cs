using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
public class OrientTowardsVelocity : ControllerComponent
{
    public float turnSpeed = 1;
    
    public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (controller.Velocity.magnitude < 0.5f) return;
            //todo ACUTE turns
            Quaternion lookRotation = Quaternion.LookRotation(controller.Velocity, controller.IsGrounded ? controller.GroundedNormal : controller.orientation.up);
            controller.orientation.rotation = Quaternion.Slerp(controller.orientation.rotation, lookRotation, turnSpeed * Time.deltaTime);
             
        }
}
}
