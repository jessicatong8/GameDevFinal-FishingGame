using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// Inspiration from https://www.reddit.com/r/Unity3D/comments/1kx1hp/best_way_to_do_fishing_line/
public class VerletLine : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;
    public int Segments = 10;
    public LineRenderer lineRenderer;
    public float SegmentLength = 0.03f;
    public float startSegmentLength = 0.03f;
    public float currentTargetLength = 0.03f;
    public float maxSegmentLength = 1f;
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);
    // Num of Physics iterations
    public int Iterations = 6;
    // higher is stiffer, lower is stretchier
    public float tensionConstant = 10f;
    public bool SecondHasRigidbody = false;
    public float LerpSpeed = 1f;
    public float Delay = 3f;
    private bool isChangingLength = false;

    [Header("Input System Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string castActionName = "Attack";
    [SerializeField] private string reelActionName = "Next";

    private InputAction castAction;
    private InputAction reelAction;

    [Header("Equip State")]
    [SerializeField] private ThirdPController playerController;
    [SerializeField] private GameObject hookVisual;
    [SerializeField] private bool hideLineWhenUnequipped = true;
    [SerializeField] private bool hideHookWhenUnequipped = true;

    private bool isRodEquipped = true;
    private bool externalEquipStateOverride = false;

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
        InitializeInputActions();

        if (playerController == null)
        {
            playerController = GetComponentInParent<ThirdPController>();
        }

        if (hookVisual == null && EndPoint != null)
        {
            hookVisual = EndPoint.gameObject;
        }

        isRodEquipped = IsRodEquipped();
        ApplyEquipVisualState(isRodEquipped);

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
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    void Update()
    {
        bool equippedNow = IsRodEquipped();
        if (equippedNow != isRodEquipped)
        {
            isRodEquipped = equippedNow;
            ApplyEquipVisualState(isRodEquipped);

            if (!isRodEquipped)
            {
                // Stop pending delayed casts and retract line immediately when unequipped.
                StopAllCoroutines();
                currentTargetLength = startSegmentLength;
                SegmentLength = startSegmentLength;
                isChangingLength = false;
            }
        }

        if (!isRodEquipped)
        {
            return;
        }

        if (CastPressed())
        {   
            // Cast out the line
            // A generic delay because I don't know the timing of casting out a fishing line and when that line comes out
            // I'm assuming at the peak of the cast, idk
            StartCoroutine(IncreaseLengthAfterDelay(Delay));

        }
        else if (ReelPressed())
        {
            // Reel In
            currentTargetLength = startSegmentLength;
            isChangingLength = true;
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

    
        foreach (var p in particles)
        {
            Verlet(p, Time.fixedDeltaTime);
        }
    
        for (int i = 0; i < Iterations; i++)
        {
            for (int j = 0; j < particles.Count - 1; j++)
            {
                PoleConstraint(particles[j], particles[j + 1], SegmentLength);
            }
        }
        particles[0].Pos = StartPoint.position;
        if (SecondHasRigidbody)
        {
            Vector3 force = (particles[particles.Count - 1].Pos - EndPoint.position) * tensionConstant;
            EndPoint.GetComponent<Rigidbody>().AddForce(force);
        }
    
        particles[particles.Count - 1].Pos = EndPoint.position;
    
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

    private bool CastPressed()
    {
        if (castAction != null)
        {
            return castAction.WasPressedThisFrame();
        }

        bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame;
        return mouse || gamepad;
    }

    private bool ReelPressed()
    {
        if (reelAction != null)
        {
            return reelAction.WasPressedThisFrame();
        }

        bool keyboard = Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    private void InitializeInputActions()
    {
        if (inputActionsAsset == null)
        {
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                inputActionsAsset = playerInput.actions;
            }
        }

        if (inputActionsAsset == null)
        {
            return;
        }

        InputActionMap actionMap = inputActionsAsset.FindActionMap(playerActionMapName, false);
        if (actionMap == null)
        {
            return;
        }

        castAction = actionMap.FindAction(castActionName, false);
        reelAction = actionMap.FindAction(reelActionName, false);

        EnableInputActions();
    }

    private void EnableInputActions()
    {
        castAction?.Enable();
        reelAction?.Enable();
    }

    private void DisableInputActions()
    {
        castAction?.Disable();
        reelAction?.Disable();
    }

    private bool IsRodEquipped()
    {
        if (externalEquipStateOverride)
        {
            return isRodEquipped;
        }

        if (playerController != null)
        {
            return playerController.isFishing;
        }

        return true;
    }

    public void SetEquippedFromController(bool equipped)
    {
        externalEquipStateOverride = true;
        isRodEquipped = equipped;
        ApplyEquipVisualState(isRodEquipped);

        if (!isRodEquipped)
        {
            StopAllCoroutines();
            currentTargetLength = startSegmentLength;
            SegmentLength = startSegmentLength;
            isChangingLength = false;
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