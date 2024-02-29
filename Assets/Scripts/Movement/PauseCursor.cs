using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class PauseCursor : MonoBehaviour
    {
        void Update()
        {
            if (Pause.paused)
                return; 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}