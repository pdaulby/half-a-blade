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

        private Vector3 _translation = default;
        public bool isGrounded { get; private set; }
        public Vector3 groundedNormal { get; private set; }

        // Because of the setting implementation:
        // * Only the most recent call to Move will be accounted for (i.e. only call it once per frame!)
        // * deltaTime will be accurate for Update and FixedUpdate but *not* custom update loops (rare but I did it for Ballistic Zen :) )
        public Vector3 velocity { get; private set; }
        public bool stepped { get; private set; }
        private Vector3 stepTranslation;

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
            //Initialise variables
            Vector3 startPosition = rb.position;
            _translation = translation;
            stepTranslation = Vector3.zero;
            isGrounded = false;
            stepped = false;

            //Collide and Slide
            if (translation != Vector3.zero)
            {
                Vector3 localTranslation = orientation.InverseTransformDirection(_translation);
                Vector3 lateralTranslation = new Vector3(localTranslation.x, 0, localTranslation.z);
                Vector3 verticalTranslation = new Vector3(0, localTranslation.y, 0);

                lateralTranslation = orientation.TransformDirection(lateralTranslation);
                verticalTranslation = orientation.TransformDirection(verticalTranslation);

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
            rb.MovePosition(m_position);
            //rb.position = m_position;
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
            else
                Debug.Log($"other, angle {slopeAngle}, limit {slopeLimit}");
        }

        public void CapsuleSweep(SweepType sweepType, Vector3 direction, float remainingDistance)
        {
            Vector3 initialDirection = direction;
            RaycastHit stepRaisedHitInfo = default;
            Vector3 preSweepPositionGround;
            Vector3 preSweepPositionStep = default;
            bool collision;
            bool stepRaisedCollision;
            bool stepDownCollision;


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

                    rb.position += orientation.up * nudgeUpDistance;
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

                    //Debug.Log($" {!stepRaisedHitSteepSlope}  {!stepDownHitSteepSlope} {stepRaisedAvoidedCollision} {stepCollidedFurther}");

                    doStep = true 
                        && !walkHitShallowSlope
                        && !stepRaisedHitSteepSlope
                        && stepWentHigher
                        && !stepDownHitSteepSlope
                        && (stepRaisedAvoidedCollision || stepCollidedFurther);

                    if (doStep)
                    {
                        Debug.Log("wow");
                        stepTranslation += orientation.up * stepOffset;
                        stepped = true;
                    }
                }

                // Do the movement
                RaycastHit iterationUsingHitInfo = doStep ? stepRaisedHitInfo : hitInfo;
                rb.position = doStep ? preSweepPositionStep : preSweepPositionGround; // If stepping it's already in the right place
                float lastIterationDistance = IterateMovement(
                    iterationUsingHitInfo,
                    initialDirection,
                    ref direction,
                    ref remainingDistance);
                
                //Clamp rigibody back down if necessary
                float downDistance = 0;

                if (isGrounded)
                {
                    downDistance += snapToFloorOffset;
                }

                if (doStep)
                {
                    downDistance += stepOffset;
                }

                if (downDistance > 0)
                {
                    if (rb.SweepTest(-orientation.up,  out RaycastHit clampDownHitInfo, downDistance, QueryTriggerInteraction.Ignore)
                        // Don't clamp if steep slope!
                        && Vector3.Angle(m_upDirection, clampDownHitInfo.normal) < slopeLimit)
                    {
                        downDistance = Mathf.Max(0, clampDownHitInfo.distance - skinWidth);
                        rb.position += -orientation.up * downDistance;

                        stepTranslation += -orientation.up * downDistance;
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
            RaycastHit hitInfo,
            Vector3 initialDirection,
            ref Vector3 direction,
            ref float distance)
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