using UnityEngine;

public class TensionManager : MonoBehaviour
{
    [SerializeField] private PlayerInputState inputState;
    private bool mashTriggeredThisFrame;
    private float tension;
    private float maxTension;
    private float tensionDropRate;
    Fish activeFish;
    private FishingManager fishingManager;


    private void OnEnable()
    {
        inputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleResetToIdle;
        FishingManager.OnEscaped += HandleResetToIdle;
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
    void Start()
    {
        fishingManager = GetComponent<FishingManager>();
        tension = 0f;
    }

    void Update()
    {
        UpdateTension(mashTriggeredThisFrame);
        mashTriggeredThisFrame = false;
    }

    private void HandleHooked()
    {
        activeFish = fishingManager.activeFish;
        if (activeFish == null)
        {
            Debug.LogError("TensionManager: No active fish found.");
        }
        maxTension = activeFish.maxTension;
        tensionDropRate = activeFish.tensionDropRate;
    }

    private void UpdateTension(bool mashedThisFrame)
    {
        if (mashedThisFrame)
        {

            tension += activeFish.reelingSpeed * 0.25f;
            tension = Mathf.Min(tension, maxTension);

        }
        else
        {
            tension -= tensionDropRate * Time.deltaTime;
            tension = Mathf.Max(tension, 0);
        }

    }
    private void HandleResetToIdle()
    {
        tension = 0f;
    }
    public float GetCurrentTension()
    {
        return tension;
    }
    public float GetCurrentMaxTension()
    {
        return maxTension;
    }
}
