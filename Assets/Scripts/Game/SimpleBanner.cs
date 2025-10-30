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
    
    [Header("Banner Fonts")]
    public TMP_FontAsset titleFont;
    public TMP_FontAsset messageFont;
    public TMP_FontAsset buttonFont;
    
    [Header("Text Effects")]
    public bool enableGlowEffect = true;
    public Color glowColor = new Color(1f, 0.8f, 0.2f, 1f); // Golden glow
    public float glowIntensity = 0.8f;
    public bool enableOutline = true;
    public Color outlineColor = new Color(0f, 0f, 0f, 1f); // Black outline
    public float outlineWidth = 0.2f;
    public bool enableGradient = true;
    public Color gradientTopColor = new Color(1f, 1f, 1f, 1f); // White
    public Color gradientBottomColor = new Color(0.8f, 0.8f, 1f, 1f); // Light blue
    
    [Header("Button Effects")]
    public bool enableButtonGlow = true;
    public Color buttonGlowColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange glow
    public float buttonGlowIntensity = 1.2f;
    public bool enableButtonPulse = true;
    public float buttonPulseSpeed = 3f;
    public float buttonPulseIntensity = 0.2f;
    public bool enableButtonShine = true;
    public Color buttonShineColor = new Color(1f, 1f, 1f, 0.8f); // White shine
    
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
        scaler.referenceResolution = new Vector2(1080, 1920); // Portrait mobile resolution
        
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
        // Create content container - Much larger for mobile (2/3 of screen)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(bannerPanel.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.05f, 0.05f); // Start from 15% from bottom
        contentRect.anchorMax = new Vector2(0.95f, 0.95f); // End at 85% from bottom (70% height)
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        // Add background to content
        Image contentImage = content.AddComponent<Image>();
        contentImage.color = backgroundColor;
        
        // Create title - Positioned at top of larger content box
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(content.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.82f); // Moved down to reduce space below
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = bannerTitle;
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Bright orange for youthful energy
        titleText.fontStyle = FontStyles.Bold;
        
        // Apply custom font if available
        if (titleFont != null)
        {
            titleText.font = titleFont;
        }
        
        // Apply text effects
        ApplyTextEffects(titleText, true); // true = is title (larger effects)
        
        // Create message - More space in the larger content box
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(content.transform, false);
        
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.05f, 0.28f); // Moved up to reduce gap with title
        messageRect.anchorMax = new Vector2(0.95f, 0.78f); // Adjusted to maintain proper spacing
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = bannerMessage;
        messageText.fontSize = 32;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = new Color(0.9f, 0.9f, 1f, 1f); // Light blue for youthful appeal
        messageText.textWrappingMode = TextWrappingModes.Normal;
        
        // Apply custom font if available
        if (messageFont != null)
        {
            messageText.font = messageFont;
        }
        
        // Apply text effects
        ApplyTextEffects(messageText, false); // false = is message (smaller effects)
        
        // Create button - Positioned at bottom of larger content box
        GameObject buttonObj = new GameObject("Button");
        buttonObj.transform.SetParent(content.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.15f, 0.05f); // Smaller button width
        buttonRect.anchorMax = new Vector2(0.85f, 0.12f); // Smaller button height
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        // Make button more exciting and youthful
        StyleExcitingButton(button, buttonImage);
        
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
        buttonTextComponent.fontSize = 32; // Smaller font for smaller button
        buttonTextComponent.alignment = TextAlignmentOptions.Center;
        buttonTextComponent.color = new Color(0.2f, 0.3f, 0.8f, 1f); // Dark blue for professional look
        buttonTextComponent.fontStyle = FontStyles.Bold;
        
        // Apply custom font if available
        if (buttonFont != null)
        {
            buttonTextComponent.font = buttonFont;
        }
        
        // Apply enhanced text effects for button visibility
        ApplyButtonTextEffects(buttonTextComponent);
        
        // Add button click listener
        button.onClick.AddListener(OnButtonClick);
    }
    
    void ApplyTextEffects(TextMeshProUGUI textComponent, bool isTitle)
    {
        if (textComponent == null) return;
        
        // Apply outline effect
        if (enableOutline)
        {
            textComponent.outlineColor = outlineColor;
            textComponent.outlineWidth = outlineWidth;
        }
        
        // Apply glow effect (using vertex gradient for glow simulation)
        if (enableGlowEffect)
        {
            textComponent.enableVertexGradient = true;
            
            // Create glow effect using vertex gradient
            if (enableGradient)
            {
                textComponent.colorGradient = new VertexGradient(
                    gradientTopColor,
                    gradientTopColor,
                    gradientBottomColor,
                    gradientBottomColor
                );
            }
            
            // Add glow effect using outline (simulates shadow/glow)
            if (enableOutline)
            {
                textComponent.outlineColor = new Color(glowColor.r, glowColor.g, glowColor.b, glowIntensity);
                textComponent.outlineWidth = outlineWidth + 0.1f; // Slightly thicker for glow effect
            }
        }
        
        // Add pulsing animation for title
        if (isTitle && enableGlowEffect)
        {
            StartCoroutine(PulseText(textComponent));
        }
    }
    
    System.Collections.IEnumerator PulseText(TextMeshProUGUI textComponent)
    {
        Color originalColor = textComponent.color;
        float pulseSpeed = 2f;
        float pulseIntensity = 0.3f;
        
        while (textComponent != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
            textComponent.color = new Color(
                originalColor.r * pulse,
                originalColor.g * pulse,
                originalColor.b * pulse,
                originalColor.a
            );
            yield return null;
        }
    }
    
    void ApplyButtonTextEffects(TextMeshProUGUI buttonTextComponent)
    {
        if (buttonTextComponent == null) return;
        
        // Make button text highly visible with darker colors
        buttonTextComponent.color = new Color(0.2f, 0.3f, 0.8f, 1f); // Dark blue for professional look
        buttonTextComponent.fontStyle = FontStyles.Bold;
        
        // Add strong outline for maximum visibility
        buttonTextComponent.outlineColor = new Color(0.1f, 0.1f, 0.3f, 1f); // Darker blue outline
        buttonTextComponent.outlineWidth = 0.4f; // Medium outline for clean look
        
        // Add glow effect for extra visibility
        buttonTextComponent.enableVertexGradient = true;
        buttonTextComponent.colorGradient = new VertexGradient(
            new Color(0.3f, 0.4f, 0.9f, 1f),     // Top: Lighter blue
            new Color(0.2f, 0.3f, 0.8f, 1f),     // Top right: Medium blue
            new Color(0.1f, 0.2f, 0.7f, 1f),     // Bottom: Darker blue
            new Color(0.1f, 0.1f, 0.6f, 1f)      // Bottom right: Darkest blue
        );
        
        // Add pulsing effect to make it more attention-grabbing
        StartCoroutine(PulseButtonText(buttonTextComponent));
    }
    
    System.Collections.IEnumerator PulseButtonText(TextMeshProUGUI buttonTextComponent)
    {
        Color baseColor = new Color(0.2f, 0.3f, 0.8f, 1f); // Dark blue base
        float pulseSpeed = 2f; // Slower pulse for professional look
        float pulseIntensity = 0.2f; // Subtle pulse for elegance
        
        while (buttonTextComponent != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
            
            // Pulse between dark blue and lighter blue for professional elegance
            Color pulseColor = Color.Lerp(baseColor, new Color(0.4f, 0.5f, 0.9f, 1f), (pulse - 1f) * 0.3f);
            buttonTextComponent.color = pulseColor;
            
            yield return null;
        }
    }
    
    void StyleExcitingButton(Button button, Image buttonImage)
    {
        if (button == null || buttonImage == null) return;
        
        // Create exciting button styling
        buttonImage.type = Image.Type.Sliced;
        
        // Add glow effect
        if (enableButtonGlow)
        {
            CreateButtonGlow(button);
        }
        
        // Add shine effect
        if (enableButtonShine)
        {
            CreateButtonShine(button);
        }
        
        // Add pulsing animation
        if (enableButtonPulse)
        {
            StartCoroutine(PulseButton(button, buttonImage));
        }
        
        // Setup exciting button colors
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(
            Mathf.Min(buttonColor.r + 0.3f, 1f),
            Mathf.Min(buttonColor.g + 0.3f, 1f),
            Mathf.Min(buttonColor.b + 0.3f, 1f),
            buttonColor.a
        );
        colors.pressedColor = new Color(
            Mathf.Max(buttonColor.r - 0.2f, 0f),
            Mathf.Max(buttonColor.g - 0.2f, 0f),
            Mathf.Max(buttonColor.b - 0.2f, 0f),
            buttonColor.a
        );
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        colors.colorMultiplier = 1.2f; // More vibrant
        colors.fadeDuration = 0.1f; // Quick transitions
        button.colors = colors;
    }
    
    void CreateButtonGlow(Button button)
    {
        // Create glow effect using multiple shadow copies
        for (int i = 0; i < 3; i++)
        {
            GameObject glowObj = new GameObject($"ButtonGlow_{i}");
            glowObj.transform.SetParent(button.transform.parent, false);
            glowObj.transform.SetSiblingIndex(button.transform.GetSiblingIndex());
            
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = button.GetComponent<RectTransform>().anchorMin;
            glowRect.anchorMax = button.GetComponent<RectTransform>().anchorMax;
            glowRect.offsetMin = button.GetComponent<RectTransform>().offsetMin;
            glowRect.offsetMax = button.GetComponent<RectTransform>().offsetMax;
            
            Image glowImage = glowObj.AddComponent<Image>();
            glowImage.color = new Color(
                buttonGlowColor.r,
                buttonGlowColor.g,
                buttonGlowColor.b,
                buttonGlowIntensity * (0.3f - i * 0.1f)
            );
            glowImage.type = Image.Type.Sliced;
            
            // Offset the glow
            float offset = (i + 1) * 2f;
            glowRect.anchoredPosition = new Vector2(offset, -offset);
        }
    }
    
    void CreateButtonShine(Button button)
    {
        // Create shine effect
        GameObject shineObj = new GameObject("ButtonShine");
        shineObj.transform.SetParent(button.transform, false);
        
        RectTransform shineRect = shineObj.AddComponent<RectTransform>();
        shineRect.anchorMin = new Vector2(0f, 0f);
        shineRect.anchorMax = new Vector2(1f, 1f);
        shineRect.offsetMin = new Vector2(5f, 5f);
        shineRect.offsetMax = new Vector2(-5f, -5f);
        
        Image shineImage = shineObj.AddComponent<Image>();
        shineImage.color = buttonShineColor;
        shineImage.type = Image.Type.Sliced;
        
        // Add shine animation
        StartCoroutine(ShineButton(shineImage));
    }
    
    System.Collections.IEnumerator PulseButton(Button button, Image buttonImage)
    {
        Color originalColor = buttonImage.color;
        float originalScale = 1f;
        
        while (button != null && buttonImage != null)
        {
            float pulse = Mathf.Sin(Time.time * buttonPulseSpeed) * buttonPulseIntensity + 1f;
            
            // Pulse the color
            buttonImage.color = new Color(
                originalColor.r * pulse,
                originalColor.g * pulse,
                originalColor.b * pulse,
                originalColor.a
            );
            
            // Pulse the scale slightly
            button.transform.localScale = Vector3.one * (originalScale + (pulse - 1f) * 0.05f);
            
            yield return null;
        }
    }
    
    System.Collections.IEnumerator ShineButton(Image shineImage)
    {
        float shineSpeed = 2f;
        
        while (shineImage != null)
        {
            float shine = Mathf.Sin(Time.time * shineSpeed) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(0f, 1f, shine) * 0.6f;
            
            shineImage.color = new Color(
                buttonShineColor.r,
                buttonShineColor.g,
                buttonShineColor.b,
                alpha
            );
            
            yield return null;
        }
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
