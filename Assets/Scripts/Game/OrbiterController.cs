using UnityEngine;
using System.Collections;

public class OrbiterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float orbitSpeed = 1f; // Speed of orbital movement
    public float approachSpeed = 0.6f; // Speed when approaching target
    public float orbitRadius = 1f; // Distance to maintain from player initial position
    
    [Header("Orbit Settings")]
    public Vector3 playerInitialPosition; // Player's position when game started
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color approachingColor = Color.white;
    public Color reachedColor = Color.gray;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.3f;
    
    [Header("Scaling Settings")]
    public float shrinkDuration = 0.5f;
    public float growDuration = 0.5f;
    public float minScale = 0f; // Completely invisible (scale 0)
    
    private Transform targetSphere;
    private bool isOrbiting = false;
    private bool hasReachedTarget = false;
    private Vector3 orbitCenter;
    private float orbitAngle = 0f;
    private Renderer orbiterRenderer;
    private AROrbitSphere orbitSphereComponent;
    private bool isPulsing = false;
    private Color baseReachedColor;
    private Vector3 originalScale; // Store the original scale when spawned
    
    // Orbit state
    private float targetOrbitAngle = 0f;
    private bool isMovingToOrbitPosition = false;
    
    // Events
    public System.Action<Transform> OnReachedTarget;
    public System.Action OnStartedOrbiting;
    public System.Action OnReachedTargetSimple; // Simple event without parameters
    
    void Start()
    {
        orbiterRenderer = GetComponent<Renderer>();
        orbitSphereComponent = GetComponent<AROrbitSphere>();
        
        // Store the original scale when spawned
        originalScale = transform.localScale;
        
        if (orbiterRenderer != null)
        {
            orbiterRenderer.material.color = normalColor;
        }
    }
    
    private void InitializeOrbitPosition()
    {
        // Set orbit center to player's initial position (already at correct height)
        orbitCenter = playerInitialPosition;
        
        // Calculate current angle based on orbiter's current position (in front of player)
        Vector3 currentDirection = (transform.position - orbitCenter).normalized;
        orbitAngle = Mathf.Atan2(currentDirection.z, currentDirection.x);
        
    }
    
    public void InitializeAfterSpawn()
    {
        // This method should be called after the orbiter is spawned and playerInitialPosition is set
        if (playerInitialPosition != Vector3.zero)
        {
            InitializeOrbitPosition();
        }
    }
    
    void Update()
    {
        if (isOrbiting && targetSphere != null)
        {
            UpdateOrbiterMovement();
        }
        
        if (isPulsing)
        {
            UpdatePulsingEffect();
        }
    }
    
    public void StartOrbitingToTarget(Transform target)
    {
        if (target == null) 
        {
            return;
        }
        
        
        // Reset orbiter state before starting new movement
        ResetOrbiterState();
        
        targetSphere = target;
        orbitCenter = playerInitialPosition; // Always orbit around player's initial position
        
        // Calculate the nearest point on the orbit to the target
        Vector3 targetDirection = (target.position - orbitCenter).normalized;
        targetOrbitAngle = Mathf.Atan2(targetDirection.z, targetDirection.x);
        
        
        // Check if orbiter is already at the target position
        // Check if already at target angle
        float initialAngleDifference = targetOrbitAngle - orbitAngle;
        while (initialAngleDifference > Mathf.PI) initialAngleDifference -= 2f * Mathf.PI;
        while (initialAngleDifference < -Mathf.PI) initialAngleDifference += 2f * Mathf.PI;
        
        
        if (Mathf.Abs(initialAngleDifference) <= 0.01f)
        {
            // Already at target angle - trigger events immediately
            hasReachedTarget = true;
            OnReachedTarget?.Invoke(targetSphere);
            OnReachedTargetSimple?.Invoke();
            StartPulsingEffect();
            return;
        }
        
        isMovingToOrbitPosition = true;
        isOrbiting = true;
        hasReachedTarget = false;
        
        OnStartedOrbiting?.Invoke();
        
        // Start the movement coroutine
        StartCoroutine(MoveToOrbitPosition());
    }
    
    private IEnumerator MoveToOrbitPosition()
    {
        
        while (isMovingToOrbitPosition && targetSphere != null)
        {
            // Calculate current orbit position
            Vector3 currentOrbitPos = GetOrbitPosition(orbitAngle);
            float distanceToOrbitPos = Vector3.Distance(transform.position, currentOrbitPos);
            
            // Calculate angle difference to target
            float angleDifference = targetOrbitAngle - orbitAngle;
            
            // Normalize angle difference to shortest path
            while (angleDifference > Mathf.PI) angleDifference -= 2f * Mathf.PI;
            while (angleDifference < -Mathf.PI) angleDifference += 2f * Mathf.PI;
            
            
            if (Mathf.Abs(angleDifference) <= 0.01f) // Very small angle threshold
            {
                // Reached the target angle - snap to exact position
                orbitAngle = targetOrbitAngle;
                transform.position = GetOrbitPosition(orbitAngle);
                hasReachedTarget = true;
                isMovingToOrbitPosition = false;
                isOrbiting = false;
                OnReachedTarget?.Invoke(targetSphere);
                OnReachedTargetSimple?.Invoke(); // Trigger simple event for SimonSaysGame
                StartPulsingEffect();
                yield break;
            }
            else
            {
                // Move along the orbit path (circular movement)
                SetOrbiterColor(approachingColor);
                
                // Calculate the direction to move along the orbit
                float currentAngleDifference = targetOrbitAngle - orbitAngle;
                
                // Normalize angle difference to shortest path
                while (currentAngleDifference > Mathf.PI) currentAngleDifference -= 2f * Mathf.PI;
                while (currentAngleDifference < -Mathf.PI) currentAngleDifference += 2f * Mathf.PI;
                
                // Move along the orbit (convert linear speed to angular speed)
                float angleStep = (approachSpeed / orbitRadius) * Time.deltaTime;
                if (Mathf.Abs(currentAngleDifference) < angleStep)
                {
                    // Close enough, snap to target angle
                    orbitAngle = targetOrbitAngle;
                }
                else
                {
                    // Move towards target angle
                    orbitAngle += Mathf.Sign(currentAngleDifference) * angleStep;
                }
                
                // Update position based on new angle
                transform.position = GetOrbitPosition(orbitAngle);
            }
            
            yield return null;
        }
        
    }
    
    private Vector3 GetOrbitPosition(float angle)
    {
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * orbitRadius,
            0f, // Keep same Y level as orbit center
            Mathf.Sin(angle) * orbitRadius
        );
        Vector3 orbitPos = orbitCenter + offset;
        return orbitPos;
    }
    
    private void UpdateOrbiterMovement()
    {
        // This method is called in Update for smooth movement
        // The actual movement logic is in the coroutine
    }
    
    public void StopOrbiting()
    {
        isOrbiting = false;
        hasReachedTarget = false;
    }
    
    public void ResetOrbiter()
    {
        StopOrbiting();
        StopPulsingEffect();
        targetSphere = null;
        SetOrbiterColor(normalColor);
    }
    
    private void SetOrbiterColor(Color color)
    {
        if (orbiterRenderer != null)
        {
            orbiterRenderer.material.color = color;
        }
        
        if (orbitSphereComponent != null)
        {
            orbitSphereComponent.SetColor(color);
        }
    }
    
    public bool IsOrbiting()
    {
        return isOrbiting;
    }
    
    public bool HasReachedTarget()
    {
        return hasReachedTarget;
    }
    
    public Transform GetCurrentTarget()
    {
        return targetSphere;
    }
    
    private void StartPulsingEffect()
    {
        isPulsing = true;
        baseReachedColor = reachedColor;
    }
    
    private void UpdatePulsingEffect()
    {
        if (orbiterRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            Color currentColor = baseReachedColor + (Color.white * pulse);
            orbiterRenderer.material.color = currentColor;
        }
    }
    
    public void StopPulsingEffect()
    {
        isPulsing = false;
        SetOrbiterColor(baseReachedColor);
    }
    
    public void ResetOrbiterState()
    {
        // Stop any current movement and coroutines
        StopAllCoroutines();
        StopOrbiting();
        StopPulsingEffect();
        
        // Reset to normal color
        SetOrbiterColor(normalColor);
        
        // Reset state variables
        hasReachedTarget = false;
        targetSphere = null;
        isMovingToOrbitPosition = false;
        
    }
    
    public void ShrinkOrbiter()
    {
        StartCoroutine(ShrinkCoroutine());
    }
    
    public void GrowOrbiter()
    {
        StartCoroutine(GrowCoroutine());
    }
    
    System.Collections.IEnumerator ShrinkCoroutine()
    {
        Vector3 currentScale = transform.localScale;
        Vector3 targetScale = Vector3.one * minScale;
        float elapsedTime = 0f;
        
        
        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, progress);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    System.Collections.IEnumerator GrowCoroutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale; // Use the original spawned scale
        float elapsedTime = 0f;
        
        
        while (elapsedTime < growDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / growDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}
