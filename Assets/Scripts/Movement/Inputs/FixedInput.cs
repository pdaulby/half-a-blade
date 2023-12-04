using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Inputs
{
    public struct FixedInput
    {
        public float Forward;
        public float Right;
        public Pressable Jump;

        internal FixedInput Next()
        {
            return new FixedInput
            {
                Forward = Forward,
                Right = Right,
                Jump = Jump.Next()
            };
        }
        // handle other pressables
        // handle look for surf 
    }
}