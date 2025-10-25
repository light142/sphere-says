using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    [Header("API Configuration")]
    public string apiEndpoint = "https://sphere-game-data-api.vercel.app/api/game-data/";
    public float retryDelay = 3f;
    public int maxQueueSize = 100;

    [Header("Performance")]
    public int maxConcurrentRequests = 5; // Allow up to 3 concurrent requests
    
    [Header("Debug")]
    public bool enableTelemetry = true;
    public bool debugMode = false;

    private Queue<TelemetryEvent> eventQueue = new Queue<TelemetryEvent>();
    private int activeRequests = 0; // Track concurrent requests
    private string sessionId;
    private string currentGameReference;
    private Coroutine processingCoroutine;

    // Event categories
    public const string CATEGORY_SESSION = "session";
    public const string CATEGORY_GAME = "game";
    public const string CATEGORY_UI = "ui";
    
    // Event types
    public const string SESSION_START = "session_start";
    public const string SESSION_RESUME = "session_resume";
    public const string SESSION_PAUSE = "session_pause";
    public const string INITIALISE_GAME = "initialise_game";
    public const string START_GAME = "start_game";
    public const string PLAYER_SELECT = "player_select";
    public const string SIMON_SELECT = "simon_select";
    public const string SIMON_SELECT_START = "simon_select_start";
    public const string SIMON_SELECT_END = "simon_select_end";
    public const string LEVEL_COMPLETE = "level_complete";
    public const string GAME_OVER = "game_over";
    public const string QUIT_APPLICATION = "quit_application";
    public const string RESTART_GAME = "restart_game";
    public const string PLAY_AGAIN = "play_again";
    public const string BACK_TO_MENU = "back_to_menu";
    public const string SHOW_BANNER_2D = "show_banner_2d";
    public const string SHOW_BANNER_AR = "show_banner_ar";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (enableTelemetry)
        {
            StartProcessing();
            // Send session start event
            TrackEvent(SESSION_START);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (enableTelemetry)
        {
            if (pauseStatus)
            {
                // App paused
                TrackEvent(SESSION_PAUSE);
            }
            else
            {
                // App resumed
                TrackEvent(SESSION_RESUME);
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (enableTelemetry)
        {
            if (hasFocus)
            {
                // App gained focus
                TrackEvent(SESSION_RESUME);
            }
            else
            {
                // App lost focus
                TrackEvent(SESSION_PAUSE);
            }
        }
    }

    private void InitializeSession()
    {
        sessionId = Guid.NewGuid().ToString();
        if (debugMode)
        {
            Debug.Log($"[Telemetry] New session started: {sessionId}");
        }
    }

    public void TrackEvent(string eventType, Color? color = null, List<Color> sequence = null, List<Color> playerInput = null, Color? correctColor = null)
    {
        if (!enableTelemetry) return;

        try
        {
            // Only include game_reference and game_level for events that should have one
            string gameReference = ShouldHaveGameReference(eventType) ? currentGameReference : null;
            int gameLevel = ShouldHaveGameLevel(eventType) ? (GameManager.Instance?.currentLevel ?? 1) : 0;
            string eventCategory = GetEventCategory(eventType);
            
            var telemetryEvent = new TelemetryEvent(
                eventType,
                eventCategory,
                sessionId,
                GameManager.Instance?.currentGameMode.ToString() ?? "None",
                gameLevel,
                gameReference
            );

            if (color.HasValue)
            {
                telemetryEvent.SetColor(color.Value);
            }

            if (sequence != null)
            {
                telemetryEvent.SetSequence(sequence);
            }

            if (playerInput != null)
            {
                telemetryEvent.SetPlayerInput(playerInput);
            }
            
            // Set correct color (only for player_select events)
            if (correctColor.HasValue)
            {
                telemetryEvent.SetCorrectColor(correctColor.Value);
            }

            EnqueueEvent(telemetryEvent);

            if (debugMode)
            {
                Debug.Log($"[Telemetry] Event tracked: {eventType}");
            }
        }
        catch (Exception e)
        {
            if (debugMode)
            {
                Debug.LogError($"[Telemetry] Error tracking event {eventType}: {e.Message}");
            }
        }
    }

    private bool ShouldHaveGameReference(string eventType)
    {
        // Events that should have game_reference (game-related events)
        return eventType == INITIALISE_GAME ||
               eventType == START_GAME ||
               eventType == PLAYER_SELECT ||
               eventType == SIMON_SELECT ||
               eventType == SIMON_SELECT_START ||
               eventType == SIMON_SELECT_END ||
               eventType == LEVEL_COMPLETE ||
               eventType == GAME_OVER ||
               eventType == RESTART_GAME ||
               eventType == PLAY_AGAIN ||
               eventType == BACK_TO_MENU;
    }

    private bool ShouldHaveGameLevel(string eventType)
    {
        // Events that should have game_level = current level (active game)
        return eventType == START_GAME ||
               eventType == PLAYER_SELECT ||
               eventType == SIMON_SELECT ||
               eventType == SIMON_SELECT_START ||
               eventType == SIMON_SELECT_END ||
               eventType == LEVEL_COMPLETE ||
               eventType == RESTART_GAME ||
               eventType == PLAY_AGAIN;
    }
    
    private string GetEventCategory(string eventType)
    {
        switch (eventType)
        {
            // Session events
            case SESSION_START:
            case SESSION_RESUME:
            case SESSION_PAUSE:
                return CATEGORY_SESSION;
            
            // Game events (lifecycle + gameplay)
            case INITIALISE_GAME:
            case START_GAME:
            case LEVEL_COMPLETE:
            case GAME_OVER:
            case PLAYER_SELECT:
            case SIMON_SELECT:
            case SIMON_SELECT_START:
            case SIMON_SELECT_END:
            case RESTART_GAME:
                return CATEGORY_GAME;
            
            // UI events
            case QUIT_APPLICATION:
            case BACK_TO_MENU:
            case PLAY_AGAIN:
                return CATEGORY_UI;
            
            default:
                return "unknown";
        }
    }

    private void EnqueueEvent(TelemetryEvent telemetryEvent)
    {
        lock (eventQueue)
        {
            if (eventQueue.Count >= maxQueueSize)
            {
                // Remove oldest event to make room
                eventQueue.Dequeue();
            }
            eventQueue.Enqueue(telemetryEvent);
        }
    }

    private void StartProcessing()
    {
        if (processingCoroutine == null)
        {
            processingCoroutine = StartCoroutine(ProcessEventQueue());
        }
    }

    private void StopProcessing()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
            processingCoroutine = null;
        }
    }

    private IEnumerator ProcessEventQueue()
    {
        while (true)
        {
            // Process multiple events concurrently (up to maxConcurrentRequests)
            while (activeRequests < maxConcurrentRequests)
            {
                TelemetryEvent eventToSend = null;

                lock (eventQueue)
                {
                    if (eventQueue.Count > 0)
                    {
                        eventToSend = eventQueue.Dequeue();
                    }
                }

                if (eventToSend != null)
                {
                    StartCoroutine(SendEventOnce(eventToSend));
                }
                else
                {
                    break; // No more events to process
                }
            }

            // Wait a bit before checking again
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator SendEventOnce(TelemetryEvent telemetryEvent)
    {
        activeRequests++; // Increment active request counter
        
        if (debugMode)
        {
            Debug.Log($"[Telemetry] Sending event (attempt {telemetryEvent.retry_count + 1}): {telemetryEvent.event_type} (Active: {activeRequests})");
        }

        yield return StartCoroutine(SendEvent(telemetryEvent, (result, error) => {
            if (!result)
            {
                // Increment retry count only when the event fails
                telemetryEvent.IncrementRetryCount();
                
                if (!string.IsNullOrEmpty(error))
                {
                    telemetryEvent.AddErrorMessage(error);
                }
                telemetryEvent.AddErrorMessage($"Retry attempt {telemetryEvent.retry_count} failed, re-queuing for retry");
                
                // Re-queue the failed event for retry
                lock (eventQueue)
                {
                    if (eventQueue.Count < maxQueueSize)
                    {
                        eventQueue.Enqueue(telemetryEvent);
                        if (debugMode)
                        {
                            Debug.Log($"[Telemetry] Event re-queued for retry: {telemetryEvent.event_type}");
                        }
                    }
                    else
                    {
                        if (debugMode)
                        {
                            Debug.LogWarning($"[Telemetry] Queue full, dropping event: {telemetryEvent.event_type}");
                        }
                    }
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.Log($"[Telemetry] Event sent successfully after {telemetryEvent.retry_count} attempts: {telemetryEvent.event_type}");
                }
            }
        }));
        
        activeRequests--; // Decrement active request counter
        
        // Add delay after processing (only for retries)
        if (telemetryEvent.retry_count > 0)
        {
            if (debugMode)
            {
                Debug.Log($"[Telemetry] Waiting {retryDelay} seconds before next attempt");
            }
            yield return new WaitForSeconds(retryDelay);
        }
    }

    private IEnumerator SendEvent(TelemetryEvent telemetryEvent, System.Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(telemetryEvent);
        
        if (debugMode)
        {
            Debug.Log($"[Telemetry] Sending JSON: {json}");
        }

        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 5; // Reduced from 10s to 5s for faster timeouts

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            string errorMessage = success ? null : $"{request.result}: {request.error}";
            
            if (debugMode)
            {
                if (success)
                {
                    Debug.Log($"[Telemetry] Event sent successfully: {telemetryEvent.event_type}");
                }
                else
                {
                    Debug.LogError($"[Telemetry] Failed to send event: {request.error}");
                }
            }

            callback?.Invoke(success, errorMessage);
        }
    }

    void OnDestroy()
    {
        StopProcessing();
    }

    void OnApplicationQuit()
    {
        if (enableTelemetry)
        {
            // Just queue the quit event - don't try to send it
            TrackEvent(QUIT_APPLICATION);
            
            if (debugMode)
            {
                Debug.Log($"[Telemetry] App quitting - {eventQueue.Count} events queued");
            }
        }
        
        // Clean shutdown - don't block
        StopProcessing();
    }


    // Public methods for easy access
    public void TrackSessionStart() => TrackEvent(SESSION_START);
    public void TrackInitialiseGame() => TrackEvent(INITIALISE_GAME);
    public void TrackStartGame() 
    {
        // Generate new game reference every time StartNewGame() is called
        currentGameReference = GenerateGameReference();
        TrackEvent(START_GAME);
    }
    public void TrackPlayerSelect(Color color, List<Color> sequence = null, List<Color> playerInput = null, Color? correctColor = null) => TrackEvent(PLAYER_SELECT, color, sequence, playerInput, correctColor);
    public void TrackSimonSelect(Color color, List<Color> sequence = null) => TrackEvent(SIMON_SELECT, color, sequence);
    public void TrackSimonSelectStart(Color color, List<Color> sequence = null) => TrackEvent(SIMON_SELECT_START, color, sequence);
    public void TrackSimonSelectEnd(Color color, List<Color> sequence = null) => TrackEvent(SIMON_SELECT_END, color, sequence);
    public void TrackLevelComplete(List<Color> sequence = null, List<Color> playerInput = null) => TrackEvent(LEVEL_COMPLETE, null, sequence, playerInput);
    public void TrackGameOver(List<Color> sequence = null, List<Color> playerInput = null) => TrackEvent(GAME_OVER, null, sequence, playerInput);
    public void TrackQuitApplication() => TrackEvent(QUIT_APPLICATION);
    public void TrackRestartGame() => TrackEvent(RESTART_GAME);
    public void TrackPlayAgain() => TrackEvent(PLAY_AGAIN);
    public void TrackBackToMenu() => TrackEvent(BACK_TO_MENU);
    public void TrackShowBanner2D() => TrackEvent(SHOW_BANNER_2D);
    public void TrackShowBannerAR() => TrackEvent(SHOW_BANNER_AR);

    private string GenerateGameReference()
    {
        // Generate a unique game reference using timestamp and random component
        return $"game_{DateTime.UtcNow:yyyyMMddHHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
    }
}
