using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Inputs
{
    public struct Pressable 
    {
        public bool Held {  get; set; }
        public bool Pressed { get; private set; }
        public bool Released { get; private set; }

        public Pressable Next()
        {
            return new Pressable { Held = Held };
        }
        public void Press(bool pressed)
        {
            if (pressed) Pressed = true;
        }
        public void Release(bool released)
        {
            if (released) Released = true;
        }
    }
}