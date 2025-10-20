using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public GameMode currentGameMode = GameMode.None;
    public int currentLevel = 1;
    public int maxLevel = 10;
    
    [Header("UI References")]
    public GameObject mainMenuUI;
    public GameObject gameUI;
    public GameObject game2DUI;
    public GameObject gameOverUI;
    
    private SimonSaysGame simonGame;
    private ARObjectSpawnerAnchored arSpawner;
    
    public enum GameMode
    {
        None,
        Mode2D,
        ModeAR
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupUI();
        SetupARComponents();
        
        // Find the AR spawner component
        arSpawner = FindFirstObjectByType<ARObjectSpawnerAnchored>();
        
        ShowMainMenu();
    }
    
    void SetupARComponents()
    {

    }
    
    void SetupUI()
    {
        // Setup Main Menu Canvas with nice styling
        if (mainMenuUI != null)
        {
            Canvas mainMenuCanvas = mainMenuUI.GetComponent<Canvas>();
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainMenuCanvas.sortingOrder = 0;
            }
            
            // Style main menu with attractive background
            StyleMainMenu();
            
            // Position main menu buttons - Much larger for mobile
            Button[] mainMenuButtons = mainMenuUI.GetComponentsInChildren<Button>();
            for (int i = 0; i < mainMenuButtons.Length; i++)
            {
                RectTransform rect = mainMenuButtons[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(300, 80); // Much larger buttons for mobile
                rect.anchoredPosition = new Vector2(0, 120 - (i * 100)); // More spacing between buttons
            }
        }
        
        // Setup Game Canvas (AR Mode)
        if (gameUI != null)
        {
            Canvas gameCanvas = gameUI.GetComponent<Canvas>();
            if (gameCanvas != null)
            {
                gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameCanvas.sortingOrder = 1;
            }
            
            // Position game UI elements
            TextMeshProUGUI[] texts = gameUI.GetComponentsInChildren<TextMeshProUGUI>();
            Button[] buttons = gameUI.GetComponentsInChildren<Button>();
            
            // Position level text at top - Much larger for AR
            if (texts.Length > 0)
            {
                RectTransform levelRect = texts[0].GetComponent<RectTransform>();
                levelRect.anchorMin = new Vector2(0.5f, 1f);
                levelRect.anchorMax = new Vector2(0.5f, 1f);
                levelRect.sizeDelta = new Vector2(500, 120); // Much larger for AR
                levelRect.anchoredPosition = new Vector2(0, -200); // Move down to avoid camera notch
                
                // Make level text much larger for AR
                TextMeshProUGUI levelText = texts[0].GetComponent<TextMeshProUGUI>();
                if (levelText != null)
                {
                    levelText.fontSize = 60; // Much larger AR level text
                    levelText.alignment = TextAlignmentOptions.Center;
                }
            }
            
            // Position instruction text below level - Much larger for AR
            if (texts.Length > 1)
            {
                RectTransform instructionRect = texts[1].GetComponent<RectTransform>();
                instructionRect.anchorMin = new Vector2(0.5f, 1f);
                instructionRect.anchorMax = new Vector2(0.5f, 1f);
                instructionRect.sizeDelta = new Vector2(600, 100); // Much larger for AR
                instructionRect.anchoredPosition = new Vector2(0, -300); // Move down to avoid camera notch
                
                // Make instruction text much larger for AR
                TextMeshProUGUI instructionText = texts[1].GetComponent<TextMeshProUGUI>();
                if (instructionText != null)
                {
                    instructionText.fontSize = 48; // Much larger AR instruction text
                    instructionText.alignment = TextAlignmentOptions.Center;
                }
            }
            
            // Position buttons at bottom - Much larger for AR (exclude SelectButton)
            int buttonIndex = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                // Skip SelectButton - it has its own positioning
                if (buttons[i].name == "SelectButton")
                    continue;
                    
                RectTransform rect = buttons[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(300, 100); // Slightly smaller AR buttons
                rect.anchoredPosition = new Vector2(-200 + (buttonIndex * 400), 120); // Better spacing to prevent overlap
                
                // Make button text much larger for AR
                TextMeshProUGUI buttonText = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontSize = 32; // Appropriate size for smaller buttons
                    buttonText.alignment = TextAlignmentOptions.Center;
                }
                
                buttonIndex++;
            }
        }
        
        // Setup 2D Game Canvas with nice styling
        if (game2DUI != null)
        {
            Canvas game2DCanvas = game2DUI.GetComponent<Canvas>();
            if (game2DCanvas != null)
            {
                game2DCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                game2DCanvas.sortingOrder = 2;
            }
            
            // Style 2D game with attractive background
            Style2DGameUI();
        }
    }
    
    void StyleMainMenu()
    {
        // Create elegant main menu background
        Image mainMenuImage = mainMenuUI.GetComponent<Image>();
        if (mainMenuImage == null)
        {
            mainMenuImage = mainMenuUI.AddComponent<Image>();
        }
        
        // Dark gradient background
        mainMenuImage.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark blue fully opaque
        
        // Add title text if it doesn't exist
        TextMeshProUGUI[] texts = mainMenuUI.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length == 0)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(mainMenuUI.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(400, 80);
            titleRect.anchoredPosition = new Vector2(0, -100);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "SPHERE SAYS";
            titleText.fontSize = 64; // Much larger title for mobile
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 1f, 1f, 1f);
            titleText.fontStyle = FontStyles.Bold;
        }
        
        // Style main menu buttons
        Button[] buttons = mainMenuUI.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            StyleButton(buttons[i], i);
        }
    }
    
    void Style2DGameUI()
    {
        // Create attractive 2D game background
        Image game2DImage = game2DUI.GetComponent<Image>();
        if (game2DImage == null)
        {
            game2DImage = game2DUI.AddComponent<Image>();
        }
        
        // Semi-transparent dark background
        game2DImage.color = new Color(0.05f, 0.05f, 0.1f, 1f); // Very dark fully opaque
        
        // Style 2D game buttons
        Button[] buttons = game2DUI.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            StyleButton(buttons[i], i);
        }
    }
    
    void StyleButton(Button button, int index)
    {
        if (button == null) return;
        
        // Simple button colors - back to original
        Color[] buttonColors = {
            new Color(0.2f, 0.6f, 1f, 1f),    // Blue
            new Color(0.2f, 0.8f, 0.2f, 1f),  // Green  
            new Color(1f, 0.4f, 0.2f, 1f),    // Orange
            new Color(0.8f, 0.2f, 0.8f, 1f)   // Purple
        };
        
        Color buttonColor = buttonColors[index % buttonColors.Length];
        
        // Set button color
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;
            
            // Add rounded corners to main menu buttons
            AddRoundedCorners(buttonImage);
        }
        
        // Style button text - Larger for mobile
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.fontSize = 28; // Much larger text for mobile
            buttonText.alignment = TextAlignmentOptions.Center;
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
    
    public void ShowMainMenu()
    {
        currentGameMode = GameMode.None;
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (game2DUI != null) game2DUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }
    
    public void Start2DGame()
    {
        currentGameMode = GameMode.Mode2D;
        
        // Unsubscribe from orbiter events when switching to 2D mode
        if (simonGame != null)
        {
            simonGame.OnOrbiterMoveToColor -= OnOrbiterMoveToColor;
            simonGame.OnOrbiterReachedTarget -= OnOrbiterReachedTarget;
        }
        
        // Force hide all UI first
        ForceHideAllUI();
        
        // Disable AR components for 2D mode
        if (arSpawner != null)
        {
            arSpawner.enabled = false;
        }
        
        StartGame();
    }
    
    void ForceHideAllUI()
    {
        // Hide all canvases
        if (mainMenuUI != null) 
        {
            mainMenuUI.SetActive(false);
        }
        if (gameUI != null) 
        {
            gameUI.SetActive(false);
        }
        if (game2DUI != null) 
        {
            game2DUI.SetActive(false);
        }
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(false);
        }
    }
    
    public void StartARGame()
    {
        currentGameMode = GameMode.ModeAR;
        
        // Stop any existing delayed start coroutine
        StopAllCoroutines();
        
        // Enable AR components for AR mode
        if (arSpawner != null)
        {
            arSpawner.enabled = true;
            arSpawner.TriggerSpawn();
            StartGame();
        }
    }
    
    private void StartGame()
    {
        currentLevel = 1;
        
        // Clear AR selections
        ClearARSelections();
        
        // Hide main menu
        if (mainMenuUI != null) 
        {
            mainMenuUI.SetActive(false);
        }
        
        if (gameOverUI != null) gameOverUI.SetActive(false);
        
        // Show appropriate UI based on game mode
        if (currentGameMode == GameMode.Mode2D)
        {
            if (game2DUI != null) 
            {
                game2DUI.SetActive(true);
            }
            if (gameUI != null) gameUI.SetActive(false);
        }
        else if (currentGameMode == GameMode.ModeAR)
        {
            if (gameUI != null) 
                gameUI.SetActive(true);

            if (game2DUI != null)
                game2DUI.SetActive(false);
        }
        
        // Initialize the Simon Says game
        simonGame = GetComponent<SimonSaysGame>();
        if (simonGame == null)
        {
            simonGame = gameObject.AddComponent<SimonSaysGame>();
        }
        
        // Subscribe to game events
        simonGame.OnGameOver += GameOver;
        simonGame.OnSequenceComplete += OnSequenceComplete;
        simonGame.OnLevelComplete += OnLevelComplete;
        simonGame.OnGetReady += OnGetReady;
        
        // Only subscribe to orbiter events in AR mode
        if (currentGameMode == GameMode.ModeAR)
        {
            simonGame.OnOrbiterMoveToColor += OnOrbiterMoveToColor;
            simonGame.OnOrbiterReachedTarget += OnOrbiterReachedTarget;
        }
        
        simonGame.StartNewGame();
    }
    
    public void GameOver()
    {
        // Hide current game UI
        if (gameUI != null) 
        {
            gameUI.SetActive(false);
        }
        if (game2DUI != null) 
        {
            game2DUI.SetActive(false);
        }
        
        // Show game over UI
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        currentLevel = 1;
        
        // Hide game over UI
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(false);
        }
        
        // Set game to idle state during restart
        if (simonGame != null)
        {
            simonGame.SetIdle(true);
        }
        
        // Clear AR objects if in AR mode
        if (currentGameMode == GameMode.ModeAR && arSpawner != null)
        {
            // Add a small delay to ensure proper cleanup before spawning new objects
            StartCoroutine(RestartARGameWithDelay());
        }
        else
        {
            StartGame();
        }
    }
    
    private System.Collections.IEnumerator RestartARGameWithDelay()
    {
        // Clear existing objects first
        arSpawner.ClearExistingObjects();
        
        // Wait a frame to ensure objects are fully destroyed
        yield return null;
        
        // Start the AR game
        StartARGame();
    }
    
    public void BackToMenu()
    {
        // Hide game over UI
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(false);
        }
        
        // Unsubscribe from orbiter events when going back to menu
        if (simonGame != null)
        {
            simonGame.OnOrbiterMoveToColor -= OnOrbiterMoveToColor;
            simonGame.OnOrbiterReachedTarget -= OnOrbiterReachedTarget;
        }
        
        // Set game to idle state when going back to menu
        if (simonGame != null)
        {
            simonGame.SetIdle(true);
        }
        
        // Clear AR objects if in AR mode
        if (currentGameMode == GameMode.ModeAR && arSpawner != null)
        {
            arSpawner.ClearExistingObjects();
        }
        
        ShowMainMenu();
    }
    
    // Event handlers for SimonSaysGame
    void OnSequenceComplete(bool success)
    {
    }
    
    void OnLevelComplete(int level)
    {
    }
    
    void OnGetReady()
    {
        // Handle "Get Ready" state - can be used to show UI or disable interactions
    }
    
    void OnOrbiterMoveToColor(Color targetColor)
    {
        
        // Find the orbiter controller and make it orbit to the target color sphere
        if (arSpawner != null)
        {
            OrbiterController orbiterController = arSpawner.GetOrbiterController();
            if (orbiterController != null)
            {
                // Subscribe to orbiter reached target event
                orbiterController.OnReachedTargetSimple += OnOrbiterReachedTarget;
                
                // Find the target sphere with the matching color
                ARColorSphere[] spheres = FindObjectsByType<ARColorSphere>(FindObjectsSortMode.None);
                
                foreach (ARColorSphere sphere in spheres)
                {
                    if (sphere.sphereColor == targetColor)
                    {
                        orbiterController.StartOrbitingToTarget(sphere.transform);
                        break;
                    }
                }
            }
            else
            {
            }
        }
        else
        {
        }
    }
    
    void OnOrbiterReachedTarget()
    {
        // Notify SimonSaysGame that orbiter has reached its target
        if (simonGame != null)
        {
            simonGame.HandleOrbiterReachedTarget();
        }
    }
    
    void ClearARSelections()
    {
        // Clear selection manager if it exists
        ARSelectionManager selectionManager = FindFirstObjectByType<ARSelectionManager>();
        if (selectionManager != null)
        {
            selectionManager.ClearAllSelections();
            selectionManager.EnableSelection(); // Re-enable selection for new game
        }
        else
        {
            // Fallback: Clear any AR sphere selections manually
            ARColorSphere[] spheres = FindObjectsByType<ARColorSphere>(FindObjectsSortMode.None);
            foreach (ARColorSphere sphere in spheres)
            {
                sphere.UnhighlightSphere();
            }
        }
    }
}
