using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public abstract class ControllerComponent : MonoBehaviour
    {
        [Header("Active When")]
        [SerializeField] protected ActiveWhen Grounded;

        [DrawIf("Grounded", ActiveWhen.IsTrue, ComparisonType.Equals)]
        [SerializeField] protected float SlopeMin = 0; //inclusive
        [DrawIf("Grounded", ActiveWhen.IsTrue, ComparisonType.Equals)]
        [SerializeField] private float SlopeMax = 90; //exclusive

        public virtual bool CanUse(PlayerController controller)
        {
            if (!enabled) return false;

            if (Grounded == ActiveWhen.IsTrue && controller.IsGrounded)
            {
                if (controller.SlopeAngle < SlopeMin) return false;
                if (controller.SlopeAngle >= SlopeMax) return false;
            }
            if (!Grounded.Allows(controller.IsGrounded)) return false;
            
            return true; 
        }

        public abstract void DoUpdate(FixedInput input, PlayerController controller);
    }
}