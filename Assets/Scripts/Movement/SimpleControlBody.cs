using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class SimpleControlBody : IControlBody
    {
        public Transform orientation;
        public new Collider collider = default;

        public float skinWidth = 0.2f;

        public Vector3 overlapCenter = new Vector3();
        public float overlapRadius = 1f;

        private Vector3 m_upDirection { get { return orientation.up; } }

        // Maximum number of detectable collisions.
        // If collisions are missing, reduce the overlap radius!
        private readonly Collider[] m_overlaps = new Collider[20];

        private const int MaxSweepSteps = 5;


        private Rigidbody rb;
        public bool interpolate = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            if (interpolate) rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public override void Move(Vector3 translation)
        {
            Vector3 startPosition = rb.position;
            isGrounded = false;
            CapsuleSweep(translation.normalized, translation.magnitude);
            Depenetrate();
            MoveWithInterpolation(startPosition);
            actualVelocity = (rb.position - startPosition) / Time.deltaTime;
        }

        public override void ResetCollider()
        {
            collider.enabled = false;
            collider.enabled = true;
            Depenetrate();
        }


        private void CapsuleSweep(Vector3 direction, float remainingDistance)
        {
            Vector3 initialDirection = direction;

            for (int i = 0; i < MaxSweepSteps; i++)
            {
                // Already travelled all the distance, no need to continue sweeping
                if (remainingDistance <= 0)
                    break;

                _ = rb.SweepTest(direction.normalized, out RaycastHit hitInfo, remainingDistance + skinWidth, QueryTriggerInteraction.Ignore);

                float lastIterationDistance = IterateMovement(
                    hitInfo,
                    initialDirection,
                    ref direction,
                    ref remainingDistance);
            }
        }

        private float IterateMovement(
            RaycastHit hitInfo,
            Vector3 initialDirection,
            ref Vector3 direction,
            ref float distance)
        {
            // How much it is possible to move without hitting the next collider (if next collider even exists)
            float safeDistance = distance;
            if (hitInfo.collider != null)
            {
                isGrounded = true;
                groundedNormal = hitInfo.normal;
                safeDistance = Mathf.Clamp(hitInfo.distance - skinWidth, 0, distance);
            }

            // Move the safe distance
            if (safeDistance > 0)
            {
                rb.position += direction * safeDistance;
                distance -= safeDistance;
            }

            if (hitInfo.collider != null)
                direction = Vector3.ProjectOnPlane(direction, hitInfo.normal);
            
            return safeDistance;
        }

        private void MoveWithInterpolation(Vector3 startPosition)
        {
            if (!interpolate) return;
            // Resetting position and using Move Position so interpolation is respected
            Vector3 m_position = rb.position;
            rb.position = startPosition;
            rb.MovePosition(m_position); 
            // Causes some strange behaviour - nested objects getting out of sync
            // Might be due to nested rigidbodies
        }

        private void Depenetrate()
        {
            // The faster the character, the higher the overlap radius should be!
            int overlapsNum = Physics.OverlapSphereNonAlloc(
                transform.position + overlapCenter,
                overlapRadius,
                m_overlaps,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < overlapsNum; i++)
            {
                var otherCollider = m_overlaps[i];

                if (otherCollider == collider)
                    continue; // skip ourself

                if (Physics.GetIgnoreLayerCollision(gameObject.layer, otherCollider.gameObject.layer))
                    continue;

                Vector3 otherPosition = otherCollider.gameObject.transform.position;
                Quaternion otherRotation = otherCollider.gameObject.transform.rotation;

                int iterations = 0;
                while (
                    Physics.ComputePenetration(
                        collider,
                        rb.position + collider.transform.localPosition,
                        rb.rotation * collider.transform.localRotation,
                        otherCollider,
                        otherPosition,
                        otherRotation,
                        out Vector3 direction,
                        out float distance)
                    )
                {
                    iterations++;
                    if (iterations > 10)
                    {
                        Debug.LogWarning("Unable to depenetrate");
                        break;
                    }
                    rb.position += direction * (distance + skinWidth);
                }
            }
        }

        public bool SetYScalePosition(float newYScale, float newYPosition, bool force = false)
        {
            bool didDownHit = rb.SweepTest(-orientation.up, out RaycastHit downHit);

            bool didUpHit = rb.SweepTest(orientation.up, out RaycastHit upHit);

            Vector3 oldboundMin = collider.bounds.min;

            Vector3 oldPos = collider.transform.localPosition;
            Vector3 oldScale = collider.transform.localScale;
            Vector3 oldPosWorld = rb.position;

            collider.transform.localScale = new Vector3(oldScale.x, newYScale, oldScale.z);
            collider.transform.localPosition = new Vector3(oldPos.x, newYPosition, oldPos.z);

            //Force bounding box to update *this frame*
            collider.enabled = false;
            collider.enabled = true;

            Vector3 newBoundMin = collider.bounds.min;

            if (isGrounded)
            {
                float yDiff = newBoundMin.y - oldboundMin.y;
                rb.position = rb.position - new Vector3(0, yDiff, 0);
                //Debug.Log("Adjusting Y scale and fixing floating character: " + yDiff);
            }
            else if (didDownHit && downHit.point.y > newBoundMin.y) // Intersecting ground!
            {
                float yDiff = downHit.point.y - newBoundMin.y;
                rb.position = rb.position + new Vector3(0, yDiff, 0);
                //Debug.Log("Adjusting Y scale and fixing ground intersection: " + yDiff);
            }
            else
            {
                //Debug.Log("Adjusting Y scale without adjusting position");
            }

            //Force bounding box to update *this frame*
            collider.enabled = false;
            collider.enabled = true;

            Vector3 newBoundMax = collider.bounds.max;

            if (!force && didUpHit && newBoundMax.y > upHit.point.y)
            {
                //Abort
                collider.transform.localScale = oldScale;
                collider.transform.localPosition = oldPos;
                rb.position = oldPosWorld;
                collider.enabled = false;
                collider.enabled = true;
                return false;
            }

            return true;
        }
    }

}