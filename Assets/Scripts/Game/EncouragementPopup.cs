using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EncouragementPopup : MonoBehaviour
{
    private static EncouragementPopup instance;
    
    [Header("Popup Settings")]
    public TextMeshProUGUI encouragementText;
    public float displayDuration = 1.5f;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    
    [Header("Encouragement Messages")]
    public string[] encouragementMessages = {
        "Excellent!",
        "Good Job!",
        "Perfect!",
        "Great!",
        "Awesome!",
        "Well Done!",
        "Fantastic!",
        "Brilliant!"
    };
    
    private bool isDisplaying = false;
    private Color originalTextColor;
    
    void Awake()
    {
        // Singleton pattern to prevent duplicates
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("Multiple EncouragementPopup instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Setup canvas for overlay
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High priority to appear above all UI
        canvas.overrideSorting = true;
        
        // Create encouragement text if not assigned
        if (encouragementText == null)
        {
            CreateEncouragementText();
        }
        
        // Store original color
        if (encouragementText != null)
        {
            originalTextColor = encouragementText.color;
        }
        
        // Initially hide the popup
        if (encouragementText != null)
        {
            encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        }
    }
    
    void CreateEncouragementText()
    {
        // Create background badge
        GameObject badgeObj = new GameObject("EncouragementBadge");
        badgeObj.transform.SetParent(transform, false);
        
        // Add RectTransform for badge
        RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRect.sizeDelta = new Vector2(300, 80);
        badgeRect.anchoredPosition = Vector2.zero;
        
        // Add background image
        Image badgeImage = badgeObj.AddComponent<Image>();
        badgeImage.color = new Color(0.1f, 0.7f, 0.1f, 0.9f); // Green background
        badgeImage.type = Image.Type.Sliced;
        
        // Create text GameObject
        GameObject textObj = new GameObject("EncouragementText");
        textObj.transform.SetParent(badgeObj.transform, false);
        
        // Add RectTransform for text
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add TextMeshProUGUI
        encouragementText = textObj.AddComponent<TextMeshProUGUI>();
        encouragementText.text = "Excellent!";
        encouragementText.fontSize = 36;
        encouragementText.color = Color.white;
        encouragementText.fontStyle = FontStyles.Bold;
        encouragementText.alignment = TextAlignmentOptions.Center;
        
        // Store original color
        originalTextColor = encouragementText.color;
        
        // Hide the badge initially
        badgeImage.color = new Color(0.1f, 0.7f, 0.1f, 0f); // Hidden initially
        
        // Hide the text initially
        encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        
        // Scale down initially
        badgeRect.localScale = Vector3.zero;
    }
    
    public void ShowEncouragement()
    {
        if (isDisplaying) return;
        
        // Ensure encouragement text exists
        if (encouragementText == null)
        {
            CreateEncouragementText();
        }
        
        // Check if text was created successfully
        if (encouragementText == null)
        {
            Debug.LogWarning("Failed to create encouragement text");
            return;
        }
        
        // Position the popup based on game mode
        PositionPopup();
        
        // Select random encouragement message
        string message = encouragementMessages[Random.Range(0, encouragementMessages.Length)];
        encouragementText.text = message;
        
        // Start popup animation
        StartCoroutine(AnimateEncouragement());
    }
    
    void PositionPopup()
    {
        // Get the badge GameObject (parent of text)
        GameObject badgeObj = encouragementText.transform.parent.gameObject;
        RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
        
        // Check if we're in 2D or AR mode by looking for specific UI elements
        Game2DUI game2DUI = FindFirstObjectByType<Game2DUI>();
        bool is2DMode = game2DUI != null;
        
        if (is2DMode && game2DUI.levelText != null && game2DUI.instructionText != null)
        {
            // For 2D mode: position between level and instruction text
            RectTransform levelRect = game2DUI.levelText.GetComponent<RectTransform>();
            RectTransform instructionRect = game2DUI.instructionText.GetComponent<RectTransform>();
            
            // Calculate position between level and instruction text
            float levelY = levelRect.anchoredPosition.y;
            float instructionY = instructionRect.anchoredPosition.y;
            float middleY = (levelY + instructionY) / 2f;
            
            badgeRect.anchoredPosition = new Vector2(0, middleY);
            Debug.Log($"Positioned encouragement popup for 2D mode between level ({levelY}) and instruction ({instructionY}) at {middleY}");
        }
        else
        {
            // For AR mode: position below instruction text
            GameUI gameUI = FindFirstObjectByType<GameUI>();
            if (gameUI != null && gameUI.instructionText != null)
            {
                RectTransform instructionRect = gameUI.instructionText.GetComponent<RectTransform>();
                float instructionY = instructionRect.anchoredPosition.y;
                float belowY = instructionY - 5f; // Position below instruction text
                
                badgeRect.anchoredPosition = new Vector2(0, belowY);
                Debug.Log($"Positioned encouragement popup for AR mode below instruction ({instructionY}) at {belowY}");
            }
            else
            {
                // Fallback position
                badgeRect.anchoredPosition = new Vector2(0, -200);
                Debug.Log("Positioned encouragement popup for AR mode (fallback position)");
            }
        }
    }
    
    private IEnumerator AnimateEncouragement()
    {
        isDisplaying = true;
        
        // Check if text exists
        if (encouragementText == null)
        {
            isDisplaying = false;
            yield break;
        }
        
        // Get the badge GameObject for scaling
        GameObject badgeObj = encouragementText.transform.parent.gameObject;
        RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
        Image badgeImage = badgeObj.GetComponent<Image>();
        
        // Reset properties
        encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        badgeImage.color = new Color(0.1f, 0.7f, 0.1f, 0f);
        badgeRect.localScale = Vector3.zero;
        
        // Flashy entrance animation
        float fadeInTime = 0f;
        while (fadeInTime < fadeInDuration)
        {
            fadeInTime += Time.deltaTime;
            float progress = fadeInTime / fadeInDuration;
            
            // Scale animation (bounce effect)
            float scale = Mathf.Lerp(0f, 1.2f, progress);
            if (progress > 0.7f)
            {
                scale = Mathf.Lerp(1.2f, 1f, (progress - 0.7f) / 0.3f);
            }
            badgeRect.localScale = Vector3.one * scale;
            
            // Fade in
            float alpha = Mathf.Lerp(0f, 1f, progress);
            encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
            badgeImage.color = new Color(0.1f, 0.7f, 0.1f, alpha * 0.9f);
            
            // Color flash effect
            if (progress < 0.3f)
            {
                badgeImage.color = Color.Lerp(new Color(1f, 1f, 0.2f, alpha), new Color(0.1f, 0.7f, 0.1f, alpha * 0.9f), progress / 0.3f);
            }
            
            yield return null;
        }
        
        // Hold for display duration with subtle pulsing
        float holdTime = 0f;
        while (holdTime < displayDuration)
        {
            holdTime += Time.deltaTime;
            
            // Subtle pulsing effect
            float pulse = Mathf.Sin(holdTime * 8f) * 0.05f + 1f;
            badgeRect.localScale = Vector3.one * pulse;
            
            yield return null;
        }
        
        // Flashy exit animation
        float fadeOutTime = 0f;
        while (fadeOutTime < fadeOutDuration)
        {
            fadeOutTime += Time.deltaTime;
            float progress = fadeOutTime / fadeOutDuration;
            
            // Scale down
            float scale = Mathf.Lerp(1f, 0f, progress);
            badgeRect.localScale = Vector3.one * scale;
            
            // Fade out
            float alpha = Mathf.Lerp(1f, 0f, progress);
            encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
            badgeImage.color = new Color(0.1f, 0.7f, 0.1f, alpha * 0.9f);
            
            yield return null;
        }
        
        // Hide popup
        encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        badgeImage.color = new Color(0.1f, 0.7f, 0.1f, 0f);
        badgeRect.localScale = Vector3.zero;
        isDisplaying = false;
    }
    
    public bool IsDisplaying()
    {
        return isDisplaying;
    }
    
    public static EncouragementPopup Instance
    {
        get { return instance; }
    }
    
    public void Cleanup()
    {
        // Stop any running coroutines
        StopAllCoroutines();
        
        // Reset display state
        isDisplaying = false;
        
        // Hide the popup
        if (encouragementText != null)
        {
            encouragementText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
            
            // Also hide the badge background
            GameObject badgeObj = encouragementText.transform.parent.gameObject;
            if (badgeObj != null)
            {
                Image badgeImage = badgeObj.GetComponent<Image>();
                if (badgeImage != null)
                {
                    badgeImage.color = new Color(0.1f, 0.7f, 0.1f, 0f);
                }
                
                RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
                if (badgeRect != null)
                {
                    badgeRect.localScale = Vector3.zero;
                }
            }
        }
    }
    
    void OnDestroy()
    {
        // Reset singleton when destroyed
        if (instance == this)
        {
            instance = null;
        }
    }
}
