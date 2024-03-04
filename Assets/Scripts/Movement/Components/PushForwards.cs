using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
public class PushForwards : ControllerComponent
{
    public float max = 14;
    public float pushStrength = 2;

    public float frequency = 1;

    private float lastPush = 0;
    
    
     public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (input.Forward < 0.5) return;
            if (lastPush + frequency > Time.time) return;
            if (controller.Velocity.magnitude > max) return;
            lastPush = Time.time;
            
            controller.Velocity += controller.orientation.forward * pushStrength;
        }
}
}
