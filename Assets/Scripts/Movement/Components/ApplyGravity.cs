using Assets.Scripts.Movement.Inputs;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class ApplyGravity : ControllerComponent
    {
        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            controller.Velocity += controller.Gravity * Time.deltaTime * controller.GravityDirection;
        }
    }
}