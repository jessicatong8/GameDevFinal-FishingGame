using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] private PlayerInputState inputState;
    private bool mashTriggeredThisFrame;
    private float progress;

    FishingManager fishingManager;
    Fish activeFish;


    private void OnEnable()
    {
        if (inputState == null)
        {
            Debug.LogError("FishingManager: No PlayerInputState reference assigned in inspector.");
        }

        if (inputState != null)
        {
            inputState.MashPerformed += HandleMashPerformed;
        }

        FishingManager.OnCaught += HandleResetToIdle;
        FishingManager.OnEscaped += HandleResetToIdle;
        FishingManager.OnHook += HandleHooked;
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.MashPerformed -= HandleMashPerformed;
        }
    }

    private void HandleMashPerformed()
    {
        mashTriggeredThisFrame = true;
    }

    private void HandleHooked()
    {
        fishingManager = GetComponent<FishingManager>();
        activeFish = fishingManager.activeFish;
        if (activeFish == null)
        {
            Debug.LogError("TensionManager: No active fish found.");
        }
        progress = 0f;
    }
    void Start()
    {
        progress = 0f;
    }

    void Update()
    {
        TickReelingState(mashTriggeredThisFrame);
        mashTriggeredThisFrame = false;
    }

    private void TickReelingState(bool mashedThisFrame)
    {
        if (mashedThisFrame)
        {
            progress += activeFish.reelingSpeed;
            progress = Mathf.Min(progress, 100f);


        }
    }

    private void HandleResetToIdle()
    {
        progress = 0f;
    }

    public float GetCurrentProgress()
    {
        return progress;
    }
}
