using UnityEngine;

public class AROrbitSphere : MonoBehaviour
{
    [Header("Sphere Settings")]
    public Color sphereColor;
    public Color highlightColor = Color.white;
    public float highlightDuration = 0.3f;
    
    [Header("References")]
    public Renderer sphereRenderer;
    public Collider sphereCollider;
    
    private Color originalColor;
    private bool isHighlighted = false;
    
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
    }
    
    public void HighlightOrbiter()
    {
        HighlightOrbiter(true); // Default to auto-unhighlight
    }
    
    public void HighlightOrbiter(bool autoUnhighlight)
    {
        if (isHighlighted) return;
        
        isHighlighted = true;
        sphereRenderer.material.color = highlightColor;
        
        if (autoUnhighlight)
        {
            // Auto-unhighlight after duration (for Simon sequence)
            Invoke(nameof(UnhighlightOrbiter), highlightDuration);
        }
        // If autoUnhighlight is false, highlight stays (for player selection)
    }
    
    public void UnhighlightOrbiter()
    {
        isHighlighted = false;
        sphereRenderer.material.color = sphereColor;
    }
    
    public void SetColor(Color color)
    {
        sphereColor = color;
        if (sphereRenderer != null)
        {
            sphereRenderer.material.color = color;
        }
    }
}
