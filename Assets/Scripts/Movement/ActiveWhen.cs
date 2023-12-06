using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public enum ActiveWhen
    {
        IsEither,
        IsTrue,
        IsFalse
    }
    public static class ActiveWhenExtension
    {
        public static bool Allows(this ActiveWhen when, bool state)
        {
            switch (when)
            {
                case ActiveWhen.IsEither:
                    return true;
                case ActiveWhen.IsTrue:
                    return state;
                case ActiveWhen.IsFalse:
                    return !state;
                default: throw new ArgumentOutOfRangeException("when is an invalid value");
            }
        }
    }
}