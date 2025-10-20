using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Game Over UI References")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI gameOverText;
    public Button playAgainButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    void Start()
    {
        SetupGameOverUI();
        EnsureLevelTextExists();
        
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(BackToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    void EnsureLevelTextExists()
    {
        // Make sure we have a level text element
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        
        // If we don't have enough text elements, create the level text
        if (texts.Length < 2)
        {
            GameObject levelTextObj = new GameObject("LevelText");
            levelTextObj.transform.SetParent(transform, false);
            
            RectTransform levelRect = levelTextObj.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 0.5f);
            levelRect.anchorMax = new Vector2(0.5f, 0.5f);
            levelRect.sizeDelta = new Vector2(500, 80); // Match the larger container size
            levelRect.anchoredPosition = new Vector2(0, 120); // Match the adjusted position
            
            TextMeshProUGUI levelText = levelTextObj.AddComponent<TextMeshProUGUI>();
            levelText.text = $"Final Score: Level {GameManager.Instance.currentLevel}";
            levelText.fontSize = 36; // Match the larger score text size
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            levelText.fontStyle = FontStyles.Bold;
            
            finalScoreText = levelText;
        }
        else if (texts.Length >= 2)
        {
            finalScoreText = texts[1];
            finalScoreText.text = $"Final Score: Level {GameManager.Instance.currentLevel}";
        }
    }
    
    void SetupGameOverUI()
    {
        // Setup Game Over Canvas
        Canvas gameOverCanvas = GetComponent<Canvas>();
        if (gameOverCanvas != null)
        {
            gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameOverCanvas.sortingOrder = 3; // Highest priority
        }
        
        // Create elegant background
        SetupBackground();
        
        // Position UI elements for mobile with better styling
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        Button[] buttons = GetComponentsInChildren<Button>();
        
        // Style game over text - positioned to avoid cutoff
        if (texts.Length > 0)
        {
            RectTransform gameOverRect = texts[0].GetComponent<RectTransform>();
            gameOverRect.anchorMin = new Vector2(0.5f, 0.5f);
            gameOverRect.anchorMax = new Vector2(0.5f, 0.5f);
            gameOverRect.sizeDelta = new Vector2(600, 120); // Larger container for larger text
            gameOverRect.anchoredPosition = new Vector2(0, 200); // Moved further down for larger text
            
            TextMeshProUGUI gameOverText = texts[0].GetComponent<TextMeshProUGUI>();
            if (gameOverText != null)
            {
                gameOverText.fontSize = 64; // Much larger Game Over text
                gameOverText.alignment = TextAlignmentOptions.Center;
                gameOverText.text = "Game Over!";
                gameOverText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red color
                gameOverText.fontStyle = FontStyles.Bold;
            }
        }
        
        // Style final score text - positioned ABOVE buttons to be visible
        if (texts.Length > 1)
        {
            RectTransform scoreRect = texts[1].GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreRect.sizeDelta = new Vector2(500, 80); // Larger container for larger text
            scoreRect.anchoredPosition = new Vector2(0, 120); // Adjusted position for larger text
            
            TextMeshProUGUI scoreText = texts[1].GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                scoreText.fontSize = 36; // Much larger for better visibility
                scoreText.alignment = TextAlignmentOptions.Center;
                scoreText.text = $"Final Score: Level {GameManager.Instance.currentLevel}";
                scoreText.color = new Color(1f, 1f, 1f, 1f); // White for better contrast
                scoreText.fontStyle = FontStyles.Bold;
            }
        }
        
        // Style buttons in center with proper spacing - positioned BELOW level text
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                RectTransform rect = buttons[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(350, 100); // Much larger buttons for mobile
                // Position buttons BELOW level text to avoid overlap
                rect.anchoredPosition = new Vector2(0, 20 - (i * 120)); // More spacing for larger buttons
                
                // Style button appearance
                StyleButton(buttons[i], i);
            }
        }
    }
    
    void SetupBackground()
    {
        // Create elegant background with gradient effect
        Image background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
        }
        
        // Create gradient background
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark semi-transparent
        
        // Add subtle border
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
    
    void StyleButton(Button button, int index)
    {
        // Create modern button styling
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Gradient-like colors for different buttons
            Color[] buttonColors = {
                new Color(0.2f, 0.8f, 0.2f, 1f), // Green for Play Again
                new Color(0.2f, 0.4f, 0.8f, 1f), // Blue for Back to Menu
                new Color(0.8f, 0.2f, 0.2f, 1f)  // Red for Quit
            };
            
            buttonImage.color = buttonColors[index % buttonColors.Length];
        }
        
        // Style button text
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.fontSize = 32; // Much larger button text for mobile
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
        }
        
        // Add subtle shadow effect (simulated with outline)
        if (buttonText != null)
        {
            buttonText.outlineWidth = 2f;
            buttonText.outlineColor = new Color(0, 0, 0, 0.5f);
        }
    }
    
    void Update()
    {
        // Update final score text
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: Level {GameManager.Instance.currentLevel}";
        }
        else
        {
            // If finalScoreText is null, try to find it in children
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 1)
            {
                finalScoreText = texts[1];
                finalScoreText.text = $"Final Score: Level {GameManager.Instance.currentLevel}";
            }
        }
    }
    
    public void PlayAgain()
    {
        GameManager.Instance.RestartGame();
    }
    
    public void BackToMainMenu()
    {
        GameManager.Instance.BackToMenu();
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
