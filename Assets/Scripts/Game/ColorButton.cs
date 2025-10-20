using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Settings")]
    public Color buttonColor;
    public Color highlightColor = Color.white;
    public float highlightDuration = 0.5f; // Slightly longer for better visibility
    
    [Header("Audio Settings")]
    public bool enableAudio = true;
    
    [Header("References")]
    public Image buttonImage;
    public Button button;
    
    private Color originalColor;
    private bool isHighlighted = false;
    
    void Start()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        
        if (button == null)
            button = GetComponent<Button>();
            
        originalColor = buttonImage.color;
        buttonImage.color = buttonColor;
        
        // Disable default button color change effects
        if (button != null)
        {
            ColorBlock colors = button.colors;
            // Set all color states to the same color to disable visual feedback
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            // Disable color transitions completely
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            
            // Add click listener to button
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    void Update()
    {
        // No longer updating button interactability - using click event filtering instead
    }
    
    public void OnButtonClicked()
    {
        if (GameManager.Instance.currentGameMode == GameManager.GameMode.Mode2D)
        {
            SimonSaysGame simonGame = FindFirstObjectByType<SimonSaysGame>();
            if (simonGame != null)
            {
                // Only allow clicks when:
                // - Game is not idle (started)
                // - Not playing sequence (Simon's turn is over)
                // - Waiting for input (player's turn)
                if (!simonGame.IsIdle() && !simonGame.IsPlayingSequence() && simonGame.IsWaitingForInput())
                {
                    
                    // Play audio note for this color
                    PlayColorAudio();
                    
                    simonGame.OnColorClicked(buttonColor);
                }
                else
                {
                    // Don't do anything - just ignore the click
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Keep this for compatibility, but use OnButtonClicked instead
        OnButtonClicked();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        SimonSaysGame simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (simonGame != null)
        {
            // Only allow clicks when:
            // - Game is not idle (started)
            // - Not playing sequence (Simon's turn is over)
            // - Waiting for input (player's turn)
            if (!simonGame.IsIdle() && !simonGame.IsPlayingSequence() && simonGame.IsWaitingForInput())
            {
                HighlightButton();
            }
        }
    }

    
    public void OnPointerUp(PointerEventData eventData)
    {
        // Highlight will be handled by the game logic
    }
    
    public void HighlightButton()
    {
        if (isHighlighted) return;
        
        isHighlighted = true;
        // Always allow highlighting regardless of button state (for Simon's sequence)
        if (buttonImage != null)
        {
            // Use bright white for clear visibility
            buttonImage.color = Color.white;
        }
        
        Invoke(nameof(UnhighlightButton), highlightDuration);
    }
    
    public void UnhighlightButton()
    {
        isHighlighted = false;
        // Always return to normal button color
        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;
        }
    }
    
    public void SetColor(Color color)
    {
        buttonColor = color;
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }
    
    public void ResetButtonAppearance()
    {
        // Reset button to normal appearance
        isHighlighted = false;
        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;
        }
        // Don't touch button.interactable - leave it as is
    }
    
    
    void PlayColorAudio()
    {
        if (enableAudio && AudioManager.Instance != null)
        {
            // Use 2D audio for 2D mode
            AudioManager.Instance.PlayColorNote(buttonColor);
        }
    }
}
