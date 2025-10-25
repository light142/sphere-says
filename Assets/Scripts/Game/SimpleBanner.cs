using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleBanner : MonoBehaviour
{
    [Header("Banner Settings")]
    public string bannerTitle = "Welcome!";
    public string bannerMessage = "Get ready to play!";
    public string buttonText = "START";
    
    [Header("Banner Colors")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.95f);
    public Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
    
    private GameObject bannerPanel;
    private System.Action onButtonClick;
    
    public void ShowBanner(string title, string message, string buttonText, System.Action onButtonClick)
    {
        this.bannerTitle = title;
        this.bannerMessage = message;
        this.buttonText = buttonText;
        this.onButtonClick = onButtonClick;
        
        CreateBanner();
    }
    
    void CreateBanner()
    {
        // Create banner panel
        bannerPanel = new GameObject("BannerPanel");
        bannerPanel.transform.SetParent(transform, false);
        
        // Add Canvas
        Canvas canvas = bannerPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Very high priority
        
        // Add CanvasScaler
        CanvasScaler scaler = bannerPanel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        bannerPanel.AddComponent<GraphicRaycaster>();
        
        // Setup RectTransform to fill screen
        RectTransform rect = bannerPanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Create background overlay
        CreateBackground();
        
        // Create banner content
        CreateContent();
        
        // Animate in
        StartCoroutine(AnimateIn());
    }
    
    void CreateBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(bannerPanel.transform, false);
        
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f); // Semi-transparent black
    }
    
    void CreateContent()
    {
        // Create content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(bannerPanel.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.3f);
        contentRect.anchorMax = new Vector2(0.9f, 0.7f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        // Add background to content
        Image contentImage = content.AddComponent<Image>();
        contentImage.color = backgroundColor;
        
        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(content.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.7f);
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = bannerTitle;
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        
        // Create message
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(content.transform, false);
        
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.05f, 0.25f);
        messageRect.anchorMax = new Vector2(0.95f, 0.65f);
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = bannerMessage;
        messageText.fontSize = 32;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        messageText.enableWordWrapping = true;
        
        // Create button
        GameObject buttonObj = new GameObject("Button");
        buttonObj.transform.SetParent(content.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        // Create button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonTextComponent = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonTextComponent.text = buttonText;
        buttonTextComponent.fontSize = 28;
        buttonTextComponent.alignment = TextAlignmentOptions.Center;
        buttonTextComponent.color = Color.white;
        buttonTextComponent.fontStyle = FontStyles.Bold;
        
        // Add button click listener
        button.onClick.AddListener(OnButtonClick);
    }
    
    void OnButtonClick()
    {
        StartCoroutine(AnimateOut());
    }
    
    IEnumerator AnimateIn()
    {
        bannerPanel.transform.localScale = Vector3.zero;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            bannerPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            yield return null;
        }
        
        bannerPanel.transform.localScale = Vector3.one;
    }
    
    IEnumerator AnimateOut()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = bannerPanel.transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            bannerPanel.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }
        
        // Execute callback
        onButtonClick?.Invoke();
        
        // Destroy banner
        Destroy(bannerPanel);
        Destroy(gameObject);
    }
}
