using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based from this repo: https://github.com/Kevin-Kwan/Unity3D-FishingRodMotion?tab=readme-ov-file
// Inspiration from https://www.reddit.com/r/Unity3D/comments/1kx1hp/best_way_to_do_fishing_line/
// MOSTLY ADAPTED WITH AI
public class VerletFishingLine : MonoBehaviour
{
    [Header("Line Settings")]
    public Transform StartPoint;
    public Transform EndPoint;
    public LineRenderer lineRenderer;
    public int Segments = 15;
    public float SegmentLength = 0.05f;

    [Header("Physics Settings")]
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);
    public int Iterations = 5; // How stiff the rope is

    [Header("State")]
    [SerializeField] private FishingManager fishingManager;
    private bool isRodEquipped = true;
    private Transform attachedFishOnLine;

    private class LineParticle
    {
        public Vector3 Pos;
        public Vector3 OldPos;
        public Vector3 Acceleration;
    }

    private List<LineParticle> particles;
    private Vector3 lockedLandingPos; // New variable to store the landing spot
    private bool isHookLocked = false; // Flag to keep it stuck
    [Header("Flight Settings")]
    [SerializeField] private float castFlightSpeed = 15f; // Adjust for faster/slower flight
    void Start()
    {
        EnsureInitialized();
    }

    private bool EnsureInitialized()
    {
        if (fishingManager == null) fishingManager = FindFirstObjectByType<FishingManager>();
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        if (StartPoint == null || EndPoint == null || lineRenderer == null)
        {
            Debug.LogWarning("VerletFishingLine: Missing StartPoint, EndPoint, or LineRenderer reference.", this);
            return false;
        }

        if (Segments < 2) Segments = 2;

        if (particles == null || particles.Count != Segments)
        {
            particles = new List<LineParticle>(Segments);
            for (int i = 0; i < Segments; i++)
            {
                Vector3 point = Vector3.Lerp(StartPoint.position, EndPoint.position, i / (float)(Segments - 1));
                particles.Add(new LineParticle { Pos = point, OldPos = point, Acceleration = Gravity });
            }

            lineRenderer.positionCount = particles.Count;
        }

        return true;
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += DetachFromFish;
        FishingManager.OnEscaped += DetachFromFish;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= DetachFromFish;
        FishingManager.OnEscaped -= DetachFromFish;
    }


    void FixedUpdate()
    {
        if (!isRodEquipped || !EnsureInitialized()) return;

        // 1. Verlet Integration
        foreach (var p in particles)
        {
            var temp = p.Pos;
            p.Pos += p.Pos - p.OldPos + (p.Acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime);
            p.OldPos = temp;
        }

        // 2. Constraints Loop
        for (int i = 0; i < Iterations; i++)
        {
            // CRITICAL: Force the anchors BEFORE and AFTER the pole constraints
            particles[0].Pos = StartPoint.position;
            particles[particles.Count - 1].Pos = EndPoint.position;

            for (int j = 0; j < particles.Count - 1; j++)
            {
                var p1 = particles[j];
                var p2 = particles[j + 1];
                var delta = p2.Pos - p1.Pos;
                var dist = delta.magnitude;
                if (dist > 0)
                {
                    var diff = (dist - SegmentLength) / dist;
                    p1.Pos += delta * diff * 0.5f;
                    p2.Pos -= delta * diff * 0.5f;
                }
            }

            // Re-pin anchors to prevent the physics from "pulling" them off the rod/hook
            particles[0].Pos = StartPoint.position;
            particles[particles.Count - 1].Pos = EndPoint.position;
        }
    }

    // 3. The Visual Sync
    void LateUpdate()
    {
        if (!isRodEquipped || !EnsureInitialized()) return;

        // If a fish is on, follow the fish. Otherwise, if casted, stay at the landing spot
        if (attachedFishOnLine != null)
        {
            EndPoint.position = attachedFishOnLine.position;
            isHookLocked = false; // Fish is moving the hook now[cite: 8]
        }
        else if (isHookLocked)
        {
            EndPoint.position = lockedLandingPos; // Keep hook stuck to the water surface[cite: 8]
        }
        // Update the LineRenderer here so it uses the most recent positions 
        // after the player's movement has finished for the frame.
        var positions = new Vector3[particles.Count];
        for (int i = 0; i < particles.Count; i++)
        {
            positions[i] = particles[i].Pos;
        }
        lineRenderer.SetPositions(positions);
    }
    public void TriggerCast(Vector3? targetPoint)
    {
        if (!EnsureInitialized()) return;

        attachedFishOnLine = null;

        // Determine the landing point (now with negative Z fallback)
        Vector3 finalPoint = targetPoint.HasValue ? targetPoint.Value : StartPoint.position + (Vector3.back * 15f);

        // Teleport hook and lock it
        EndPoint.position = finalPoint;
        lockedLandingPos = finalPoint;
        isHookLocked = true;

        // Distribute particles to prevent snap-back
        for (int i = 0; i < particles.Count; i++)
        {
            Vector3 spawnPos = Vector3.Lerp(StartPoint.position, finalPoint, i / (float)(Segments - 1));
            particles[i].Pos = spawnPos;
            particles[i].OldPos = spawnPos;
        }
    }

    public void TriggerReel()
    {
        if (!EnsureInitialized()) return;

        isHookLocked = false; // Unlock so it can return[cite: 8]
        EndPoint.position = StartPoint.position;
        attachedFishOnLine = null;
    }

    private void HandleHooked()
    {
        if (!EnsureInitialized()) return;

        if (fishingManager?.activeFish == null)
            return;

        // Try to find the fish's mouth bone (main1), searching recursively.
        Transform mouth = FindBoneInHierarchy(fishingManager.activeFish.transform, "main1");
        Transform target = mouth != null ? mouth : fishingManager.activeFish.transform;
        attachedFishOnLine = target;
        isHookLocked = false;

        if (mouth != null)
            DebugLogger.Instance.Log($"VerletFishingLine: Attached to fish mouth at {mouth.position}");
        else
            DebugLogger.Instance.LogWarning("VerletFishingLine: Fish 'main1' mouth not found; using root instead.");

        // Re-seed particles to the new endpoint to avoid explosive correction when endpoint jumps.
        EndPoint.position = target.position;
        for (int i = 0; i < particles.Count; i++)
        {
            Vector3 p = Vector3.Lerp(StartPoint.position, EndPoint.position, i / (float)(Segments - 1));
            particles[i].Pos = p;
            particles[i].OldPos = p;
        }
    }

    // Helper to find a bone by name in the fish's transform hierarchy
    private Transform FindBoneInHierarchy(Transform parent, string boneName)
    {
        if (parent.name == boneName)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindBoneInHierarchy(child, boneName);
            if (result != null)
                return result;
        }

        return null;
    }
    private void DetachFromFish() => attachedFishOnLine = null;

    public void SetEquippedFromController(bool equipped)
    {
        isRodEquipped = equipped;
        lineRenderer.enabled = equipped;
    }
}