using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// Based from this repo: https://github.com/Kevin-Kwan/Unity3D-FishingRodMotion?tab=readme-ov-file
// Inspiration from https://www.reddit.com/r/Unity3D/comments/1kx1hp/best_way_to_do_fishing_line/
public class VerletFishingLine : MonoBehaviour
{
    [Header("Line Settings")]
    public Transform StartPoint;
    public Transform EndPoint;
    public LineRenderer lineRenderer;
    [Tooltip("When true, both rope ends are fixed to transforms. Disable for a dynamic hook end.")]
    public bool lockEndToTransform = false;

    // The length of the line will be Segments * SegmentLength, so adjust SegmentLength to change the total length of the line. You can also adjust SegmentLength at runtime to simulate casting and reeling in.
    public int Segments = 10;
    public float SegmentLength = 0.03f;

    // The initial length of the line when cast, and the max length when fully cast. Adjust these to change how far the player can cast the line, and how long the line is when fully cast. You can also adjust these at runtime to change casting distance.
    [Header("Casting Settings")]
    public float startSegmentLength = 0.03f;
    public float currentTargetLength = 0.03f;
    public float maxSegmentLength = 1f;

    [Header("Cast Flight")]
    [SerializeField] private LayerMask waterLayerMask;
    [SerializeField] private float castFlightSpeed = 18f;
    [SerializeField] private float castDistanceMin = 8f;
    [SerializeField] private float castDistanceMax = 12f;
    [SerializeField] private float castAngleRange = 18f;
    [SerializeField] private float waterSurfaceOffset = 0.03f;

    [Header("Water Bobbing")]
    [SerializeField] private float bobRadius = 0.35f;
    [SerializeField] private float bobMoveSpeed = 1.5f;
    [SerializeField] private float bobTargetArrivalThreshold = 0.05f;

    // line gravity and physics settings
    [Header("Physics Settings")]
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);
    // Num of Physics iterations
    public int Iterations = 6;
    // higher is stiffer, lower is stretchier
    public float tensionConstant = 10f;
    public bool SecondHasRigidbody = false;
    public float LerpSpeed = 1f;
    public float Delay = 2f;
    private bool isChangingLength = false;
    private bool isCastingToWater;
    private bool isBobbingOnWater;
    private Vector3 castTargetPosition;
    private Vector3 bobCenterPosition;
    private Vector3 bobTargetPosition;

    [Header("Equip State")]
    [SerializeField] private GameObject hookVisual;
    [SerializeField] private bool hideLineWhenUnequipped = true;
    [SerializeField] private bool hideHookWhenUnequipped = true;

    [Header("Debug / Prototyping")]
    [SerializeField] private bool useSimpleLine = false;
    [Tooltip("When true, draws a straight line for prototyping. When false, uses full Verlet physics.")]

    [Header("Fish Attach")]
    [SerializeField] private FishingManager fishingManager;
    [Tooltip("Child transform names checked on the active fish to find a mouth anchor.")]
    [SerializeField] private string[] fishMouthAnchorNames = { "Main1" };

    private bool isRodEquipped = true;
    private Transform attachedFishOnLine;
    private bool lockEndBeforeAttach;

    // Represents a segment of the line.
    private class LineParticle
    {
        public Vector3 Pos;
        public Vector3 OldPos;
        public Vector3 Acceleration;
    }

    private List<LineParticle> particles;
    // Initializes the line.
    void Start()
    {
        if (fishingManager == null)
        {
            fishingManager = FindFirstObjectByType<FishingManager>();
        }

        if (hookVisual == null && EndPoint != null)
        {
            hookVisual = EndPoint.gameObject;
        }

        ApplyEquipVisualState(isRodEquipped);

        particles = new List<LineParticle>();
        for (int i = 0; i < Segments; i++)
        {
            Vector3 point = Vector3.Lerp(StartPoint.position, EndPoint.position, i / (float)(Segments - 1));
            particles.Add(new LineParticle { Pos = point, OldPos = point, Acceleration = Gravity });
        }
        // The particles are simulated in world-space positions.
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.positionCount = particles.Count;
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += DetachFromFish;
        FishingManager.OnEscaped += DetachFromFish;
        FishingManager.OnReturnToIdle += DetachFromFish;
        FishingManager.OnCast += DetachFromFish;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= DetachFromFish;
        FishingManager.OnEscaped -= DetachFromFish;
        FishingManager.OnReturnToIdle -= DetachFromFish;
        FishingManager.OnCast -= DetachFromFish;
        StopCastMotion();
    }

    void Update()
    {
        if (!isRodEquipped)
        {
            return;
        }

        if (isChangingLength)
        {
            SegmentLength = Mathf.Lerp(SegmentLength, currentTargetLength, LerpSpeed * Time.deltaTime);

            // Stop changing the line length when it's close enough to the min
            if (Mathf.Abs(SegmentLength - currentTargetLength) < 0.01f)
            {
                SegmentLength = currentTargetLength;
                isChangingLength = false;
            }
        }

        if (isCastingToWater)
        {
            UpdateCastFlight();
        }
        else if (isBobbingOnWater)
        {
            UpdateWaterBob();
        }
    }

    void LateUpdate()
    {
        if (!isRodEquipped)
        {
            return;
        }

        if (attachedFishOnLine != null && EndPoint != null)
        {
            EndPoint.position = attachedFishOnLine.position;
        }
    }

    private IEnumerator IncreaseLengthAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentTargetLength = maxSegmentLength;
        isChangingLength = true;
    }

    // Update the line with Verlet Physics or simple straight line.
    void FixedUpdate()
    {
        if (!isRodEquipped)
        {
            return;
        }

        if (StartPoint == null || EndPoint == null || particles == null || particles.Count == 0)
        {
            return;
        }

        if (useSimpleLine)
        {
            // Simple straight line for prototyping
            var simpleLinePositions = new Vector3[2];
            simpleLinePositions[0] = StartPoint.position;
            simpleLinePositions[1] = EndPoint.position;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(simpleLinePositions);
            return;
        }

        // Full Verlet physics simulation
        foreach (var p in particles)
        {
            Verlet(p, Time.fixedDeltaTime);
        }

        particles[0].Pos = StartPoint.position;
        if (lockEndToTransform)
        {
            particles[particles.Count - 1].Pos = EndPoint.position;
        }
    
        for (int i = 0; i < Iterations; i++)
        {
            for (int j = 0; j < particles.Count - 1; j++)
            {
                PoleConstraint(particles[j], particles[j + 1], SegmentLength);
            }

            // Keep start anchor pinned every iteration to avoid cumulative drift while moving.
            particles[0].Pos = StartPoint.position;

            if (lockEndToTransform)
            {
                particles[particles.Count - 1].Pos = EndPoint.position;
            }
        }

        if (lockEndToTransform && SecondHasRigidbody)
        {
            Vector3 force = (particles[particles.Count - 1].Pos - EndPoint.position) * tensionConstant;
            EndPoint.GetComponent<Rigidbody>().AddForce(force);
        }
        else if (!lockEndToTransform)
        {
            Rigidbody endBody = EndPoint != null ? EndPoint.GetComponent<Rigidbody>() : null;
            if (endBody != null && !endBody.isKinematic)
            {
                endBody.MovePosition(particles[particles.Count - 1].Pos);
            }
            else if (EndPoint != null)
            {
                EndPoint.position = particles[particles.Count - 1].Pos;
            }
        }
    
        var positions = new Vector3[particles.Count];
        for (int i = 0; i < particles.Count; i++)
        {
            positions[i] = particles[i].Pos;
        }
        lineRenderer.SetPositions(positions);
    }

    private void HandleHooked()
    {
        StopCastMotion();
        TryAttachToActiveFishMouth();
    }

    private void TryAttachToActiveFishMouth()
    {
        if (fishingManager == null)
        {
            fishingManager = FindFirstObjectByType<FishingManager>();
        }

        if (fishingManager == null || fishingManager.activeFish == null || EndPoint == null)
        {
            return;
        }

        Transform fishRoot = fishingManager.activeFish.transform;
        Transform mouthAnchor = ResolveMouthAnchor(fishRoot);
        attachedFishOnLine = mouthAnchor != null ? mouthAnchor : fishRoot;

        lockEndBeforeAttach = lockEndToTransform;
        lockEndToTransform = true;
        EndPoint.position = attachedFishOnLine.position;
    }

    private Transform ResolveMouthAnchor(Transform fishRoot)
    {
        if (fishRoot == null)
        {
            return null;
        }

        if (fishMouthAnchorNames != null)
        {
            for (int i = 0; i < fishMouthAnchorNames.Length; i++)
            {
                string anchorName = fishMouthAnchorNames[i];
                if (string.IsNullOrWhiteSpace(anchorName))
                {
                    continue;
                }

                Transform candidate = FindChildByNameRecursive(fishRoot, anchorName);
                if (candidate != null)
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private Transform FindChildByNameRecursive(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildByNameRecursive(root.GetChild(i), targetName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void DetachFromFish()
    {
        StopCastMotion();
        attachedFishOnLine = null;
        lockEndToTransform = lockEndBeforeAttach;
    }

    private void StopCastMotion()
    {
        isCastingToWater = false;
        isBobbingOnWater = false;
        StopAllCoroutines();
    }

    // Performs Verlet integration to update the position of a particle.
    private void Verlet(LineParticle p, float dt)
    {
        var temp = p.Pos;
        p.Pos += p.Pos - p.OldPos + (p.Acceleration * dt * dt);
        p.OldPos = temp;
    }

    // Applies a pole constraint to a pair of particles.
    // We want the distance between each particle to be a specific length
    private void PoleConstraint(LineParticle p1, LineParticle p2, float restLength)
    {
        var delta = p2.Pos - p1.Pos;
        var deltaLength = delta.magnitude;
        var diff = (deltaLength - restLength) / deltaLength;
        p1.Pos += delta * diff * 0.5f;
        p2.Pos -= delta * diff * 0.5f;
    }

    public void TriggerCast()
    {
        if (!isRodEquipped)
        {
            return;
        }

        StopCastMotion();
        currentTargetLength = startSegmentLength;
        SegmentLength = startSegmentLength;
        isChangingLength = false;

        if (StartPoint == null || EndPoint == null)
        {
            return;
        }

        lockEndBeforeAttach = lockEndToTransform;
        lockEndToTransform = true;
        EndPoint.position = StartPoint.position;

        Vector3 castOrigin = StartPoint.position;
        Vector3 castDirection = GetCastDirection();
        float castDistance = Random.Range(castDistanceMin, castDistanceMax);
        Vector3 fallbackTarget = castOrigin + castDirection * castDistance;

        if (TryGetWaterLandingPoint(castOrigin, castDirection, castDistance, out Vector3 waterLandingPoint))
        {
            castTargetPosition = waterLandingPoint;
            isCastingToWater = true;
            StartCoroutine(MoveHookToWater(castTargetPosition));
            return;
        }

        castTargetPosition = fallbackTarget;
        isCastingToWater = true;
        StartCoroutine(MoveHookToTarget(castTargetPosition));
    }

    public void TriggerReel()
    {
        if (!isRodEquipped)
        {
            return;
        }

        StopCastMotion();

        currentTargetLength = startSegmentLength;
        isChangingLength = true;
    }

    private Vector3 GetCastDirection()
    {
        if (StartPoint == null)
        {
            return Vector3.forward;
        }

        Vector3 forward = Vector3.ProjectOnPlane(StartPoint.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        }

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();
        float angleOffset = Random.Range(-castAngleRange, castAngleRange);
        return Quaternion.AngleAxis(angleOffset, Vector3.up) * forward;
    }

    private bool TryGetWaterLandingPoint(Vector3 origin, Vector3 direction, float distance, out Vector3 landingPoint)
    {
        landingPoint = origin + direction * distance;

        if (waterLayerMask.value == 0)
        {
            return false;
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, waterLayerMask, QueryTriggerInteraction.Collide))
        {
            landingPoint = hit.point + Vector3.up * waterSurfaceOffset;
            return true;
        }

        return false;
    }

    private IEnumerator MoveHookToWater(Vector3 targetPosition)
    {
        while (isCastingToWater && EndPoint != null && Vector3.Distance(EndPoint.position, targetPosition) > 0.02f)
        {
            EndPoint.position = Vector3.MoveTowards(EndPoint.position, targetPosition, castFlightSpeed * Time.deltaTime);
            yield return null;
        }

        if (!isCastingToWater || EndPoint == null)
        {
            yield break;
        }

        EndPoint.position = targetPosition;
        isCastingToWater = false;
        BeginWaterBob(targetPosition);
    }

    private IEnumerator MoveHookToTarget(Vector3 targetPosition)
    {
        while (isCastingToWater && EndPoint != null && Vector3.Distance(EndPoint.position, targetPosition) > 0.02f)
        {
            EndPoint.position = Vector3.MoveTowards(EndPoint.position, targetPosition, castFlightSpeed * Time.deltaTime);
            yield return null;
        }

        if (!isCastingToWater || EndPoint == null)
        {
            yield break;
        }

        EndPoint.position = targetPosition;
        isCastingToWater = false;
    }

    private void BeginWaterBob(Vector3 waterPoint)
    {
        bobCenterPosition = waterPoint;
        bobTargetPosition = waterPoint;
        isBobbingOnWater = true;
    }

    private void UpdateCastFlight()
    {
        if (EndPoint == null)
        {
            isCastingToWater = false;
        }
    }

    private void UpdateWaterBob()
    {
        if (EndPoint == null)
        {
            isBobbingOnWater = false;
            return;
        }

        if (Vector3.Distance(EndPoint.position, bobTargetPosition) <= bobTargetArrivalThreshold)
        {
            Vector2 randomOffset = Random.insideUnitCircle * bobRadius;
            bobTargetPosition = bobCenterPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
        }

        EndPoint.position = Vector3.MoveTowards(EndPoint.position, bobTargetPosition, bobMoveSpeed * Time.deltaTime);
    }

    public void SetEquippedFromController(bool equipped)
    {
        isRodEquipped = equipped;
        ApplyEquipVisualState(isRodEquipped);

        if (!isRodEquipped)
        {
            StopAllCoroutines();
            currentTargetLength = startSegmentLength;
            SegmentLength = startSegmentLength;
            isChangingLength = false;
            ResetParticlesToAnchors();
        }
    }

    private void ResetParticlesToAnchors()
    {
        if (particles == null || particles.Count == 0 || StartPoint == null || EndPoint == null)
        {
            return;
        }

        for (int i = 0; i < particles.Count; i++)
        {
            Vector3 point = Vector3.Lerp(StartPoint.position, EndPoint.position, i / (float)(particles.Count - 1));
            particles[i].Pos = point;
            particles[i].OldPos = point;
        }
    }

    private void ApplyEquipVisualState(bool equipped)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = equipped || !hideLineWhenUnequipped;
        }

        if (hookVisual != null)
        {
            hookVisual.SetActive(equipped || !hideHookWhenUnequipped);
        }
    }
}