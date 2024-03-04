using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
public class TiltOrientation : ControllerComponent
{
    public float turnSpeed = 2;

    public bool effectVelocity = false;
    
    public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            float tiltUpDown = turnSpeed * input.Forward;
            controller.orientation.Rotate(tiltUpDown, 0, 0);

            if (effectVelocity)
                controller.Velocity = Quaternion.AngleAxis(tiltUpDown, controller.orientation.right) * controller.Velocity;

        }
    }
}
