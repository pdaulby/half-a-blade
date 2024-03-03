using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
public class PushForwards : ControllerComponent
{
    public AnimationCurve pushStrength;

    public float frequency = 1;

    private float lastPush = 0;
    
    
     public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (input.Forward < 0.5) return;
            if (lastPush + frequency > Time.time) return;
            lastPush = Time.time;
            float x = pushStrength.Evaluate(controller.Velocity.magnitude);
            
            controller.Velocity += controller.orientation.forward * x;
        }
}
}
