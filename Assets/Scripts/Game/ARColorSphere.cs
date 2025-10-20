using UnityEngine;

public class ARColorSphere : MonoBehaviour
{
    [Header("Sphere Settings")]
    public Color sphereColor;
    public Color highlightColor = Color.white;
    public float highlightDuration = 0.3f;
    
    [Header("References")]
    public Renderer sphereRenderer;
    public Collider sphereCollider;
    
    [Header("Lighting Effects")]
    public Light pointLight;
    public float lightIntensity = 2f;
    public float highlightLightIntensity = 5f;
    public Color lightColor = Color.white;
    
    [Header("Alternative Visual Effects")]
    public ParticleSystem highlightParticles;
    public ParticleSystem selectionParticles;
    public float pulseScale = 1.2f;
    public float pulseSpeed = 3f;
    public Color emissionColor = Color.white;
    public float emissionIntensity = 0.5f;
    
    [Header("Audio Effects")]
    public bool enableAudio = true;
    public float audioVolume = 0.7f;
    
    private Color originalColor;
    private bool isHighlighted = false;
    private Vector3 originalScale;
    private Material sphereMaterial;
    private bool isPulsing = false;
    
    void Start()
    {
        if (sphereRenderer == null)
            sphereRenderer = GetComponent<Renderer>();
        
        if (sphereCollider == null)
            sphereCollider = GetComponent<Collider>();
        
        originalColor = sphereRenderer.material.color;
        sphereRenderer.material.color = sphereColor;
        
        // Add basic collider interaction
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
        
        // Setup lighting effects
        SetupLighting();
        
        // Setup alternative visual effects
        SetupAlternativeEffects();
    }
    
    void SetupLighting()
    {
        // Create point light if it doesn't exist
        if (pointLight == null)
        {
            GameObject lightObj = new GameObject("PointLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = lightColor;
            pointLight.intensity = lightIntensity;
            pointLight.range = 2f;
            pointLight.enabled = false; // Start disabled
            
        }
    }
    
    public void HighlightSphere()
    {
        HighlightSphere(true); // Default to auto-unhighlight
    }
    
    public void HighlightSphere(bool autoUnhighlight)
    {
        if (isHighlighted) return;
        
        isHighlighted = true;
        
        // Use multiple visual effects for better visibility
        // StartPulsingEffect();
        // StartParticleEffect(highlightParticles);
        // SetEmissionEffect(true);
        
        // Play audio note for this color
        PlayColorAudio();
        
        // Keep lighting as backup
        if (pointLight != null)
        {
            pointLight.enabled = true;
            pointLight.intensity = highlightLightIntensity;
            pointLight.color = highlightColor;
        }
        
        
        if (autoUnhighlight)
        {
            // Auto-unhighlight after duration (for Simon sequence)
            Invoke(nameof(UnhighlightSphere), highlightDuration);
        }
        // If autoUnhighlight is false, highlight stays (for player selection)
    }
    
    public void UnhighlightSphere()
    {
        isHighlighted = false;
        
        // Stop all visual effects
        StopPulsingEffect();
        StopParticleEffect(highlightParticles);
        SetEmissionEffect(false);
        
        // Disable lighting effect
        if (pointLight != null)
        {
            pointLight.enabled = false;
        }
    }
    
    public void SetHighlightColor(Color color)
    {
        highlightColor = color;
        if (isHighlighted && pointLight != null)
        {
            // Only update lighting color, not sphere color
            pointLight.color = highlightColor;
        }
    }
    
    public void SetColor(Color color)
    {
        sphereColor = color;
        if (sphereRenderer != null)
        {
            sphereRenderer.material.color = color;
        }
    }
    
    // Special method for player selection (different from Simon's highlighting)
    public void SetPlayerSelection(bool selected)
    {
        if (selected)
        {
            // Player selection - subtle effects (no audio on raycast/hover)
            // StartParticleEffect(selectionParticles);
            // SetEmissionEffect(true, Color.white);
            
            // Keep lighting as backup
            if (pointLight != null)
            {
                pointLight.enabled = true;
                pointLight.intensity = lightIntensity;
                pointLight.color = Color.yellow;
            }
            
        }
        else
        {
            // Remove player selection
            StopParticleEffect(selectionParticles);
            SetEmissionEffect(false);
            
            if (pointLight != null)
            {
                pointLight.enabled = false;
            }
            
        }
    }
    
    // Alternative visual effects methods
    void SetupAlternativeEffects()
    {
        // Store original scale
        originalScale = transform.localScale;
        
        // Get sphere material for emission effects
        if (sphereRenderer != null)
        {
            sphereMaterial = sphereRenderer.material;
        }
        
        // Create particle systems if they don't exist
        CreateParticleSystems();
    }
    
    void CreateParticleSystems()
    {
        // Create highlight particles (for Simon's sequence)
        if (highlightParticles == null)
        {
            GameObject highlightParticleObj = new GameObject("HighlightParticles");
            highlightParticleObj.transform.SetParent(transform);
            highlightParticleObj.transform.localPosition = Vector3.zero;
            
            highlightParticles = highlightParticleObj.AddComponent<ParticleSystem>();
            SetupParticleSystem(highlightParticles, Color.white, 20f);
        }
        
        // Create selection particles (for player targeting)
        if (selectionParticles == null)
        {
            GameObject selectionParticleObj = new GameObject("SelectionParticles");
            selectionParticleObj.transform.SetParent(transform);
            selectionParticleObj.transform.localPosition = Vector3.zero;
            
            selectionParticles = selectionParticleObj.AddComponent<ParticleSystem>();
            SetupParticleSystem(selectionParticles, Color.yellow, 10f);
        }
    }
    
    void SetupParticleSystem(ParticleSystem ps, Color color, float emissionRate)
    {
        var main = ps.main;
        main.startColor = color;
        main.startSize = 0.1f;
        main.startLifetime = 1f;
        main.maxParticles = 50;
        
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        ps.Stop();
    }
    
    void StartPulsingEffect()
    {
        isPulsing = true;
        StartCoroutine(PulsingCoroutine());
    }
    
    void StopPulsingEffect()
    {
        isPulsing = false;
        transform.localScale = originalScale;
    }
    
    System.Collections.IEnumerator PulsingCoroutine()
    {
        while (isPulsing)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            float scale = Mathf.Lerp(1f, pulseScale, pulse);
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }
    
    void StartParticleEffect(ParticleSystem ps)
    {
        if (ps != null)
        {
            ps.Play();
        }
    }
    
    void StopParticleEffect(ParticleSystem ps)
    {
        if (ps != null)
        {
            ps.Stop();
        }
    }
    
    void SetEmissionEffect(bool enabled, Color? color = null)
    {
        if (sphereMaterial != null)
        {
            if (enabled)
            {
                Color emissionColorToUse = color ?? emissionColor;
                sphereMaterial.EnableKeyword("_EMISSION");
                sphereMaterial.SetColor("_EmissionColor", emissionColorToUse * emissionIntensity);
            }
            else
            {
                sphereMaterial.DisableKeyword("_EMISSION");
                sphereMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
    }
    
    void PlayColorAudio()
    {
        if (enableAudio && AudioManager.Instance != null)
        {
            // Use spatial audio for AR - sound comes from sphere's position
            AudioManager.Instance.PlayColorNoteSpatial(sphereColor, transform.position);
        }
    }
    
    public void PlaySelectionAudio()
    {
        // Play audio when sphere is selected by player
        if (enableAudio && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayColorNoteSpatial(sphereColor, transform.position);
        }
    }
}
