using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menuPausePanel;
    private PlayerInputState playerInputState;
    private void OnEnable()
    {
        playerInputState = PlayerInputState.Instance;
        PlayerInputState.MenuTogglePerformed += HandleMenu;
    }
    private void Start()
    {
        menuPausePanel.SetActive(false);
    }
    private void OnDisable()
    {
        PlayerInputState.MenuTogglePerformed -= HandleMenu;
    }

    private void HandleMenu()
    {
        if (playerInputState.CurrentState == PlayerInputState.InputStates.Menu)     // Disable menu and return to gameplay    
        {
            // DebugLogger.Instance.Log("MenuUI: Toggling menu off, returning to gameplay.");
            menuPausePanel.SetActive(false);
            playerInputState.SetState(PlayerInputState.InputStates.Gameplay);
        }
        else                                                                        // Enable menu
        {
            // DebugLogger.Instance.Log("MenuUI: Toggling menu on, switching to Menu input state.");
            menuPausePanel.SetActive(true);
            playerInputState.SetState(PlayerInputState.InputStates.Menu);
        }
    }
    public void HandleResume()
    {
        HandleMenu();
    }
    public void HandleQuit()
    {
        SceneManager.LoadScene("TitleScene");
    }
}