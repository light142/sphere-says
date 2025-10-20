using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI instructionText;
    public Button restartButton;
    public Button menuButton;
    
    [Header("AR Selection")]
    public Button selectButton;
    public TextMeshProUGUI selectButtonText;
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button playAgainButton;
    
    private SimonSaysGame simonGame;
    private ARSelectionManager selectionManager;
    private OrbiterController orbiterController;
    
    void Start()
    {
        // Make GameUIPanel background transparent to remove whitish overlay
        SetupTransparentBackground();
        
        // Setup AR UI with same styling as 2D
        SetupARUI();
        
        // Setup AR selection system
        SetupARSelection();
        
        simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (simonGame != null)
        {
            simonGame.OnSequenceComplete += OnSequenceComplete;
            simonGame.OnGameOver += OnGameOver;
            simonGame.OnLevelComplete += OnLevelComplete;
            simonGame.OnColorHighlight += OnColorHighlight; // ✅ Added missing event subscription
            simonGame.OnOrbiterShrink += OnOrbiterShrink;
            simonGame.OnOrbiterGrow += OnOrbiterGrow;
        }
        
        // Find orbiter controller
        orbiterController = FindFirstObjectByType<OrbiterController>();
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(BackToMenu);
            
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);
    }
    
    void SetupARSelection()
    {
        // Create AR Selection Manager
        selectionManager = gameObject.AddComponent<ARSelectionManager>();
        
        // Create Select button if it doesn't exist
        if (selectButton == null)
        {
            CreateSelectButton();
        }
        
        // Assign UI references
        if (selectButton != null)
        {
            selectionManager.selectButton = selectButton;
        }
        
        if (selectButtonText != null)
        {
            selectionManager.selectButtonText = selectButtonText;
        }
        
        // Position and style Select button
        StyleSelectButton();
    }
    
    void CreateSelectButton()
    {
        // Create Select button GameObject
        GameObject selectButtonObj = new GameObject("SelectButton");
        selectButtonObj.transform.SetParent(transform, false);
        
        // Add RectTransform
        RectTransform selectRect = selectButtonObj.AddComponent<RectTransform>();
        selectRect.anchorMin = new Vector2(0.5f, 0f);
        selectRect.anchorMax = new Vector2(0.5f, 0f);
        selectRect.sizeDelta = new Vector2(300, 100); // Match other button sizes
        selectRect.anchoredPosition = new Vector2(0, 250); // Position above other buttons
        
        // Add Button component
        selectButton = selectButtonObj.AddComponent<Button>();
        
        // Add Image component
        Image selectImage = selectButtonObj.AddComponent<Image>();
        selectImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Simple Green
        
        // Add rounded corners to SELECT button
        AddRoundedCorners(selectImage);
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(selectButtonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        selectButtonText = textObj.AddComponent<TextMeshProUGUI>();
        selectButtonText.text = "SELECT";
        selectButtonText.color = Color.white;
        selectButtonText.fontStyle = FontStyles.Bold;
        selectButtonText.fontSize = 36; // Match other button text size
        selectButtonText.alignment = TextAlignmentOptions.Center;
        
        // Initially hide the button
        selectButtonObj.SetActive(false);
        
    }
    
    void StyleSelectButton()
    {
        if (selectButton != null)
        {
            // Position Select button at bottom center - Keep larger size
            RectTransform selectRect = selectButton.GetComponent<RectTransform>();
            selectRect.anchorMin = new Vector2(0.5f, 0f);
            selectRect.anchorMax = new Vector2(0.5f, 0f);
            selectRect.sizeDelta = new Vector2(300, 100); // Match other button sizes
            selectRect.anchoredPosition = new Vector2(0, 250); // Position above other buttons
            
            // Style the select button
            Image selectImage = selectButton.GetComponent<Image>();
            if (selectImage != null)
            {
                selectImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
            }
            
            if (selectButtonText != null)
            {
                selectButtonText.text = "SELECT";
                selectButtonText.color = Color.white;
                selectButtonText.fontStyle = FontStyles.Bold;
                selectButtonText.fontSize = 36; // Match other button text size
            }
        }
    }
    
    
    void SetupTransparentBackground()
    {
        // Find and make GameUIPanel background transparent
        Image panelImage = GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(1f, 1f, 1f, 0f); // Fully transparent
        }
        
        // Also check for any child panels
        Image[] childImages = GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img.name.Contains("Panel") || img.name.Contains("Background"))
            {
                img.color = new Color(1f, 1f, 1f, 0f); // Fully transparent
            }
        }
    }
    
    void SetupARUI()
    {
        // Setup AR Game Canvas for mobile (same as 2D)
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
        
        // Style text elements - Even larger for AR
        if (levelText != null)
        {
            levelText.fontSize = 60; // Even larger AR level text
            levelText.alignment = TextAlignmentOptions.Center;
            // Position level text at top (mobile-friendly)
            RectTransform levelRect = levelText.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 1f);
            levelRect.anchorMax = new Vector2(0.5f, 1f);
            levelRect.sizeDelta = new Vector2(500, 120); // Larger container for AR
            levelRect.anchoredPosition = new Vector2(0, -200); // Move down to avoid camera notch
        }
        
        if (instructionText != null)
        {
            instructionText.fontSize = 48; // Even larger AR instruction text
            instructionText.alignment = TextAlignmentOptions.Center;
            // Position instruction text below level (mobile-friendly)
            RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
            instructionRect.anchorMin = new Vector2(0.5f, 1f);
            instructionRect.anchorMax = new Vector2(0.5f, 1f);
            instructionRect.sizeDelta = new Vector2(600, 100); // Larger container for AR
            instructionRect.anchoredPosition = new Vector2(0, -300); // Move down to avoid camera notch
        }
        
        if (selectButtonText != null)
        {
            selectButtonText.fontSize = 36; // Larger select button text for AR
            selectButtonText.alignment = TextAlignmentOptions.Center;
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.fontSize = 48; // Larger final score text for AR
            finalScoreText.alignment = TextAlignmentOptions.Center;
        }
        
        // Style buttons - Match 2D game button theme for Restart and Menu buttons
        Button[] allButtons = { restartButton, menuButton, selectButton, playAgainButton };
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button button = allButtons[i];
            if (button != null)
            {
                // Make buttons appropriate size for AR mobile experience
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(300, 100); // Match GameManager sizing
                
                // Apply 2D game button theme to Restart and Menu buttons only
                if (button == restartButton || button == menuButton)
                {
                    StyleARControlButton(button, i);
                }
                
                // Style button text
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = 32; // Match GameManager font size
                    buttonText.alignment = TextAlignmentOptions.Center;
                }
            }
        }
    }
    
    void StyleARControlButton(Button button, int index)
    {
        if (button == null) return;
        
        // AR-style sleek colors
        Color[] arColors = {
            new Color(0.1f, 0.3f, 0.8f, 0.9f),    // Deep blue with transparency for Restart
            new Color(0.8f, 0.2f, 0.3f, 0.9f)     // Deep red with transparency for Menu
        };
        
        Color[] highlightColors = {
            new Color(0.2f, 0.5f, 1f, 0.9f),      // Brighter blue for Restart
            new Color(1f, 0.4f, 0.5f, 0.9f)       // Brighter red for Menu
        };
        
        Color primaryColor = arColors[index % arColors.Length];
        Color highlightColor = highlightColors[index % highlightColors.Length];
        
        // Set button color with AR-style transparency
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = primaryColor;
            
            // Add rounded corners to button
            AddRoundedCorners(buttonImage);
            
            // Add subtle inner glow for AR effect
            CreateARButtonGlow(button, highlightColor);
        }
        
        // Style button text with AR aesthetics
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
        }
        
        // Setup AR-style hover effects
        SetupARButtonHoverEffects(button, primaryColor, highlightColor);
    }
    
    void CreateARButtonGlow(Button button, Color glowColor)
    {
        // Create subtle inner glow for AR effect
        GameObject glowObj = new GameObject("ARButtonGlow");
        glowObj.transform.SetParent(button.transform, false);
        
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(2, 2);
        glowRect.offsetMax = new Vector2(-2, -2);
        
        Image glowImage = glowObj.AddComponent<Image>();
        glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.2f);
        
        // Add rounded corners to glow
        AddRoundedCorners(glowImage);
    }
    
    
    void SetupARButtonHoverEffects(Button button, Color normalColor, Color highlightColor)
    {
        // Create AR-style color transitions
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightColor;
        colors.pressedColor = new Color(highlightColor.r * 0.8f, highlightColor.g * 0.8f, highlightColor.b * 0.8f, 0.9f);
        colors.selectedColor = highlightColor;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.2f; // Slightly longer for AR feel
        
        button.colors = colors;
    }
    
    void AddRoundedCorners(Image image)
    {
        // Add rounded corners using a simple approach
        // This creates a subtle rounded corner effect
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
        }
    }
    
    void OnSequenceComplete(bool success)
    {
        if (success)
        {
            instructionText.text = "Correct! Next level...";
            // Disable selection immediately when player completes sequence correctly
            selectionManager?.DisableSelection();
            // Keep the message visible for a while
            StartCoroutine(ClearSuccessMessageAfterDelay(2f));
        }
        else
        {
            instructionText.text = "Wrong! Game Over!";
            // Disable selection on game over
            selectionManager?.DisableSelection();
        }
    }
    
    System.Collections.IEnumerator ClearSuccessMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
    
    void OnLevelComplete(int level)
    {
        instructionText.text = $"Level {level} Complete!";
    }
    
    void OnGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: Level {GameManager.Instance.currentLevel}";
        }
    }
    
    void OnColorHighlight(Color color)
    {
        // Re-enable selection when Simon starts new sequence
        if (selectionManager != null)
        {
            selectionManager.EnableSelection();
        }
        
        // Find and highlight the corresponding AR sphere
        ARColorSphere[] spheres = FindObjectsByType<ARColorSphere>(FindObjectsSortMode.None);
        foreach (ARColorSphere sphere in spheres)
        {
            if (sphere.sphereColor == color)
            {
                // Simon sequence highlighting (temporary - auto-unhighlight after duration)
                sphere.SetHighlightColor(Color.white);
                sphere.HighlightSphere(true); // true = auto-unhighlight for Simon sequence
                break;
            }
        }
    }
    
    public void RestartGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        GameManager.Instance.RestartGame();
    }
    
    public void BackToMenu()
    {
        GameManager.Instance.BackToMenu();
    }
    
    void OnOrbiterShrink()
    {
        // Find orbiter controller dynamically in case it wasn't found at startup
        if (orbiterController == null)
        {
            orbiterController = FindFirstObjectByType<OrbiterController>();
        }
        
        if (orbiterController != null)
        {
            orbiterController.ShrinkOrbiter();
        }
        else
        {
        }
    }
    
    void OnOrbiterGrow()
    {
        // Find orbiter controller dynamically in case it wasn't found at startup
        if (orbiterController == null)
        {
            orbiterController = FindFirstObjectByType<OrbiterController>();
        }
        
        if (orbiterController != null)
        {
            orbiterController.GrowOrbiter();
        }
        else
        {
        }
    }
}
