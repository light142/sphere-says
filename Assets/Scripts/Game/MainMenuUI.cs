using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button play2DButton;
    public Button playARButton;
    public Button quitButton;
    
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    
    void Start()
    {
        // Add background manager for nice visual effects
        UIBackgroundManager bgManager = GetComponent<UIBackgroundManager>();
        if (bgManager == null)
        {
            bgManager = gameObject.AddComponent<UIBackgroundManager>();
        }
        
        // Purple theme for main menu
        bgManager.topColor = new Color(0.3f, 0.1f, 0.5f, 0.2f); // Deep purple
        bgManager.bottomColor = new Color(0.1f, 0.05f, 0.2f, 0.2f); // Dark purple
        bgManager.patternColor = new Color(0.8f, 0.6f, 1f, 0.15f); // Light purple pattern
        bgManager.particleColor = new Color(0.9f, 0.7f, 1f, 1f); // Purple particles - fully opaque
        
        // Update colors if background already exists
        bgManager.UpdateBackgroundColors();

        if (play2DButton != null)
            play2DButton.onClick.AddListener(Play2D);
            
        if (playARButton != null)
            playARButton.onClick.AddListener(PlayAR);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void Play2D()
    {
        GameManager.Instance.Start2DGame();
    }
    
    public void PlayAR()
    {
        GameManager.Instance.StartARGame();
    }
    
    public void QuitGame()
    {
        // Track quit_application event
        TelemetryManager.Instance?.TrackQuitApplication();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
