using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
public class TurnOrientation : ControllerComponent
{
    public float turnSpeed = 2;
    public bool turnVelocity = true;
    
    public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            float turnLeftRight = turnSpeed * input.Right;
            controller.orientation.Rotate(0, turnLeftRight, 0);

            if (!turnVelocity) return;
            
            controller.Velocity = Quaternion.AngleAxis(turnLeftRight, controller.orientation.up) * controller.Velocity;
            
        }
}
}
