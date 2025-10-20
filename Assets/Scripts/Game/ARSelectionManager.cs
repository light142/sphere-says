using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARSelectionManager : MonoBehaviour
{
    [Header("Selection Settings")]
    public float selectionDistance = 5f;
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
            selectButtonText.text = "SELECT";
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
            selectButtonText.color = Color.black; // Default black text
            selectButtonText.outlineColor = Color.white;
            selectButtonText.outlineWidth = 0.1f;
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
        
        // Also update button text color for better contrast
        if (selectButtonText != null)
        {
            // Use white text for better contrast against colored background
            selectButtonText.color = Color.white;
            selectButtonText.outlineColor = Color.black;
            selectButtonText.outlineWidth = 0.2f;
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
