using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class InstantMovement : ControllerComponent
    {
        public float Speed = 8;

        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (input.Forward == 0 && input.Right == 0) return;
            Vector3 horz = (controller.orientation.forward * input.Forward + controller.orientation.right * input.Right) * Speed;

            controller.Velocity = new Vector3(horz.x, controller.Velocity.y, horz.z);
            //todo handle rotated up uh oh
        }
    }
}