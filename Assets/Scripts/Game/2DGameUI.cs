using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game2DUI : MonoBehaviour
{
    [Header("2D Game UI References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI instructionText;
    public Button restartButton;
    public Button menuButton;
    
    [Header("Color Buttons")]
    public Button redButton;
    public Button blueButton;
    public Button greenButton;
    public Button yellowButton;
    
    private SimonSaysGame simonGame;
    
    void Start()
    {
        Setup2DUI();
        SetupColorButtons();
        simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (simonGame != null)
        {
            simonGame.OnSequenceComplete += OnSequenceComplete;
            simonGame.OnGameOver += OnGameOver;
            simonGame.OnLevelComplete += OnLevelComplete;
            simonGame.OnColorHighlight += OnColorHighlight;
        }
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(BackToMenu);
            
        // Reset button appearance on start
        ResetAllButtonAppearance();
    }
    
    void SetupColorButtons()
    {
        // Setup color buttons with proper colors and ColorButton scripts
        Button[] colorButtons = { redButton, blueButton, greenButton, yellowButton };
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
        
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] != null)
            {
                // Add ColorButton script if not present
                ColorButton colorButtonScript = colorButtons[i].GetComponent<ColorButton>();
                if (colorButtonScript == null)
                {
                    colorButtonScript = colorButtons[i].gameObject.AddComponent<ColorButton>();
                }
                
                // Set the button color
                colorButtonScript.SetColor(colors[i]);
                
                // Set button image color
                Image buttonImage = colorButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = colors[i];
                }
                
            }
        }
    }
    
    void Setup2DUI()
    {
        // Setup 2D Game Canvas for mobile
        Canvas gameCanvas = GetComponent<Canvas>();
        if (gameCanvas != null)
        {
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameCanvas.sortingOrder = 2;
            
            // Setup Canvas Scaler for mobile
            CanvasScaler scaler = gameCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameCanvas.gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Portrait mobile resolution
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }
        
        // Position UI elements for mobile
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        Button[] buttons = GetComponentsInChildren<Button>();
        
        // Position level text at top (mobile-friendly)
        if (texts.Length > 0)
        {
            RectTransform levelRect = texts[0].GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 1f);
            levelRect.anchorMax = new Vector2(0.5f, 1f);
            levelRect.sizeDelta = new Vector2(300, 80);
            levelRect.anchoredPosition = new Vector2(0, -80);
            
            // Make text larger for mobile
            TextMeshProUGUI levelText = texts[0].GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.fontSize = 36;
                levelText.alignment = TextAlignmentOptions.Center;
            }
        }
        
        // Position instruction text below level (mobile-friendly)
        if (texts.Length > 1)
        {
            RectTransform instructionRect = texts[1].GetComponent<RectTransform>();
            instructionRect.anchorMin = new Vector2(0.5f, 1f);
            instructionRect.anchorMax = new Vector2(0.5f, 1f);
            instructionRect.sizeDelta = new Vector2(400, 60);
            instructionRect.anchoredPosition = new Vector2(0, -160);
            
            // Make text larger for mobile
            TextMeshProUGUI instructionText = texts[1].GetComponent<TextMeshProUGUI>();
            if (instructionText != null)
            {
                instructionText.fontSize = 28;
                instructionText.alignment = TextAlignmentOptions.Center;
            }
        }
        
        // Position color buttons in a 2x2 grid (mobile-optimized)
        Button[] colorButtons = { redButton, blueButton, greenButton, yellowButton };
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] != null)
            {
                RectTransform rect = colorButtons[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(200, 200); // Larger buttons for mobile
                
                // 2x2 grid positioning with better spacing
                int row = i / 2;
                int col = i % 2;
                float x = (col - 0.5f) * 250; // Increased spacing
                float y = (row - 0.5f) * 250; // Increased spacing
                rect.anchoredPosition = new Vector2(x, y - 50); // Slightly lower for mobile
                
                // Make button text larger
                TextMeshProUGUI buttonText = colorButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = 32;
                    buttonText.alignment = TextAlignmentOptions.Center;
                }
            }
        }
        
        // Position control buttons at bottom (mobile-friendly)
        Button[] controlButtons = { restartButton, menuButton };
        for (int i = 0; i < controlButtons.Length; i++)
        {
            if (controlButtons[i] != null)
            {
                RectTransform rect = controlButtons[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(200, 60); // Larger buttons for mobile
                rect.anchoredPosition = new Vector2(-100 + (i * 200), 80); // Better spacing
                
                // Make button text larger
                TextMeshProUGUI buttonText = controlButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = 24;
                    buttonText.alignment = TextAlignmentOptions.Center;
                }
            }
        }
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {GameManager.Instance.currentLevel}";
        }
        
        if (instructionText != null)
        {
            if (simonGame != null)
            {
                if (simonGame.IsIdle())
                {
                    instructionText.text = "Get ready...";
                }
                else if (simonGame.IsPlayingSequence())
                {
                    instructionText.text = "Watch the sequence...";
                }
                else if (simonGame.IsWaitingForInput())
                {
                    instructionText.text = "Repeat the sequence!";
                }
                else
                {
                    instructionText.text = "Get ready...";
                }
            }
            else
            {
                instructionText.text = "Simon Says Game not found!";
            }
        }
    }
    
    void OnSequenceComplete(bool success)
    {
        if (success)
        {
            instructionText.text = "Correct! Next level...";
        }
        else
        {
            instructionText.text = "Wrong! Game Over!";
        }
    }
    
    void OnLevelComplete(int level)
    {
        instructionText.text = $"Level {level} Complete!";
    }
    
    void OnGameOver()
    {
        instructionText.text = "Game Over!";
    }
    
    void OnColorHighlight(Color color)
    {
        // Find and highlight the corresponding button
        Button[] colorButtons = { redButton, blueButton, greenButton, yellowButton };
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
        
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] != null && colors[i] == color)
            {
                ColorButton colorButtonScript = colorButtons[i].GetComponent<ColorButton>();
                if (colorButtonScript != null)
                {
                    colorButtonScript.HighlightButton();
                }
                break;
            }
        }
        
        // Play audio for Simon's highlighting
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayColorNote(color);
        }
        
    }
    
    void ResetAllButtonAppearance()
    {
        // Reset all color buttons to normal appearance
        Button[] colorButtons = { redButton, blueButton, greenButton, yellowButton };
        foreach (Button button in colorButtons)
        {
            if (button != null)
            {
                ColorButton colorButtonScript = button.GetComponent<ColorButton>();
                if (colorButtonScript != null)
                {
                    colorButtonScript.ResetButtonAppearance();
                }
            }
        }
    }
    
    public void RestartGame()
    {
        // Reset all color buttons to normal appearance
        ResetAllButtonAppearance();
        
        GameManager.Instance.RestartGame();
    }
    
    public void BackToMenu()
    {
        GameManager.Instance.BackToMenu();
    }
}
