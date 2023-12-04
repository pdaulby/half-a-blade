using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public Transform orientation = default;
        public KinematicBody3 characterController = default;

        private InputReader inputReader = new();
        public FixedInput input { get; private set; } = new();

        void Update()
        {
            inputReader.Update();
        }

        void FixedUpdate()
        {
            input = inputReader.GetNext();
        }
    }
}