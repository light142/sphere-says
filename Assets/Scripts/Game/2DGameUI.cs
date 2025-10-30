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
    
    [Header("Encouragement System")]
    public EncouragementPopup encouragementPopup;
    
    private SimonSaysGame simonGame;
    
    void Start()
    {
        // Add background manager for nice visual effects
        UIBackgroundManager bgManager = GetComponent<UIBackgroundManager>();
        if (bgManager == null)
        {
            bgManager = gameObject.AddComponent<UIBackgroundManager>();
        }
        
        // Blue theme for 2D game
        bgManager.topColor = new Color(0.1f, 0.3f, 0.6f, 0.5f); // Ocean blue
        bgManager.bottomColor = new Color(0.05f, 0.1f, 0.3f, 0.5f); // Deep blue
        bgManager.patternColor = new Color(0.6f, 0.8f, 1f, 0.15f); // Light blue pattern
        bgManager.particleColor = new Color(0.7f, 0.9f, 1f, 1f); // Blue particles - fully opaque
        
        // Update colors if background already exists
        bgManager.UpdateBackgroundColors();
        
        Setup2DUI();
        SetupColorButtons();
        SetupEncouragementSystem();
        simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (simonGame != null)
        {
            // Unsubscribe from previous events first to prevent duplicates
            simonGame.OnSequenceComplete -= OnSequenceComplete;
            simonGame.OnGameOver -= OnGameOver;
            simonGame.OnLevelComplete -= OnLevelComplete;
            simonGame.OnColorHighlight -= OnColorHighlight;
            
            // Subscribe to events
            simonGame.OnSequenceComplete += OnSequenceComplete;
            simonGame.OnGameOver += OnGameOver;
            simonGame.OnLevelComplete += OnLevelComplete;
            simonGame.OnColorHighlight += OnColorHighlight;
            simonGame.OnGetReady += OnGetReady;
            // Note: OnCorrectColorSelected is not subscribed for 2D mode
            // Encouragement is only shown on level completion
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
    
    void SetupEncouragementSystem()
    {
        // Get or create the singleton encouragement popup
        if (EncouragementPopup.Instance == null)
        {
            GameObject encouragementObj = new GameObject("EncouragementPopup");
            encouragementPopup = encouragementObj.AddComponent<EncouragementPopup>();
            Debug.Log("Created new EncouragementPopup instance");
        }
        else
        {
            encouragementPopup = EncouragementPopup.Instance;
            Debug.Log("Using existing EncouragementPopup instance");
        }
    }
    
    void Setup2DUI()
    {
        // Setup 2D Game Canvas for responsive design
        Canvas gameCanvas = GetComponent<Canvas>();
        if (gameCanvas != null)
        {
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameCanvas.sortingOrder = 2;
            
            // Setup Canvas Scaler for responsive design
            CanvasScaler scaler = gameCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameCanvas.gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // Use a balanced reference resolution
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }
        
        // Position UI elements using anchors for better responsiveness
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        Button[] buttons = GetComponentsInChildren<Button>();
        
        // Position level text at top using proper anchors
        if (texts.Length > 0)
        {
            RectTransform levelRect = texts[0].GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.1f, 0.85f);
            levelRect.anchorMax = new Vector2(0.9f, 0.95f);
            levelRect.offsetMin = Vector2.zero;
            levelRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI levelText = texts[0].GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.fontSize = 48;
                levelText.alignment = TextAlignmentOptions.Center;
                levelText.color = new Color(1f, 0.8f, 0.2f, 1f); // Bright orange for visibility
                levelText.fontStyle = FontStyles.Bold;
                levelText.outlineColor = new Color(0.1f, 0.1f, 0.3f, 1f); // Dark blue outline for contrast
                levelText.outlineWidth = 0.4f;
                levelText.lineSpacing = 1.2f; // More line spacing
            }
        }
        
        // Position instruction text below level using proper anchors
        if (texts.Length > 1)
        {
            RectTransform instructionRect = texts[1].GetComponent<RectTransform>();
            instructionRect.anchorMin = new Vector2(0.1f, 0.75f);
            instructionRect.anchorMax = new Vector2(0.9f, 0.85f);
            instructionRect.offsetMin = Vector2.zero;
            instructionRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI instructionText = texts[1].GetComponent<TextMeshProUGUI>();
            if (instructionText != null)
            {
                instructionText.fontSize = 36;
                instructionText.alignment = TextAlignmentOptions.Center;
                instructionText.color = new Color(0.9f, 0.9f, 1f, 1f); // Light blue for galaxy theme
                instructionText.fontStyle = FontStyles.Bold;
                instructionText.outlineColor = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark outline for contrast
                instructionText.outlineWidth = 0.3f;
                instructionText.lineSpacing = 1.3f; // More line spacing for better readability
            }
        }
        
        // Position color buttons in a proper 2x2 grid using anchors
        Button[] colorButtons = { redButton, blueButton, greenButton, yellowButton };
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] != null)
            {
                RectTransform rect = colorButtons[i].GetComponent<RectTransform>();
                
                // Calculate grid position
                int row = i / 2;
                int col = i % 2;
                
                // Use proper anchor-based positioning for 2x2 grid
                float leftAnchor = 0.1f + col * 0.4f;   // 0.1, 0.5
                float rightAnchor = 0.1f + (col + 1) * 0.4f; // 0.5, 0.9
                float bottomAnchor = 0.25f + row * 0.25f;     // 0.25, 0.5
                float topAnchor = 0.25f + (row + 1) * 0.25f;  // 0.5, 0.75
                
                rect.anchorMin = new Vector2(leftAnchor, bottomAnchor);
                rect.anchorMax = new Vector2(rightAnchor, topAnchor);
                rect.offsetMin = new Vector2(10, 10); // Small padding
                rect.offsetMax = new Vector2(-10, -10); // Small padding
                
                // Add rounded corners to color buttons
                Image buttonImage = colorButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    AddRoundedCorners(buttonImage);
                }
                
                // Make button text responsive
                TextMeshProUGUI buttonText = colorButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = 48;
                    buttonText.alignment = TextAlignmentOptions.Center;
                }
            }
        }
        
        // Position control buttons at bottom using proper anchors
        Button[] controlButtons = { restartButton, menuButton };
        for (int i = 0; i < controlButtons.Length; i++)
        {
            if (controlButtons[i] != null)
            {
                RectTransform rect = controlButtons[i].GetComponent<RectTransform>();
                
                // Position buttons side by side at bottom
                float leftAnchor = 0.1f + i * 0.4f;   // 0.1, 0.5
                float rightAnchor = 0.1f + (i + 1) * 0.4f; // 0.5, 0.9
                
                rect.anchorMin = new Vector2(leftAnchor, 0.05f);
                rect.anchorMax = new Vector2(rightAnchor, 0.15f);
                rect.offsetMin = new Vector2(10, 10); // Small padding
                rect.offsetMax = new Vector2(-10, -10); // Small padding
                
                // Add rounded corners to control buttons
                Image buttonImage = controlButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    AddRoundedCorners(buttonImage);
                }
                
                // Make button text responsive
                TextMeshProUGUI buttonText = controlButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = 32;
                    buttonText.alignment = TextAlignmentOptions.Center;
                }
            }
        }
    }
    
    void AddRoundedCorners(Image image)
    {
        // Add rounded corners using a simple approach
        image.type = Image.Type.Sliced;
        
        // Note: For true rounded corners, you would need a custom shader or sprite
        // This approach uses the sliced image type which can provide some rounding
        // with the right sprite asset, but for now we'll use a simple approach
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
                // Try to find the SimonSaysGame again
                simonGame = FindFirstObjectByType<SimonSaysGame>();
                if (simonGame != null)
                {
                    // Reconnect events
                    simonGame.OnSequenceComplete -= OnSequenceComplete;
                    simonGame.OnGameOver -= OnGameOver;
                    simonGame.OnLevelComplete -= OnLevelComplete;
                    simonGame.OnColorHighlight -= OnColorHighlight;
                    simonGame.OnGetReady -= OnGetReady;
                    
                    simonGame.OnSequenceComplete += OnSequenceComplete;
                    simonGame.OnGameOver += OnGameOver;
                    simonGame.OnLevelComplete += OnLevelComplete;
                    simonGame.OnColorHighlight += OnColorHighlight;
                    simonGame.OnGetReady += OnGetReady;
                }
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
        
        // Show encouragement popup for level completion in 2D mode
        if (encouragementPopup != null)
        {
            encouragementPopup.ShowEncouragement();
        }
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
    
    void OnDestroy()
    {
        // Hide encouragement popup immediately
        if (EncouragementPopup.Instance != null)
        {
            EncouragementPopup.Instance.Cleanup();
        }
        
        // Unsubscribe from events to prevent memory leaks and duplicate calls
        if (simonGame != null)
        {
            simonGame.OnSequenceComplete -= OnSequenceComplete;
            simonGame.OnGameOver -= OnGameOver;
            simonGame.OnLevelComplete -= OnLevelComplete;
            simonGame.OnColorHighlight -= OnColorHighlight;
            simonGame.OnGetReady -= OnGetReady;
        }
    }
    
    void OnGetReady()
    {
        UpdateUI();
    }
    
}
