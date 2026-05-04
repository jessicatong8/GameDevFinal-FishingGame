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

    void Start()
    {
        if (fishingManager == null) fishingManager = FindFirstObjectByType<FishingManager>(); 
        
        // Initialize particles in a straight line between start and end
        particles = new List<LineParticle>();
        for (int i = 0; i < Segments; i++)
        {
            Vector3 point = Vector3.Lerp(StartPoint.position, EndPoint.position, i / (float)(Segments - 1));
            particles.Add(new LineParticle { Pos = point, OldPos = point, Acceleration = Gravity });
        }
        lineRenderer.positionCount = particles.Count;
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

    // This handles the "Teleport" to the water or fish mouth
    void LateUpdate()
    {
        if (!isRodEquipped) return;

        if (attachedFishOnLine != null)
        {
            EndPoint.position = attachedFishOnLine.position;
        }
    }

    void FixedUpdate()
    {
        if (!isRodEquipped || particles == null) return;

        // 1. Verlet Integration (Movement)
        foreach (var p in particles)
        {
            var temp = p.Pos;
            p.Pos += p.Pos - p.OldPos + (p.Acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime);
            p.OldPos = temp;
        }

        // 2. Constraints (Keep the segments together)
        for (int i = 0; i < Iterations; i++)
        {
            for (int j = 0; j < particles.Count - 1; j++)
            {
                var p1 = particles[j];
                var p2 = particles[j + 1];
                var delta = p2.Pos - p1.Pos;
                var diff = (delta.magnitude - SegmentLength) / delta.magnitude;
                p1.Pos += delta * diff * 0.5f;
                p2.Pos -= delta * diff * 0.5f;
            }

            // Fix the ends to the transforms
            particles[0].Pos = StartPoint.position;
            particles[particles.Count - 1].Pos = EndPoint.position;
        }

        // 3. Update LineRenderer visuals
        var positions = new Vector3[particles.Count];
        for (int i = 0; i < particles.Count; i++) positions[i] = particles[i].Pos;
        lineRenderer.SetPositions(positions);
    }

    public void TriggerCast(Vector3? targetPoint)
    {
        attachedFishOnLine = null;
        if (targetPoint.HasValue)
        {
            // Instant teleport to the bob point
            EndPoint.position = targetPoint.Value;
        }
    }

    public void TriggerReel()
    {
        // Simply snap the hook back to the rod tip
        EndPoint.position = StartPoint.position;
        attachedFishOnLine = null;
    }

    private void HandleHooked()
    {
        if (fishingManager?.activeFish != null)
        {
            // Snap to fish mouth (already implemented logic)
            attachedFishOnLine = fishingManager.activeFish.transform;
        }
    }

    private void DetachFromFish() => attachedFishOnLine = null;

    public void SetEquippedFromController(bool equipped)
    {
        isRodEquipped = equipped;
        lineRenderer.enabled = equipped;
    }
}