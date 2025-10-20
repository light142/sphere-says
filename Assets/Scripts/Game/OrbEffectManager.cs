using UnityEngine;

public class OrbEffectManager : MonoBehaviour
{
    [Header("Effect Settings")]
    public float orbScale = 1f; // Scale of the orb effect relative to the sphere
    public Vector3 orbOffset = Vector3.zero; // Offset from the sphere center
    
    private GameObject currentOrbEffect;
    private Transform targetSphere;
    
    public void AttachSpecificOrbEffect(Transform sphereTransform, GameObject specificOrbPrefab)
    {
        // Remove existing effect if any
        if (currentOrbEffect != null)
        {
            DestroyImmediate(currentOrbEffect);
        }
        
        targetSphere = sphereTransform;
        
        if (specificOrbPrefab != null)
        {
            // Instantiate the specific orb effect as a child of the sphere
            currentOrbEffect = Instantiate(specificOrbPrefab, sphereTransform);
            currentOrbEffect.transform.localPosition = orbOffset;
            
            // Debug: Log the original prefab scale
            Debug.Log($"Original prefab scale: {specificOrbPrefab.transform.localScale}");
            Debug.Log($"Our orbScale: {orbScale}");
            
            // Force a very small scale regardless of prefab scale
            currentOrbEffect.transform.localScale = Vector3.one * orbScale;
            
            Debug.Log($"Final scale: {currentOrbEffect.transform.localScale}");
            
            // Disable the original orb's collider to avoid interference
            Collider orbCollider = currentOrbEffect.GetComponent<Collider>();
            if (orbCollider != null)
            {
                orbCollider.enabled = false;
            }
            
            // Make sure the orb effect doesn't interfere with sphere interactions
            currentOrbEffect.layer = sphereTransform.gameObject.layer;
        }
    }
    
    public void RemoveOrbEffect()
    {
        if (currentOrbEffect != null)
        {
            DestroyImmediate(currentOrbEffect);
            currentOrbEffect = null;
        }
    }
    
    public void ChangeOrbEffect(GameObject newOrbPrefab)
    {
        if (targetSphere != null)
        {
            AttachSpecificOrbEffect(targetSphere, newOrbPrefab);
        }
    }
    
    public void SetOrbScale(float scale)
    {
        orbScale = scale;
        if (currentOrbEffect != null)
        {
            currentOrbEffect.transform.localScale = Vector3.one * orbScale;
        }
    }
    
    public void MakeOrbSmaller(float reductionFactor = 0.5f)
    {
        orbScale *= reductionFactor;
        if (currentOrbEffect != null)
        {
            currentOrbEffect.transform.localScale = Vector3.one * orbScale;
        }
    }
    
    public void MakeOrbLarger(float increaseFactor = 2f)
    {
        orbScale *= increaseFactor;
        if (currentOrbEffect != null)
        {
            currentOrbEffect.transform.localScale = Vector3.one * orbScale;
        }
    }
    
    public void ForceTinyScale()
    {
        if (currentOrbEffect != null)
        {
            currentOrbEffect.transform.localScale = Vector3.one * 0.001f;
            Debug.Log($"Forced tiny scale: {currentOrbEffect.transform.localScale}");
        }
    }
    
    public void SetOrbOffset(Vector3 offset)
    {
        orbOffset = offset;
        if (currentOrbEffect != null)
        {
            currentOrbEffect.transform.localPosition = orbOffset;
        }
    }
}
