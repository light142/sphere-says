using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARSelectionManager : MonoBehaviour
{
    [Header("Selection Settings")]
    public float selectionDistance = 10f;
    public float selectionAngle = 30f; // Degrees from center
    public Color selectionHighlightColor = Color.black; // For player targeting
    
    [Header("UI References")]
    public Button selectButton;
    public TextMeshProUGUI selectButtonText;
    
    private Camera arCamera;
    private ARColorSphere currentTarget;
    private ARColorSphere selectedSphere;
    private bool selectionDisabled = false;
    
    void Start()
    {
        arCamera = Camera.main;
        
        // Setup Select button
        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(false);
            selectButton.onClick.AddListener(SelectCurrentTarget);
        }
        
        if (selectButtonText != null)
        {
            selectButtonText.text = "THAT ONE!";
            selectButtonText.color = Color.white; // Clean white text
            selectButtonText.fontStyle = FontStyles.Bold;
            selectButtonText.outlineColor = Color.black; // Black outline for contrast
            selectButtonText.outlineWidth = 0.5f; // Thicker outline for better visibility
            
            // Start pulsing animation
            StartCoroutine(PulseSelectText());
        }
    }
    
    void Update()
    {
        if (arCamera == null) return;
        
        // Don't allow selection if disabled, during sequence playback, or when game is idle
        SimonSaysGame simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (selectionDisabled || (simonGame != null && (simonGame.IsPlayingSequence() || simonGame.IsIdle())))
        {
            // Clear any current targeting during sequence, when idle, or when disabled
            if (currentTarget != null)
            {
                currentTarget.UnhighlightSphere();
                currentTarget = null;
            }
            HideSelectButton();
            return;
        }
        
        // Perform raycast selection
        ARColorSphere newTarget = GetTargetedSphere();
        
        // Update selection if target changed
        if (newTarget != currentTarget)
        {
            // Clear previous target highlight
            if (currentTarget != null)
            {
                currentTarget.UnhighlightSphere();
            }
            
            // Set new target
            currentTarget = newTarget;
            
            // Highlight new target if found
            if (currentTarget != null)
            {
                // Use new lighting-based player selection
                currentTarget.SetPlayerSelection(true);
                ShowSelectButton();
                
                // Update button color to match the new target
                UpdateSelectButtonColor(currentTarget.sphereColor);
            }
            else
            {
                HideSelectButton();
            }
        }
    }
    
    ARColorSphere GetTargetedSphere()
    {
        // Get all AR spheres in scene
        ARColorSphere[] spheres = FindObjectsByType<ARColorSphere>(FindObjectsSortMode.None);
        ARColorSphere bestTarget = null;
        float bestScore = 0f;
        
        foreach (ARColorSphere sphere in spheres)
        {
            // Skip if already selected
            if (sphere == selectedSphere) continue;
            
            float score = CalculateSelectionScore(sphere);
            if (score > bestScore && score > 0.5f) // Minimum threshold
            {
                bestScore = score;
                bestTarget = sphere;
            }
        }
        
        return bestTarget;
    }
    
    float CalculateSelectionScore(ARColorSphere sphere)
    {
        Vector3 cameraPos = arCamera.transform.position;
        Vector3 cameraForward = arCamera.transform.forward;
        Vector3 spherePos = sphere.transform.position;
        
        // Calculate distance
        float distance = Vector3.Distance(cameraPos, spherePos);
        if (distance > selectionDistance) return 0f;
        
        // Calculate angle from camera center
        Vector3 directionToSphere = (spherePos - cameraPos).normalized;
        float angle = Vector3.Angle(cameraForward, directionToSphere);
        if (angle > selectionAngle) return 0f;
        
        // Calculate score (closer and more centered = higher score)
        float distanceScore = 1f - (distance / selectionDistance);
        float angleScore = 1f - (angle / selectionAngle);
        
        return (distanceScore + angleScore) / 2f;
    }
    
    void ShowSelectButton()
    {
        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(true);
            
            // Change button color to match the targeted sphere
            if (currentTarget != null)
            {
                UpdateSelectButtonColor(currentTarget.sphereColor);
            }
        }
    }
    
    void HideSelectButton()
    {
        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(false);
            // Reset button to default appearance when hidden
            ResetSelectButtonAppearance();
        }
    }
    
    void ResetSelectButtonAppearance()
    {
        if (selectButton == null) return;
        
        // Reset button background to default color
        Image buttonImage = selectButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = Color.white; // Default white background
        }
        
        // Reset text to default appearance
        if (selectButtonText != null)
        {
            selectButtonText.color = Color.white; // Clean white text
            selectButtonText.outlineColor = Color.black; // Black outline for contrast
            selectButtonText.outlineWidth = 0.5f; // Thicker outline for better visibility
        }
    }
    
    void UpdateSelectButtonColor(Color sphereColor)
    {
        if (selectButton == null) return;
        
        // Get the button's Image component
        Image buttonImage = selectButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Set button background color to exactly match sphere color
            // Use the exact same color without any modifications
            buttonImage.color = sphereColor;
            
        }
        
        // Also update button text color for clean white with black outline
        if (selectButtonText != null)
        {
            selectButtonText.color = Color.white; // Clean white text
            selectButtonText.outlineColor = Color.black; // Black outline for contrast
            selectButtonText.outlineWidth = 0.5f; // Thicker outline for better visibility
        }
    }
    
    Color GetContrastingTextColor(Color sphereColor)
    {
        // Create darker, contrasting text colors based on sphere color
        if (IsColorSimilar(sphereColor, Color.red))
        {
            return new Color(0.8f, 0.8f, 0.1f, 1f); // Dark yellow for red spheres
        }
        else if (IsColorSimilar(sphereColor, Color.blue))
        {
            return new Color(0.8f, 0.5f, 0.1f, 1f); // Dark orange for blue spheres
        }
        else if (IsColorSimilar(sphereColor, Color.green))
        {
            return new Color(0.8f, 0.1f, 0.5f, 1f); // Dark pink for green spheres
        }
        else if (IsColorSimilar(sphereColor, Color.yellow))
        {
            return new Color(0.5f, 0.1f, 0.5f, 1f); // Dark purple for yellow spheres
        }
        else
        {
            return new Color(0.8f, 0.7f, 0.1f, 1f); // Default dark yellow
        }
    }
    
    Color GetContrastingOutlineColor(Color sphereColor)
    {
        // Create darker contrasting outline colors based on sphere color
        if (IsColorSimilar(sphereColor, Color.red))
        {
            return new Color(0.1f, 0.5f, 0.1f, 1f); // Dark green outline for red spheres
        }
        else if (IsColorSimilar(sphereColor, Color.blue))
        {
            return new Color(0.5f, 0.1f, 0.1f, 1f); // Dark red outline for blue spheres
        }
        else if (IsColorSimilar(sphereColor, Color.green))
        {
            return new Color(0.1f, 0.1f, 0.5f, 1f); // Dark blue outline for green spheres
        }
        else if (IsColorSimilar(sphereColor, Color.yellow))
        {
            return new Color(0.1f, 0.5f, 0.5f, 1f); // Dark cyan outline for yellow spheres
        }
        else
        {
            return new Color(0.5f, 0.1f, 0.1f, 1f); // Default dark red outline
        }
    }
    
    bool IsColorSimilar(Color color1, Color color2)
    {
        // Check if two colors are similar (within a threshold)
        float threshold = 0.3f;
        return Mathf.Abs(color1.r - color2.r) < threshold &&
               Mathf.Abs(color1.g - color2.g) < threshold &&
               Mathf.Abs(color1.b - color2.b) < threshold;
    }
    
    System.Collections.IEnumerator PulseSelectText()
    {
        if (selectButtonText == null) yield break;
        
        float baseScale = 1f;
        float pulseScale = 1.1f; // 10% larger
        float pulseSpeed = 2f; // 2 pulses per second
        float pulseIntensity = 0.5f; // Moderate pulsing
        
        while (selectButtonText != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
            float currentScale = Mathf.Lerp(baseScale, pulseScale, (pulse - 1f) * 0.5f);
            
            selectButtonText.transform.localScale = Vector3.one * currentScale;
            
            yield return null;
        }
    }
    
    public void SelectCurrentTarget()
    {
        if (currentTarget == null || selectionDisabled) return;
        
        // Clear previous selection
        if (selectedSphere != null)
        {
            selectedSphere.UnhighlightSphere();
        }
        
        // Set new selection
        selectedSphere = currentTarget;
        
        // Play audio through the sphere's method
        selectedSphere.PlaySelectionAudio();
        
        // Hide select button
        HideSelectButton();
        
        // Send selection to game
        SendSelectionToGame();
    }
    
    void SendSelectionToGame()
    {
        if (selectedSphere == null) return;
        
        // Find Simon Says game and send selection
        SimonSaysGame simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (simonGame != null && simonGame.IsWaitingForInput() && !simonGame.IsIdle())
        {
            simonGame.OnColorClicked(selectedSphere.sphereColor);
            
            // Clear selection after sending to game
            StartCoroutine(ClearSelectionAfterDelay(0.5f));
        }
        else
        {
            // Game not ready for input or is idle, clear selection
            ClearSelection();
        }
    }
    
    System.Collections.IEnumerator ClearSelectionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearSelection();
    }
    
    public void ClearSelection()
    {
        if (selectedSphere != null)
        {
            selectedSphere.SetPlayerSelection(false);
            selectedSphere = null;
        }
        
        if (currentTarget != null)
        {
            currentTarget.SetPlayerSelection(false);
            currentTarget = null;
        }
        
        HideSelectButton();
    }
    
    public void ClearAllSelections()
    {
        // Clear all AR spheres in the scene
        ARColorSphere[] allSpheres = FindObjectsByType<ARColorSphere>(FindObjectsSortMode.None);
        foreach (ARColorSphere sphere in allSpheres)
        {
            sphere.SetPlayerSelection(false);
        }
        
        // Clear current selections
        ClearSelection();
        
    }
    
    public bool HasSelection()
    {
        return selectedSphere != null;
    }
    
    public ARColorSphere GetSelectedSphere()
    {
        return selectedSphere;
    }
    
    public void DisableSelection()
    {
        selectionDisabled = true;
        ClearSelection();
    }
    
    public void EnableSelection()
    {
        selectionDisabled = false;
    }
    
    public bool IsSelectionDisabled()
    {
        return selectionDisabled;
    }
}
