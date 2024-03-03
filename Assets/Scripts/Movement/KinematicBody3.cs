using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public class KinematicBody3 : MonoBehaviour
    {
        public Transform orientation;
        public new Collider collider = default;
        public float slopeLimit = 45f;
        public float stepOffset = 0.3f;

        // Currently using a naive implementation for floor snapping
        // For some very specific gradients of stairs / slopes, and player speeds
        // The behaviour may be inconsistent
        public float snapToFloorOffset = 0;
        public float skinWidth = 0.2f;

        public Vector3 overlapCenter = new Vector3();
        public float overlapRadius = 1f;

        private Vector3 m_upDirection { get { return orientation.up; } }

        // Maximum number of detectable collisions.
        // If collisions are missing, reduce the overlap radius!
        private readonly Collider[] m_overlaps = new Collider[20];

        private const int MaxSweepSteps = 5;
        private const float MinMoveDistance = 0f;

        private Vector3 _translation = default;
        public bool isGrounded { get; private set; }
        public Vector3 groundedNormal { get; private set; }

        // Because of the setting implementation:
        // * Only the most recent call to Move will be accounted for (i.e. only call it once per frame!)
        // * deltaTime will be accurate for Update and FixedUpdate but *not* custom update loops (rare but I did it for Ballistic Zen :) )
        public Vector3 velocity { get; private set; }
        public bool stepped { get; private set; }
        private Vector3 stepTranslation;

        public bool isOnSlope { get; private set; }
        public Vector3 slopeNormal { get; private set; }
        public float slopeAngle { get; private set; }

        private Rigidbody rb;

        public enum SweepType
        {
            LATERAL,
            VERTICAL
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            InitializeRigidbody();
        }

        private void InitializeRigidbody()
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void ResetCollider()
        {
            collider.enabled = false;
            collider.enabled = true;
            Depenetrate();
        }

        public void Move(Vector3 translation)
        {
            Vector3 startPosition;

            //Initialise variables
            
            startPosition = rb.position;
            //m_upDirection = transform.up;
            _translation = translation;
            stepTranslation = Vector3.zero;
            isGrounded = false;
            isOnSlope = false;
            stepped = false;

            // If we are not rising, we can set grounded at the start which is useful in CapsuleSweep
            // (in addition, we set grounded based on all collisions post movement step)
            // (not sure that part is necessary or helpful!)
            if (_translation.y <= 0)
            {
                if (rb.SweepTest(Vector3.down, out RaycastHit hit, skinWidth + 0.01f, QueryTriggerInteraction.Ignore))
                {
                    SetGroundedAndSlopeState(hit.normal);
                }
            }
            

            //Collide and Slide
            if (_translation.sqrMagnitude > MinMoveDistance)
            {
                Vector3 localTranslation = transform.InverseTransformDirection(_translation);
                Vector3 lateralTranslation = new Vector3(localTranslation.x, 0, localTranslation.z);
                Vector3 verticalTranslation = new Vector3(0, localTranslation.y, 0);

                lateralTranslation = transform.TransformDirection(lateralTranslation);
                verticalTranslation = transform.TransformDirection(verticalTranslation);

                CapsuleSweep(SweepType.LATERAL, lateralTranslation.normalized, lateralTranslation.magnitude);

                CapsuleSweep(SweepType.VERTICAL, verticalTranslation.normalized, verticalTranslation.magnitude);
            }

            Depenetrate();

            // Move
            // Resetting position and using Move Position so interpolation is respected
            Vector3 m_position = rb.position;
            rb.position = startPosition;
            _translation = m_position - startPosition;
            _translation -= stepTranslation;
            velocity = _translation / Time.deltaTime;
            // Causes some strange behaviour - nested objects getting out of sync
            // Might be due to nested rigidbodies
            // rb.MovePosition(m_position);
            rb.position = m_position;
        }

        public void Depenetrate()
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
                // Unity docs says penetration is one and done:
                //    https://docs.unity3d.com/ScriptReference/Physics.ComputePenetration.html
                // Some random issue response *implies* it should be used iteratively:
                //    https://issuetracker.unity3d.com/issues/physics-dot-computepenetration-returns-incorrect-distance-and-direction-when-box-collider-with-a-non-convex-mesh-collider-is-used
                // So yolo we're using it iteratively
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

        private void SetGroundedAndSlopeState(Vector3 contactNormal)
        {
            slopeAngle = Vector3.Angle(m_upDirection, contactNormal);

            if (slopeAngle <= slopeLimit)
            {
                isGrounded = true;
                groundedNormal = contactNormal;
            }
            else if (slopeAngle >= slopeLimit && slopeAngle < 90)
            {
                isOnSlope = true;
                slopeNormal = contactNormal;
            }
        }

        public void CapsuleSweep(SweepType sweepType, Vector3 direction, float remainingDistance)
        {
            Vector3 initialDirection = direction;
            RaycastHit stepRaisedHitInfo = default;
            RaycastHit lastUsedHitInfo = default;
            Vector3 preSweepPositionGround;
            Vector3 preSweepPositionStep = default;
            bool collision;
            bool stepRaisedCollision;
            bool stepDownCollision;

            //For Vertical, blocking angle is between 0 - slopeLimit
            //For Lateral, blocking angle is slopeLimit - 90 (grounded) or 360 (not grounded)
            float minBlockAngle = sweepType == SweepType.LATERAL ? slopeLimit : 0;
            float maxBlockAngle = slopeLimit;
            if (sweepType == SweepType.LATERAL)
                maxBlockAngle = isGrounded ? 360 : 90;

            for (int i = 0; i < MaxSweepSteps; i++)
            {
                // Already travelled all the distance, no need to continue sweeping
                if (remainingDistance <= 0)
                    break;

                preSweepPositionGround = rb.position;
                collision = rb.SweepTest(direction.normalized, out RaycastHit hitInfo, remainingDistance + skinWidth, QueryTriggerInteraction.Ignore);

                // The Step algorithm nudges the character up; let's the character move; then nudes them back down
                // It only makes sense for lateral movement
                // Realistically it's a half implemented 1 iteration version of IterateMovement; so it should be there.
                // But I can't be bothered to reason about it now
                bool doStep = false;
                if (sweepType == SweepType.LATERAL && stepOffset > 0)
                {
                    float nudgeUpDistance = stepOffset;

                    // How far up can we nudge the character? (is there a ceiling blocking upward nudging)
                    rb.SweepTest(
                        m_upDirection,
                        out RaycastHit nudgeUpHitInfo,
                        stepOffset,
                        QueryTriggerInteraction.Ignore
                    );
                    if (nudgeUpHitInfo.collider != null)
                        nudgeUpDistance = nudgeUpHitInfo.distance - skinWidth;

                    rb.position += Vector3.up * nudgeUpDistance;
                    preSweepPositionStep = rb.position;

                    // While raised, do we collide with anything?
                    stepRaisedCollision = rb.SweepTest(
                        direction.normalized,
                        out stepRaisedHitInfo,
                        remainingDistance * 2,
                        QueryTriggerInteraction.Ignore
                    );

                    rb.position += direction.normalized * remainingDistance;
                    stepDownCollision = rb.SweepTest(-m_upDirection, out RaycastHit stepDownHitInfo, stepOffset + skinWidth, QueryTriggerInteraction.Ignore);

                    bool stepRaisedAvoidedCollision = !stepRaisedCollision && collision;
                    bool stepCollidedFurther = stepRaisedCollision
                        && collision
                        && stepRaisedHitInfo.distance > hitInfo.distance + 0.001;
                    bool stepRaisedHitSteepSlope = stepRaisedCollision
                        && Vector3.Angle(m_upDirection, stepRaisedHitInfo.normal) >= slopeLimit;
                    bool stepDownHitSteepSlope =  stepDownCollision
                        && Vector3.Angle(m_upDirection, stepDownHitInfo.normal) >= slopeLimit;
                    bool walkHitShallowSlope =
                        collision && Vector3.Angle(m_upDirection, hitInfo.normal) < slopeLimit;
                    bool stepWentHigher = stepDownCollision && stepDownHitInfo.distance < nudgeUpDistance;

                    doStep =
                        !walkHitShallowSlope
                        && !stepRaisedHitSteepSlope
                        && stepWentHigher
                        && !stepDownHitSteepSlope
                        && (stepRaisedAvoidedCollision || stepCollidedFurther);

                    if (doStep)
                    {
                        stepTranslation += Vector3.up * stepOffset;
                        stepped = true;
                    }
                }

                // Do the movement
                RaycastHit iterationUsingHitInfo = doStep ? stepRaisedHitInfo : hitInfo;
                rb.position = doStep ? preSweepPositionStep : preSweepPositionGround; // If stepping it's already in the right place
                float lastIterationDistance = IterateMovement(
                    sweepType,
                    iterationUsingHitInfo,
                    lastUsedHitInfo,
                    initialDirection,
                    ref direction,
                    ref remainingDistance,
                    minBlockAngle,
                    maxBlockAngle
                );
                lastUsedHitInfo = iterationUsingHitInfo;
                
                //Clamp rigibody back down if necessary
                float downDistance = 0;

                if (isGrounded && !isOnSlope)
                {
                    downDistance += snapToFloorOffset;
                }

                if (doStep)
                {
                    downDistance += stepOffset;
                }

                if (downDistance > 0)
                {
                    if (rb.SweepTest(Vector3.down,  out RaycastHit clampDownHitInfo, downDistance, QueryTriggerInteraction.Ignore)
                        // Don't clamp if steep slope!
                        && Vector3.Angle(m_upDirection, clampDownHitInfo.normal) < slopeLimit)
                    {
                        downDistance = Mathf.Max(0, clampDownHitInfo.distance - skinWidth);
                        rb.position += Vector3.down * downDistance;

                        stepTranslation += Vector3.down * downDistance;
                    }
                    else
                    {
                        //This should be rare:
                        //  When stepping entirely over something
                        //  When clamping to a ramp that ends in a drop
                    }
                }
            }
        }

        private float IterateMovement(
            SweepType sweepType,
            RaycastHit hitInfo,
            RaycastHit lastUsedHitInfo,
            Vector3 initialDirection,
            ref Vector3 direction,
            ref float distance,
            float minBlockAngle,
            float maxBlockAngle)
        {
            // How much it is possible to move without hitting the next collider (if next collider even exists)
            float safeDistance = distance;
            if (hitInfo.collider != null)
            {
                safeDistance = Mathf.Clamp(hitInfo.distance - skinWidth, 0, distance);
                SetGroundedAndSlopeState(hitInfo.normal);
            }

            // Move the safe distance
            if (safeDistance > 0)
            {
                rb.position += direction * safeDistance;
                distance -= safeDistance;
            }

            if (hitInfo.collider == null)
                return safeDistance;
            
            // If there was a next collider; figure out what the next direction will be
            // Default: project direction straight into the plane of the hit surface
            Vector3 projectionNormal = hitInfo.normal;

            float surfaceAngle = Vector3.Angle(m_upDirection, hitInfo.normal) - 0.001f;

            // If hitting a slope actually use the plane adjusted for the players direction
            // (i.e. force the final direction to align with players input direction)
            if (sweepType == SweepType.LATERAL && surfaceAngle < 90)
            {
                projectionNormal = KinematicBodyMath.RelativeSlopeNormal(direction.Horizontal(), hitInfo.normal);
            }

            // Do even more fancy things if it's a "blocking" surface
            if ((surfaceAngle >= minBlockAngle) && (surfaceAngle <= maxBlockAngle))
            {
                if (sweepType == SweepType.LATERAL)
                {
                    // Default for blocking slope is to "wall off" along the slope
                    projectionNormal = new Vector3(hitInfo.normal.x, 0, hitInfo.normal.z);

                    // Ground / Slope case: figure out the plane lines up the direction
                    // with the seam between ground and slope
                    if (isGrounded
                        && Vector3.Angle(hitInfo.normal, Vector3.up) < 90
                        // If same normal is hit twice, the seam algorithm is stuck
                        // So try to just default projectionNormal
                        // (happens at the top of slopes)
                        && (lastUsedHitInfo.collider == null
                            || lastUsedHitInfo.normal != hitInfo.normal))
                    {
                        var seam = Vector3.Cross(hitInfo.normal, groundedNormal);
                        projectionNormal = new Plane(Vector3.zero, seam, seam + Vector3.up).normal;
                    }
                    // Ceiling / Ground squeeze case: figure out the plane that lines up the direction
                    // Out of a ground and ceiling that are squeezing together
                    else if (isGrounded
                        && Vector3.Angle(hitInfo.normal, Vector3.up) > 90
                        // Same as above; I'm less confident about how this happens
                        // but it seemed to smooth things out
                        && (lastUsedHitInfo.collider == null
                            || lastUsedHitInfo.normal != hitInfo.normal))
                    {
                        // Experimental algorithm: I noticed that when the floor is flat
                        // Squeezing ceilings work correctly
                        // So when the floor is not flat, figure out what rotation would make it so,
                        // Apply that to the ceiling; work out the appropriate plane,
                        // Then rotate it back.
                        // I'm not totally confident in it
                        var rotation = Quaternion.FromToRotation(groundedNormal, Vector3.up);
                        var rotatedHitNormal = rotation * hitInfo.normal;
                        projectionNormal = new Vector3(rotatedHitNormal.x, 0, rotatedHitNormal.z);
                        projectionNormal = Quaternion.Inverse(rotation) * projectionNormal;
                    }
                }
                // For vertical sweeps, just block any downward movement when on a slope
                if (sweepType == SweepType.VERTICAL)
                {
                    projectionNormal = Vector3.up;
                }
            }

            /**
            * A bit confusing, I think!
            * 
            * For some situations such as climbing a ramp while also sort of strafing into a wall, it is important
            * that the 'collide and slide' off one follows the 'collide and slide' off the other. 
            * 
            * In some situations, such as running into a corner between 90-180 degrees, the collide and slides should converge
            * into each other.
            */
            var continueDirection = Vector3.ProjectOnPlane(direction, projectionNormal);
            var initialInfluenceDirection = Vector3.ProjectOnPlane(initialDirection, projectionNormal);

            direction = Vector3.Dot(continueDirection, initialDirection) < 0
                    ? initialInfluenceDirection
                    : continueDirection;

            return safeDistance;
        }

        public bool SetYScalePosition(float newYScale, float newYPosition, bool force = false)
        {
            bool didDownHit = rb.SweepTest(Vector3.down, out RaycastHit downHit);

            bool didUpHit = rb.SweepTest(Vector3.up, out RaycastHit upHit);

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