using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    public bool enableGradientBackground = true;
    public bool enableParticleEffect = true;
    public bool enableAnimatedPattern = true;
    
    [Header("Gradient Colors")]
    public Color topColor = new Color(0.2f, 0.3f, 0.6f, 1f);
    public Color bottomColor = new Color(0.1f, 0.1f, 0.3f, 1f);
    
    [Header("Pattern Settings")]
    public Color patternColor = new Color(1f, 1f, 1f, 0.1f);
    public float patternSpeed = 0.2f;
    public float patternScale = 2f;
    
    [Header("Particle Settings")]
    public Color particleColor = new Color(1f, 1f, 1f, 0.8f);
    public float particleSize = 0.02f;
    public float particleSpeed = 1f;
    
    private Image backgroundImage;
    private RectTransform patternTransform;
    private Coroutine patternCoroutine;
    
    void Start()
    {
        // Check if this is a duplicate UIBackgroundManager
        UIBackgroundManager[] managers = GetComponents<UIBackgroundManager>();
        if (managers.Length > 1)
        {
            Destroy(this);
            return;
        }
        
        // Only create background if it doesn't already exist
        if (backgroundImage == null)
        {
            CreateBackground();
        }
    }
    
    void OnEnable()
    {
        // Only recreate background if it doesn't exist
        if (backgroundImage == null)
        {
            CreateBackground();
        }
        else
        {
            // Restart animation if background already exists
            RestartAnimation();
        }
    }
    
    void OnValidate()
    {
        // Update colors when they change in the inspector
        if (Application.isPlaying && backgroundImage != null)
        {
            UpdateBackgroundColors();
        }
    }
    
    public void UpdateBackgroundColors()
    {
        // Update gradient colors if gradient exists
        if (enableGradientBackground)
        {
            Transform gradientTop = backgroundImage.transform.Find("GradientTop");
            Transform gradientBottom = backgroundImage.transform.Find("GradientBottom");
            
            if (gradientTop != null)
            {
                gradientTop.GetComponent<Image>().color = topColor;
            }
            if (gradientBottom != null)
            {
                gradientBottom.GetComponent<Image>().color = bottomColor;
            }
        }
        
        // Update pattern color if pattern exists
        if (enableAnimatedPattern && patternTransform != null)
        {
            patternTransform.GetComponent<Image>().color = patternColor;
        }
        
        // Update particle colors if particles exist
        if (enableParticleEffect)
        {
            Transform particleEffect = transform.Find("ParticleEffect");
            if (particleEffect != null)
            {
                foreach (Transform particle in particleEffect)
                {
                    if (particle.name == "Particle")
                    {
                        particle.GetComponent<Image>().color = particleColor;
                    }
                }
            }
        }
    }
    
    void CreateBackground()
    {
        // Create main background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(transform, false);
        backgroundObj.transform.SetAsFirstSibling(); // Put it behind everything
        
        RectTransform backgroundRect = backgroundObj.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        backgroundImage = backgroundObj.AddComponent<Image>();
        
        // Ensure background renders behind other UI elements
        Canvas backgroundCanvas = backgroundObj.AddComponent<Canvas>();
        backgroundCanvas.overrideSorting = true;
        backgroundCanvas.sortingOrder = -1; // Render behind everything
        
        if (enableGradientBackground)
        {
            CreateGradientBackground();
        }
        else
        {
            backgroundImage.color = new Color(0.15f, 0.15f, 0.25f, 1f);
        }
        
        if (enableAnimatedPattern)
        {
            CreateAnimatedPattern();
        }
        
        if (enableParticleEffect)
        {
            CreateParticleEffect();
        }
    }
    
    void CreateGradientBackground()
    {
        // Create a simple gradient effect using multiple images
        GameObject gradientTop = new GameObject("GradientTop");
        gradientTop.transform.SetParent(backgroundImage.transform, false);
        
        RectTransform topRect = gradientTop.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 0.5f);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;
        
        Image topImage = gradientTop.AddComponent<Image>();
        topImage.color = topColor;
        
        GameObject gradientBottom = new GameObject("GradientBottom");
        gradientBottom.transform.SetParent(backgroundImage.transform, false);
        
        RectTransform bottomRect = gradientBottom.AddComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, 0.5f);
        bottomRect.offsetMin = Vector2.zero;
        bottomRect.offsetMax = Vector2.zero;
        
        Image bottomImage = gradientBottom.AddComponent<Image>();
        bottomImage.color = bottomColor;
    }
    
    void CreateAnimatedPattern()
    {
        // Clean up existing pattern first
        if (patternTransform != null)
        {
            if (patternCoroutine != null)
            {
                StopCoroutine(patternCoroutine);
                patternCoroutine = null;
            }
            DestroyImmediate(patternTransform.gameObject);
            patternTransform = null;
        }
        
        // Create new pattern
        GameObject patternObj = new GameObject("AnimatedPattern");
        patternObj.transform.SetParent(transform, false); // Put it directly under the UI manager
        patternObj.transform.SetAsFirstSibling(); // Put it behind everything but after background
        
        patternTransform = patternObj.AddComponent<RectTransform>();
        
        // Add Canvas to pattern with specific sorting order
        Canvas patternCanvas = patternObj.AddComponent<Canvas>();

        patternTransform.anchorMin = Vector2.zero;
        patternTransform.anchorMax = Vector2.one;
        patternTransform.offsetMin = Vector2.zero;
        patternTransform.offsetMax = Vector2.zero;
        
        Image patternImage = patternObj.AddComponent<Image>();
        patternImage.color = patternColor;
        patternImage.raycastTarget = false; // Don't block clicks
        
        
        // Start the pattern animation
        patternCoroutine = StartCoroutine(AnimatePattern());
    }
    
    void CreateParticleEffect()
    {
        GameObject particleObj = new GameObject("ParticleEffect");
        particleObj.transform.SetParent(transform, false); // Parent to main transform, not background
        
        RectTransform particleRect = particleObj.AddComponent<RectTransform>();
        particleRect.anchorMin = Vector2.zero;
        particleRect.anchorMax = Vector2.one;
        particleRect.offsetMin = Vector2.zero;
        particleRect.offsetMax = Vector2.zero;
        
        // Add Canvas to particles with higher sorting order to appear above pattern and background
        Canvas particleCanvas = particleObj.AddComponent<Canvas>();
        particleCanvas.overrideSorting = true;
        particleCanvas.sortingOrder = 1; // Above pattern (-1) and background (-1), below UI elements (2+)
        
        // Create floating particles
        for (int i = 0; i < 20; i++)
        {
            CreateFloatingParticle(particleObj.transform);
        }
    }
    
    void CreateFloatingParticle(Transform parent)
    {
        GameObject particle = new GameObject("Particle");
        particle.transform.SetParent(parent, false);
        
        RectTransform rect = particle.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(particleSize * 200, particleSize * 200);
        
        Image image = particle.AddComponent<Image>();
        image.color = particleColor;
        image.raycastTarget = false; // Don't block clicks
        
        // Create a simple circle sprite for the particle
        Texture2D circleTexture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        float radius = 15f;
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * 32 + x] = Color.white;
                }
                else
                {
                    pixels[y * 32 + x] = Color.clear;
                }
            }
        }
        
        circleTexture.SetPixels(pixels);
        circleTexture.Apply();
        
        Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        image.sprite = circleSprite;
        
        // Random position
        rect.anchoredPosition = new Vector2(
            Random.Range(-Screen.width/2, Screen.width/2),
            Random.Range(-Screen.height/2, Screen.height/2)
        );
        
        // Start floating animation
        StartCoroutine(FloatParticle(rect));
    }
    
    
    IEnumerator AnimatePattern()
    {
        while (true)
        {
            if (patternTransform != null)
            {
                float time = Time.time * patternSpeed;
                float scale = 1f + Mathf.Sin(time) * 0.1f * patternScale;
                patternTransform.localScale = Vector3.one * scale;
                
                // Rotate the pattern slightly
                patternTransform.rotation = Quaternion.Euler(0, 0, time * 10f);
            }
            else
            {
                break;
            }
            yield return null;
        }
    }
    
    IEnumerator FloatParticle(RectTransform particleRect)
    {
        Vector2 startPos = particleRect.anchoredPosition;
        float duration = Random.Range(3f, 8f) / particleSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (particleRect != null)
            {
                float progress = elapsed / duration;
                float yOffset = Mathf.Sin(progress * Mathf.PI * 2) * 50f;
                float xOffset = Mathf.Cos(progress * Mathf.PI * 2) * 30f;
                
                particleRect.anchoredPosition = startPos + new Vector2(xOffset, yOffset);
                
                // Fade in and out
                Image image = particleRect.GetComponent<Image>();
                if (image != null)
                {
                    float alpha = Mathf.Sin(progress * Mathf.PI) * 0.3f;
                    image.color = new Color(1f, 1f, 1f, alpha);
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restart the particle
        if (particleRect != null)
        {
            CreateFloatingParticle(particleRect.parent);
            Destroy(particleRect.gameObject);
        }
    }
    
    public void RefreshBackground()
    {
        // Prevent multiple simultaneous refreshes
        if (backgroundImage == null)
        {
            return; // Already refreshing or no background to refresh
        }
        
        // Stop existing pattern animation
        if (patternCoroutine != null)
        {
            StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }
        
        // Reset pattern transform reference
        patternTransform = null;
        
        // Destroy existing background
        if (backgroundImage != null)
        {
            DestroyImmediate(backgroundImage.gameObject);
            backgroundImage = null; // Clear reference immediately
        }
        
        // Recreate background
        CreateBackground();
    }
    
    public void RestartAnimation()
    {
        // Restart pattern animation
        if (patternTransform != null)
        {
            // Stop existing animation
            if (patternCoroutine != null)
            {
                StopCoroutine(patternCoroutine);
            }
            
            // Restart the pattern animation
            patternCoroutine = StartCoroutine(AnimatePattern());
        }
        
        // Restart particle animations
        Transform particleEffect = transform.Find("ParticleEffect");
        if (particleEffect != null)
        {
            // Stop all existing particle animations
            foreach (Transform particle in particleEffect)
            {
                if (particle.name == "Particle")
                {
                    // Restart particle animation
                    StartCoroutine(FloatParticle(particle.GetComponent<RectTransform>()));
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (patternCoroutine != null)
        {
            StopCoroutine(patternCoroutine);
        }
    }
}
