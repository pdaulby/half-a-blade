using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class KinematicBodyMath : MonoBehaviour
    {

        public static Vector3 SlopeNormalToDownSlope(Vector3 slopeNormal, Vector3 up)
        {
            return Vector3.RotateTowards(slopeNormal, -up, Mathf.PI / 2, 0);
        }

        public static Vector3 RelativeSlopeNormal(Vector3 horizontal, Vector3 slopeNormal, Vector3 up)
        {
            Vector3 slopeVector = HorizontalToSlopeGradient(horizontal, slopeNormal, up);
            return Vector3.SlerpUnclamped(slopeVector, up, 90 / Vector3.Angle(slopeVector, up)).normalized;
        }

        // Private - not sure if a custom up vector works! Let's see :)
        private static Vector3 HorizontalToSlopeGradient(
            Vector3 horizontal,
            Vector3 slopeNormal,
            Vector3 up
        )
        {
            Vector3 straight = slopeNormal.Horizontal();

            // Assumes collision is going into slope!
            float angleToStraight = Vector3.Angle(-horizontal, straight);

            float straightLength = Mathf.Cos(Mathf.Deg2Rad * angleToStraight) * horizontal.magnitude;

            float angleToUp = Vector3.Angle(slopeNormal, up);

            if (angleToUp > 90)
                Debug.LogError("Trying to calculate slope gradient for a ceiling!");

            float complentaryAngleToUp = 90 - angleToUp;
            float height = straightLength / Mathf.Tan(Mathf.Deg2Rad * complentaryAngleToUp);

            //Vector up slope!
            return new Vector3(horizontal.x, height, horizontal.z);
        }
    }
}