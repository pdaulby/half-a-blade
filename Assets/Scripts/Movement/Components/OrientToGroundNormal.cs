using Assets.Scripts.Movement.Inputs;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
public class OrientToGroundNormal : ControllerComponent
{
    public float turnSpeed = 1;
    
    public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (!controller.IsGrounded) throw new Exception("MustBeGrounded");

            var proj = Vector3.ProjectOnPlane(controller.orientation.forward, controller.GroundedNormal);
            if (proj == Vector3.zero) return;
            Debug.DrawLine(transform.position, transform.position + proj, Color.green, 1);
            
            Quaternion lookRotation = Quaternion.LookRotation(proj);
            controller.orientation.rotation = Quaternion.Slerp(controller.orientation.rotation, lookRotation, turnSpeed * Time.deltaTime);
             
        }
}
}
