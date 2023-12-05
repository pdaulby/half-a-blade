﻿using Assets.Scripts.Movement.Inputs;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement.Components
{
    public abstract class ControllerComponent : MonoBehaviour
    {
        public virtual bool CanUse(PlayerController playerController)
        {
            //TODO
            return true; 
        }

        public abstract void DoUpdate(FixedInput input, PlayerController playerController);
    }
}