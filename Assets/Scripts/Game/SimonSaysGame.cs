using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimonSaysGame : MonoBehaviour
{
    [Header("Game Settings")]
    public float sequenceDelay = 1f;
    public float inputTimeout = 3f;
    
    [Header("Colors")]
    public Color redColor = Color.red;
    public Color blueColor = Color.blue;
    public Color greenColor = Color.green;
    public Color yellowColor = Color.yellow;
    
    private List<Color> sequence = new List<Color>();
    private List<Color> playerInput = new List<Color>();
    private bool isPlayingSequence = false;
    private bool isWaitingForInput = false;
    private bool isIdle = true; // New idle state
    private int currentInputIndex = 0;
    private bool waitingForOrbiter = false;
    private bool isProcessingInput = false; // Prevent multiple simultaneous clicks
    
    // Events
    public System.Action<Color> OnColorHighlight;
    public System.Action<Color> OnColorPressed;
    public System.Action<bool> OnSequenceComplete;
    public System.Action OnGameOver;
    public System.Action<int> OnLevelComplete;
    public System.Action OnGetReady; // New event for idle state
    public System.Action<Color> OnOrbiterMoveToColor; // New event for orbiter movement
    public System.Action OnOrbiterReachedTarget; // New event when orbiter reaches target
    public System.Action OnOrbiterShrink; // New event for orbiter shrinking
    public System.Action OnOrbiterGrow; // New event for orbiter growing
    
    public void StartNewGame()
    {
        // Stop any existing coroutines to prevent multiple running
        StopAllCoroutines();
        
        sequence.Clear();
        playerInput.Clear();
        currentInputIndex = 0;
        AddRandomColor();
        StartCoroutine(DelayStart());
    }

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(1f);
        isIdle = false; // Game is starting, no longer idle
        StartCoroutine(PlaySequence());
    }
    
    public void AddRandomColor()
    {
        Color[] colors = { redColor, blueColor, greenColor, yellowColor };
        // Color[] colors = { blueColor, greenColor };
        Color randomColor = colors[Random.Range(0, colors.Length)];
        sequence.Add(randomColor);
    }
    
    public void OnColorClicked(Color color)
    {
        if (!isWaitingForInput || isIdle || isProcessingInput) 
        {
            return;
        }
        
        // Check if currentInputIndex is within bounds
        if (currentInputIndex >= sequence.Count)
        {
            return;
        }
        
        // Set processing flag to prevent multiple clicks
        isProcessingInput = true;
        
        OnColorPressed?.Invoke(color);
        
        if (color == sequence[currentInputIndex])
        {
            playerInput.Add(color);
            currentInputIndex++;
            
            if (currentInputIndex >= sequence.Count)
            {
                // Sequence completed correctly
                OnSequenceComplete?.Invoke(true);
                StartCoroutine(NextLevel());
            }
            else
            {
                // Reset processing flag after a short delay to allow next input
                StartCoroutine(ResetProcessingFlagAfterDelay(0.1f));
            }
        }
        else
        {
            // Wrong color - game over
            OnSequenceComplete?.Invoke(false);
            OnGameOver?.Invoke();
        }
    }
    
    private IEnumerator PlaySequence()
    {
        isPlayingSequence = true;
        isWaitingForInput = false;
        isProcessingInput = false; // Reset processing flag for new sequence
        
        // Grow orbiter at the start of sequence
        OnOrbiterGrow?.Invoke();
        
        // Wait for orbiter to grow before starting sequence
        yield return new WaitForSeconds(0.5f); // Wait for grow animation
        
        // Create a copy of the sequence to avoid modification during iteration
        List<Color> sequenceCopy = new List<Color>(sequence);
        
        foreach (Color color in sequenceCopy)
        {
            
            // Wait for orbiter to reach target (only in AR mode)
            if (OnOrbiterMoveToColor != null)
            {
                waitingForOrbiter = true;
                OnOrbiterMoveToColor?.Invoke(color); // Trigger orbiter movement AFTER setting waitingForOrbiter
                
                yield return new WaitUntil(() => !waitingForOrbiter);
                
                // NOW highlight the color after orbiter has reached the target
                OnColorHighlight?.Invoke(color);
                
                // Brief pause after highlighting
                yield return new WaitForSeconds(0.5f);
                
                // Reset orbiter for next color
                ResetOrbiterForNextColor();
            }
            else
            {
                // In 2D mode, highlight immediately and use the original timing
                OnColorHighlight?.Invoke(color);
                yield return new WaitForSeconds(sequenceDelay);
            }
        }
        
        // Shrink orbiter after sequence is complete
        OnOrbiterShrink?.Invoke();
        
        // Wait for orbiter to shrink before ending sequence
        yield return new WaitForSeconds(0.5f); // Wait for shrink animation
        
        isPlayingSequence = false;
        isWaitingForInput = true;
        isProcessingInput = false; // Reset processing flag when sequence ends
        currentInputIndex = 0;
        playerInput.Clear();
    }
    
    private IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(1f);
        
        int currentLevel = GameManager.Instance.currentLevel;
        currentLevel++;
        GameManager.Instance.currentLevel = currentLevel;
        
        if (currentLevel > GameManager.Instance.maxLevel)
        {
            OnGameOver?.Invoke();
        }
        else
        {
            OnLevelComplete?.Invoke(currentLevel);
            AddRandomColor();
            StartCoroutine(PlaySequence());
        }
    }
    
    public bool IsPlayingSequence()
    {
        return isPlayingSequence;
    }
    
    public bool IsWaitingForInput()
    {
        return isWaitingForInput;
    }
    
    public bool IsIdle()
    {
        return isIdle;
    }
    
    public void SetIdle(bool idle)
    {
        isIdle = idle;
        if (isIdle)
        {
            OnGetReady?.Invoke();
        }
    }
    
    public void HandleOrbiterReachedTarget()
    {
        waitingForOrbiter = false;
    }
    
    private void ResetOrbiterForNextColor()
    {
        // Find the orbiter controller and reset it
        ARObjectSpawnerAnchored arSpawner = FindFirstObjectByType<ARObjectSpawnerAnchored>();
        if (arSpawner != null)
        {
            OrbiterController orbiterController = arSpawner.GetOrbiterController();
            if (orbiterController != null)
            {
                orbiterController.ResetOrbiter();
            }
        }
    }
    
    private System.Collections.IEnumerator ResetProcessingFlagAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isProcessingInput = false;
    }
}
