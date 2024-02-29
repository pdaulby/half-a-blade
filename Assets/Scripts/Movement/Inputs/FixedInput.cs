using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Inputs
{
    public readonly struct FixedInput
    {
        public readonly Vector2 MoveVector;
        public readonly Pressable Jump;

        public FixedInput(InputConfig ic, float forward, float right, Pressable jump)
        {
            MoveVector = new Vector2(forward, right);
            if (MoveVector.magnitude >= ic.MaxZone) MoveVector = MoveVector.normalized;
            else if (MoveVector.magnitude <= ic.DeadZone) MoveVector = Vector2.zero;

            Jump = jump;
        }

        public float Forward { get => MoveVector.x; }
        public float Right { get => MoveVector.y; }
        // handle other pressables
        // handle look for surf 
    }
}