using Assets.Scripts.Movement.Inputs;
using System;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public class OrientTowardsVelocity : ControllerComponent
    {
        public float turnSpeed = 1;

        [SerializeField] public OrientUp OrientUp;

        public override void DoUpdate(FixedInput input, PlayerController controller)
        {
            if (controller.Velocity.magnitude < 0.5f) return; //TODO handle this better
            //todo ACUTE turns
            Quaternion lookRotation = Quaternion.LookRotation(controller.Velocity, orientUpDirection(controller));
            controller.orientation.rotation = Quaternion.Slerp(controller.orientation.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }

        private Vector3 orientUpDirection(PlayerController controller)
        {
            return OrientUp switch
            {
                OrientUp.GRAVITY => -controller.GravityDirection,
                OrientUp.CURRENT_UP => controller.orientation.up,
                OrientUp.GROUND_NORMAL => controller.GroundedNormal,
                _ => throw new Exception("not implemented"),
            };
        }
    }

    public enum OrientUp
    {
        GRAVITY,
        CURRENT_UP,
        GROUND_NORMAL
    }
}
