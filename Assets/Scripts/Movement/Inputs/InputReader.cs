using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Inputs
{
    public class InputReader // maybe should be monobehaviour
    {
        public FixedInput input = new();
                
        public void Update()
        {
            input.Jump.Held = Input.GetKey(KeyCode.Space);
            input.Jump.Press(Input.GetKeyDown(KeyCode.Space));
            input.Jump.Release(Input.GetKeyUp(KeyCode.Space));

            input.Forward = Input.GetAxisRaw("Vertical");
            input.Right = Input.GetAxisRaw("Horizontal");
        }
        
        public FixedInput GetNext()
        {
            var next = input;
            input = input.Next();
            return next;
        }
    }
}