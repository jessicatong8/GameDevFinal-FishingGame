using Unity.VisualScripting;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] private PlayerInputState inputState;
    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private FishingManager fishingManager;

    private const float TARGET_MASH_RATE = 3f; // expected mashes per second
    private bool mashTriggeredThisFrame;
    private float progress;
    private bool isReeling;



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

        activeFish = fishingManager.activeFish;
        if (activeFish == null)
        {
            Debug.LogError("ProgressManager: No active fish found.");
        }
        progress = 0f;
        isReeling = true;
    }
    void Start()
    {
        progress = 0f;
    }

    void Update()
    {
        if (isReeling)
        {
            UpdateProgress(mashTriggeredThisFrame);
            mashTriggeredThisFrame = false;
        }
    }

    private void UpdateProgress(bool mashedThisFrame)
    {
        if (mashedThisFrame && tensionManager.IsInSafeZone())
        {
            float increment = activeFish.reelingSpeed / TARGET_MASH_RATE;
            progress += increment;
            progress = Mathf.Min(progress, 100f);
        }
        if (progress >= 100f)
        {
            fishingManager.CaughtFish();
        }
    }

    private void HandleResetToIdle()
    {
        progress = 0f;
        isReeling = false;
    }

    public float GetCurrentProgress()
    {
        return progress;
    }

    public bool IsProgressComplete()
    {
        return progress >= 100f;

    }
}
