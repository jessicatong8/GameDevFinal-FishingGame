using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based from this repo: https://github.com/Kevin-Kwan/Unity3D-FishingRodMotion?tab=readme-ov-file
// Inspiration from https://www.reddit.com/r/Unity3D/comments/1kx1hp/best_way_to_do_fishing_line/
public class VerletLine : MonoBehaviour
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

    // line gravity and physics settings
    [Header("Physics Settings")]
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);
    // Num of Physics iterations
    public int Iterations = 6;
    // higher is stiffer, lower is stretchier
    public float tensionConstant = 10f;
    public bool SecondHasRigidbody = false;
    public float LerpSpeed = 1f;
    public float Delay = 3f;
    private bool isChangingLength = false;

    [Header("Equip State")]
    [SerializeField] private GameObject hookVisual;
    [SerializeField] private bool hideLineWhenUnequipped = true;
    [SerializeField] private bool hideHookWhenUnequipped = true;

    private bool isRodEquipped = true;

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
        lineRenderer.positionCount = particles.Count;
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
    }

    private IEnumerator IncreaseLengthAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentTargetLength = maxSegmentLength;
        isChangingLength = true;
    }

    // Update the line with Verlet Physics.
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

        StopAllCoroutines();
        StartCoroutine(IncreaseLengthAfterDelay(Delay));
    }

    public void TriggerReel()
    {
        if (!isRodEquipped)
        {
            return;
        }

        currentTargetLength = startSegmentLength;
        isChangingLength = true;
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