using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetup = true;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupGameOverUI();
        }
    }
    
    [ContextMenu("Setup Game Over UI")]
    public void SetupGameOverUI()
    {
        
        // Create Game Over Canvas if it doesn't exist
        GameObject gameOverCanvas = GameObject.Find("GameOverCanvas");
        if (gameOverCanvas == null)
        {
            gameOverCanvas = new GameObject("GameOverCanvas");
            Canvas canvas = gameOverCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 3;
            
            // Add Canvas Scaler
            CanvasScaler scaler = gameOverCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Add Graphic Raycaster
            gameOverCanvas.AddComponent<GraphicRaycaster>();
            
            // Set inactive initially
            gameOverCanvas.SetActive(false);
        }
        
        // Create Game Over Panel
        GameObject gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel == null)
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(gameOverCanvas.transform, false);
            
            // Add RectTransform
            RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
            
            // Add Image
            Image panelImage = gameOverPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
            
            // Add GameOverUI script
            GameOverUI gameOverUI = gameOverPanel.AddComponent<GameOverUI>();
        }
        
        // Create Game Over Text
        GameObject gameOverText = GameObject.Find("GameOverText");
        if (gameOverText == null)
        {
            gameOverText = new GameObject("GameOverText");
            gameOverText.transform.SetParent(gameOverPanel.transform, false);
            
            // Add RectTransform
            RectTransform textRect = gameOverText.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.sizeDelta = new Vector2(400, 100);
            textRect.anchoredPosition = new Vector2(0, -100);
            
            // Add TextMeshPro
            TextMeshProUGUI text = gameOverText.AddComponent<TextMeshProUGUI>();
            text.text = "Game Over!";
            text.fontSize = 64; // Match the larger Game Over text
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }
        
        // Create Final Score Text
        GameObject finalScoreText = GameObject.Find("FinalScoreText");
        if (finalScoreText == null)
        {
            finalScoreText = new GameObject("FinalScoreText");
            finalScoreText.transform.SetParent(gameOverPanel.transform, false);
            
            // Add RectTransform
            RectTransform scoreRect = finalScoreText.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 1f);
            scoreRect.anchorMax = new Vector2(0.5f, 1f);
            scoreRect.sizeDelta = new Vector2(300, 60);
            scoreRect.anchoredPosition = new Vector2(0, -200);
            
            // Add TextMeshPro
            TextMeshProUGUI scoreText = finalScoreText.AddComponent<TextMeshProUGUI>();
            scoreText.text = "Final Score: Level 1";
            scoreText.fontSize = 36; // Match the larger score text
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.color = Color.white;
        }
        
        // Create Play Again Button
        GameObject playAgainButton = GameObject.Find("PlayAgainButton");
        if (playAgainButton == null)
        {
            playAgainButton = CreateButton("PlayAgainButton", "Play Again", new Vector2(0, 100));
            playAgainButton.transform.SetParent(gameOverPanel.transform, false);
        }
        
        // Create Back to Menu Button
        GameObject mainMenuButton = GameObject.Find("MainMenuButton");
        if (mainMenuButton == null)
        {
            mainMenuButton = CreateButton("MainMenuButton", "Back to Menu", new Vector2(0, 20));
            mainMenuButton.transform.SetParent(gameOverPanel.transform, false);
        }
        
        // Create Quit Button
        GameObject quitButton = GameObject.Find("QuitButton");
        if (quitButton == null)
        {
            quitButton = CreateButton("QuitButton", "Quit", new Vector2(0, -60));
            quitButton.transform.SetParent(gameOverPanel.transform, false);
        }
        
        // Connect to GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // Use reflection to set the gameOverUI field
            var field = typeof(GameManager).GetField("gameOverUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                field = typeof(GameManager).GetField("gameOverUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }
            if (field != null)
            {
                field.SetValue(gameManager, gameOverCanvas);
            }
        }
        
    }
    
    GameObject CreateButton(string name, string text, Vector2 position)
    {
        GameObject button = new GameObject(name);
        
        // Add RectTransform
        RectTransform rect = button.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(350, 100); // Match the larger button size
        rect.anchoredPosition = position;
        
        // Add Image
        Image image = button.AddComponent<Image>();
        image.color = Color.white;
        
        // Add Button
        Button buttonComponent = button.AddComponent<Button>();
        
        // Create Text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 48; // Match the larger button text
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.black;
        
        return button;
    }
}
