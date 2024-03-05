using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public abstract class IControlBody: MonoBehaviour
    {
        public abstract void Move(Vector3 translation);
        public abstract void ResetCollider();
        public bool isGrounded { get; protected set; }
        public Vector3 groundedNormal { get; protected set; }
        public Vector3 actualVelocity { get; protected set; }
    }

}