using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Inputs
{
    public class InputReader // maybe should be monobehaviour
    {
        private InputConfig config = new()
        {
            MaxZone = 0.9f,
            DeadZone = 0.2f
        };

        float forward;
        float right;
        Pressable jump = new();
                
        public void Update()
        {
            jump.Held = Input.GetKey(KeyCode.Space);
            jump.Press(Input.GetKeyDown(KeyCode.Space));
            jump.Release(Input.GetKeyUp(KeyCode.Space));

            forward = Input.GetAxisRaw("Vertical");
            right = Input.GetAxisRaw("Horizontal");
        }
        
        public FixedInput GetNext()
        {
            var next = new FixedInput(config, forward, right, jump) ;
            jump = jump.Next();
            return next;
        }
    }
}