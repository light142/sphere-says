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
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
