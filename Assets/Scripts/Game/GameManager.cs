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
    private GameObject currentBanner;
    
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
        
        // Initialize telemetry manager
        InitializeTelemetry();
        
        ShowMainMenu();
        
        // Show startup banner
        ShowStartupBanner();
    }
    
    void InitializeTelemetry()
    {
        // Create telemetry manager if it doesn't exist
        if (TelemetryManager.Instance == null)
        {
            GameObject telemetryObj = new GameObject("TelemetryManager");
            telemetryObj.AddComponent<TelemetryManager>();
        }
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
            
            // Position main menu buttons - Much larger for mobile with galaxy theme
            Button[] mainMenuButtons = mainMenuUI.GetComponentsInChildren<Button>();
            MainMenuUI mainMenuScript = mainMenuUI.GetComponent<MainMenuUI>();
            if (mainMenuScript == null)
            {
                mainMenuScript = mainMenuUI.GetComponentInChildren<MainMenuUI>();
            }
            int buttonIndex = 0;
            
            for (int i = 0; i < mainMenuButtons.Length; i++)
            {
                Button btn = mainMenuButtons[i];
                RectTransform rect = btn.GetComponent<RectTransform>();
                
                // Check if this is the info button - check by reference or name
                bool isInfoButton = false;
                if (mainMenuScript != null && mainMenuScript.infoButton != null)
                {
                    isInfoButton = (btn == mainMenuScript.infoButton);
                }
                // Fallback: check by name
                if (!isInfoButton && (btn.name.ToLower().Contains("info") || btn.name.ToLower().Contains("information")))
                {
                    isInfoButton = true;
                }
                
                if (isInfoButton)
                {
                    // Position info button in top-right corner
                    rect.anchorMin = new Vector2(1f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.sizeDelta = new Vector2(100, 100); // Small circular button
                    rect.anchoredPosition = new Vector2(-120, -120); // Top-right corner with padding
                }
                else
                {
                    // Regular menu buttons
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(600, 150); // Much larger buttons for mobile
                    rect.anchoredPosition = new Vector2(0, 200 - (buttonIndex * 200)); // More spacing between buttons
                    buttonIndex++;
                }
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
                instructionRect.sizeDelta = new Vector2(900, 120); // Much wider container for AR
                instructionRect.anchoredPosition = new Vector2(0, -350); // Move down to avoid camera notch
                
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
        
        // Galaxy background
        mainMenuImage.color = new Color(0.05f, 0.05f, 0.15f, 1f); // Deep space blue
        
        
        // Style main menu buttons
        Button[] buttons = mainMenuUI.GetComponentsInChildren<Button>();
        MainMenuUI mainMenuScript = mainMenuUI.GetComponent<MainMenuUI>();
        if (mainMenuScript == null)
        {
            mainMenuScript = mainMenuUI.GetComponentInChildren<MainMenuUI>();
        }
        
        int styleIndex = 0;
        for (int i = 0; i < buttons.Length; i++)
        {
            // Check if this is the info button - check by reference or name
            bool isInfoButton = false;
            if (mainMenuScript != null && mainMenuScript.infoButton != null)
            {
                isInfoButton = (buttons[i] == mainMenuScript.infoButton);
            }
            // Fallback: check by name
            if (!isInfoButton && (buttons[i].name.ToLower().Contains("info") || buttons[i].name.ToLower().Contains("information")))
            {
                isInfoButton = true;
            }
            
            if (isInfoButton)
            {
                StyleInfoButton(buttons[i]);
            }
            else
            {
                StyleButton(buttons[i], styleIndex);
                styleIndex++;
            }
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
        
        // Galaxy-themed button colors
        Color[] buttonColors = {
            new Color(0.3f, 0.4f, 0.9f, 1f),    // Cosmic blue
            new Color(0.8f, 0.2f, 0.8f, 1f),    // Cosmic purple
            new Color(1f, 0.6f, 0.2f, 1f),      // Cosmic orange
            new Color(0.2f, 0.8f, 0.8f, 1f)     // Cosmic cyan
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
        
        // Style button text - Galaxy theme for mobile
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = new Color(1f, 1f, 1f, 1f); // Bright white text
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.fontSize = 40; // Much larger text for mobile
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.outlineColor = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark outline for contrast
            buttonText.outlineWidth = 0.3f;
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
    
    void StyleInfoButton(Button button)
    {
        if (button == null) return;
        
        // Make button small and circular-looking
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Style as circular button - light blue background
            buttonImage.color = new Color(0.4f, 0.5f, 0.9f, 0.9f); // Light blue with transparency
            buttonImage.type = Image.Type.Simple;
        }
        
        // Find or create button text - show "i" icon
        TextMeshProUGUI buttonText = null;
        
        // First, try to find existing text component
        TextMeshProUGUI[] allTexts = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null && text.name != "MaskImage")
            {
                buttonText = text;
                break;
            }
        }
        
        // If no text component exists, create one
        if (buttonText == null)
        {
            GameObject textObj = new GameObject("InfoText");
            textObj.transform.SetParent(button.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            buttonText = textObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Style the text
        if (buttonText != null)
        {
            buttonText.text = "i"; // Information icon
            buttonText.color = new Color(1f, 1f, 1f, 1f); // White text
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.fontSize = 48; // Large "i" for visibility
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.outlineColor = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark outline
            buttonText.outlineWidth = 0.3f;
            buttonText.enabled = true;
        }
    }
    
    public void ShowMainMenu()
    {
        // Hide encouragement popup immediately
        if (EncouragementPopup.Instance != null)
        {
            EncouragementPopup.Instance.Cleanup();
        }
        
        currentGameMode = GameMode.None;
        if (mainMenuUI != null) 
        {
            mainMenuUI.SetActive(true);
            // Restart background animation when showing main menu
            UIBackgroundManager bgManager = mainMenuUI.GetComponent<UIBackgroundManager>();
            if (bgManager != null)
            {
                bgManager.RestartAnimation();
            }
        }
        if (gameUI != null) gameUI.SetActive(false);
        if (game2DUI != null) game2DUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }
    
    void ShowStartupBanner()
    {
        if (currentBanner != null)
        {
            Destroy(currentBanner);
        }
        currentBanner = new GameObject("StartupBanner");
        SimpleBanner banner = currentBanner.AddComponent<SimpleBanner>();
        banner.ShowBanner(
            "~ RESEARCH PARTICIPANT INFORMATION ~",
            "\n\n\n STUDY PURPOSE\n~~~\nThis research explores how Augmented Reality (AR) affects short-term memory performance compared to traditional 2D games. Your participation helps us understand cognitive engagement in AR environments.\n\n\n YOUR PARTICIPATION\n~~~\nYour participation is entirely voluntary. You may withdraw at any time without giving a reason. You are not required to provide any personal identifying information.\n\n\n PRIVACY & CONFIDENTIALITY\n~~~\nAll data collected is anonymous and confidential. Only game performance metrics (accuracy and response times) are recorded. Your identity will not be linked to any results.\n\n\n RESEARCH TEAM\n~~~\nResearcher: Aung Aung (au00001@ess.ais.ac.nz)\nSupervisor: Dr. Saranya Selvarangan (saranyas@ais.ac.nz)\nAuckland Institute of Studies\n\n\nThank you for participating in this research study!",
            "I AGREE TO PARTICIPATE",
            () => { ShowResearchInstructionsBanner(); }
        );
    }
    
    public void ShowResearchInstructionsBanner()
    {
        if (currentBanner != null)
        {
            Destroy(currentBanner);
        }
        currentBanner = new GameObject("ResearchInstructionsBanner");
        SimpleBanner banner = currentBanner.AddComponent<SimpleBanner>();
        banner.ShowBanner(
            "~ RESEARCH INSTRUCTIONS ~",
            "We appreciate your participation in this research study.\n\n\n\n GAME MODES\n~~~\nThere are two versions of the memory game available to play:\n\n• AR (Augmented Reality) Mode\n\n• 2D (Traditional Touchscreen) Mode\n\n\n\n INSTRUCTIONS\n~~~\nYou can play either game mode in any order you prefer.\n\nYou can play each mode as many times as you want.\n\nSwitch between modes anytime using the menu buttons.\nSelect your preferred game mode from the menu and start playing!\n\n\n\nReady to begin your memory challenge?",
            "GOT IT",
            () => { /* Banner confirmed, ready for game selection */ }
        );
    }
    
    public void ShowResearchInfoFromMenu()
    {
        // Show research participant information, then instructions
        ShowStartupBanner();
    }
    
    public void Start2DGame()
    {

        currentGameMode = GameMode.Mode2D;
        
        // Track initialise_game event
        TelemetryManager.Instance?.TrackInitialiseGame();
        
        // Stop the game and clean up orbiter state when switching to 2D mode
        if (simonGame != null)
        {
            simonGame.StopGame();
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
        
        // Track initialise_game event
        TelemetryManager.Instance?.TrackInitialiseGame();
        
        // Stop any existing delayed start coroutine and clean up game state
        StopAllCoroutines();
        
        // Stop the game and clean up orbiter state when switching to AR mode
        if (simonGame != null)
        {
            simonGame.StopGame();
        }
        
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
                // Restart background animation when showing 2D game
                UIBackgroundManager bgManager = game2DUI.GetComponent<UIBackgroundManager>();
                if (bgManager != null)
                {
                    bgManager.RestartAnimation();
                }
            }
            if (gameUI != null) gameUI.SetActive(false);
        }
        else if (currentGameMode == GameMode.ModeAR)
        {
            if (gameUI != null) 
            {
                gameUI.SetActive(true);
                // Restart background animation when showing AR game
                UIBackgroundManager bgManager = gameUI.GetComponent<UIBackgroundManager>();
                if (bgManager != null)
                {
                    bgManager.RestartAnimation();
                }
            }

            if (game2DUI != null)
                game2DUI.SetActive(false);
        }
        
        // Initialize the Simon Says game
        simonGame = GetComponent<SimonSaysGame>();
        if (simonGame == null)
        {
            simonGame = gameObject.AddComponent<SimonSaysGame>();
        }
        
        // Unsubscribe from previous events first to prevent duplicates
        simonGame.OnGameOver -= GameOver;
        simonGame.OnSequenceComplete -= OnSequenceComplete;
        simonGame.OnLevelComplete -= OnLevelComplete;
        simonGame.OnGetReady -= OnGetReady;
        
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
            
            // Show AR game banner
            if (currentBanner != null)
            {
                Destroy(currentBanner);
            }
            // Track show_banner_ar event
            TelemetryManager.Instance?.TrackShowBannerAR();
            
            currentBanner = new GameObject("ARGameBanner");
            SimpleBanner banner = currentBanner.AddComponent<SimpleBanner>();
            banner.ShowBanner(
                "~ SPIKY'S COSMIC ADVENTURE ~",
                "Welcome to the Augmented Reality Dimension!\n\n\n ~SPIKY~ The Monster Traveller\n~~~\nYour spiky orange monster companion will orbit around you, moving between glowing cosmic orbs in your space.\n\n\n YOUR MISSION\n~~~\nWatch Spiky visit each orb in sequence. When it's your turn, look at the correct orb and interact with it!\n\n\n AR MAGIC\n~~~\nThe cosmic orbs exist in your real world - look around and interact with them! Each orb pulses with mystical energy and plays unique sounds.\n\n\n MEMORY CHALLENGE\n~~~\nRemember the sequence as Spiky visits each orb. When Spiky disappears, repeat the pattern by selecting the correct orbs in order!\n\n\n Ready to help Spiky master the cosmic AR dimension?",
                "JOIN SPIKY'S ADVENTURE",
                () => {
                    simonGame.StartNewGame();
                }
            );
        }
        else
        {
            // Show 2D game banner
            if (currentBanner != null)
            {
                Destroy(currentBanner);
            }
            // Track show_banner_2d event
            TelemetryManager.Instance?.TrackShowBanner2D();
            
            currentBanner = new GameObject("2DGameBanner");
            SimpleBanner banner = currentBanner.AddComponent<SimpleBanner>();
            banner.ShowBanner(
                "CLASSIC GAME",
                "Welcome to the classic memory challenge!\n\n\nFOUR COLOR BUTTONS\n~~~\nWatch the sequence of colored buttons light up and play their unique sounds.\n\n\nMEMORY CHALLENGE\n~~~\nRemember the exact order and repeat the sequence by tapping the buttons in the same order!\n\n\nLEVEL UP!\n~~~\nEach level adds one more button to the sequence. How far can you go?\n\n\nBUTTON COLORS\n~~~\nGREEN • YELLOW\nRED • BLUE\n\n\nReady to test your memory skills?",
                "Let's Play!",
                () => {
                    simonGame.StartNewGame();
                }
            );
        }
    }
    
    public void GameOver()
    {
        // Hide encouragement popup immediately
        if (EncouragementPopup.Instance != null)
        {
            EncouragementPopup.Instance.Cleanup();
        }
        
        // Track game_over event with sequence and player input data
        if (simonGame != null)
        {
            // Get current sequence and player input from SimonSaysGame
            var sequence = simonGame.GetCurrentSequence();
            var playerInput = simonGame.GetCurrentPlayerInput();
            TelemetryManager.Instance?.TrackGameOver(sequence, playerInput);
        }
        else
        {
            TelemetryManager.Instance?.TrackGameOver();
        }
        
        // Reset level when game is over
        currentLevel = 1;
        
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
            // Restart background animation when showing game over
            UIBackgroundManager bgManager = gameOverUI.GetComponent<UIBackgroundManager>();
            if (bgManager != null)
            {
                bgManager.RestartAnimation();
            }
        }
    }
    
    public void RestartGame()
    {
        // Hide encouragement popup immediately
        if (EncouragementPopup.Instance != null)
        {
            EncouragementPopup.Instance.Cleanup();
        }
        
        // Track restart_game event
        TelemetryManager.Instance?.TrackRestartGame();
        
        currentLevel = 1;
        
        // Hide game over UI
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(false);
        }
        
        // Stop the game completely to clean up orbiter state
        if (simonGame != null)
        {
            simonGame.StopGame();
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
        // Hide encouragement popup immediately
        if (EncouragementPopup.Instance != null)
        {
            EncouragementPopup.Instance.Cleanup();
        }
        
        // Track back_to_menu event
        TelemetryManager.Instance?.TrackBackToMenu();
        
        // Reset level when going back to menu
        currentLevel = 1;
        
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
        
        // Stop the game completely to clean up orbiter state
        if (simonGame != null)
        {
            simonGame.StopGame();
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
