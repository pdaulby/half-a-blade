using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class Jump : ControllerComponent
    {
        public float jump = 6;

        public override void DoUpdate(FixedInput input, PlayerController playerController)
        {
            if (!input.Jump.Pressed) return;

            if (playerController.IsGrounded)
            {
                playerController.Velocity += playerController.transform.up * jump;
            }
            //todo slope limit
            //todo use proper up
            //todo implement coyote time 
        }
    }
}