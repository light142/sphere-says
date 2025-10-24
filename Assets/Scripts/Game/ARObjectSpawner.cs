using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARObjectSpawnerAnchored : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject orbiterPrefab;
    public Material redMat;
    public Material blueMat;
    public Material greenMat;
    public Material yellowMat;
    public Material orbiterMat;
    public float distance = 1.5f; // Distance from player
    
    [Header("Color-Specific Orb Effect Prefabs")]
    public GameObject redOrbEffect;
    public GameObject blueOrbEffect;
    public GameObject greenOrbEffect;
    public GameObject yellowOrbEffect;

    private Transform arCamera;
    private ARAnchorManager anchorManager;
    private GameObject[] spawnedAnchors = new GameObject[4]; // Track spawned anchors
    private GameObject spawnedOrbiter = null; // Track spawned orbiter
    
    public GameObject GetOrbiter()
    {
        return spawnedOrbiter;
    }
    
    public OrbiterController GetOrbiterController()
    {
        if (spawnedOrbiter != null)
        {
            // The OrbiterController is on the child object, not the anchor
            OrbiterController controller = spawnedOrbiter.GetComponentInChildren<OrbiterController>();
            if (controller != null)
            {
            }
            else
            {
            }
            return controller;
        }
        return null;
    }

    void Start()
    {
        arCamera = Camera.main.transform;
        anchorManager = GetComponent<ARAnchorManager>();
        // Don't spawn automatically - wait for explicit call
    }
    
    public void TriggerSpawn()
    {
        // Clear existing objects first
        ClearExistingObjects();
        
        // Spawn new objects
        StartCoroutine(SpawnObjectsDelayed());
    }
    
    public void ClearExistingObjects()
    {
        // Destroy all tracked anchors and their children
        for (int i = 0; i < spawnedAnchors.Length; i++)
        {
            if (spawnedAnchors[i] != null)
            {
                DestroyImmediate(spawnedAnchors[i]);
                spawnedAnchors[i] = null;
            }
        }

        if (spawnedOrbiter != null)
        {
            DestroyImmediate(spawnedOrbiter);
            spawnedOrbiter = null;
        }
        else
        {
        }
        
        // Also clear any orphaned AR anchors in the scene (including orbiter anchor)
        ARAnchor[] existingAnchors = FindObjectsByType<ARAnchor>(FindObjectsSortMode.None);
        foreach (ARAnchor anchor in existingAnchors)
        {
            if (anchor.name.StartsWith("Anchor_"))
            {
                DestroyImmediate(anchor.gameObject);
            }
        }
        
        // Force clear any remaining orbiter objects
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Orbiter") || obj.name.Contains("orbiter"))
            {
                DestroyImmediate(obj);
            }
        }
    }
    
    System.Collections.IEnumerator SpawnObjectsDelayed()
    {
        yield return null; // Wait one frame
        SpawnObjects();
    }

    void SpawnObjects()
    {        
        // Get camera position and create a flat forward direction (ignore pitch/roll)
        Vector3 cameraPos = arCamera.position;
        Vector3 cameraForward = arCamera.forward;
        cameraForward.y = 0f; // Flatten to horizontal plane
        cameraForward.Normalize();
        
        // Create perpendicular directions for left/right
        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized;
        
        // Define sphere positions relative to camera's horizontal forward direction
        Vector3[] relativePositions = {
            cameraPos + cameraForward * distance,     // Front (Red)
            cameraPos + (-cameraForward) * distance,   // Back (Green) 
            cameraPos + (-cameraRight) * distance,     // Left (Blue)
            cameraPos + cameraRight * distance         // Right (Yellow)
        };
        
        Material[] materials = { redMat, greenMat, blueMat, yellowMat };

        for (int i = 0; i < 4; i++)
        {
            // Use position relative to camera's horizontal direction
            Vector3 worldPos = relativePositions[i];
            worldPos.y = cameraPos.y - 1f; // Place 1m below camera height
            
            spawnedAnchors[i] = SpawnSphere(worldPos, materials[i]);
        }
        
        // Spawn orbiter in front of camera (horizontal direction)
        Vector3 orbiterPos = cameraPos + cameraForward * distance * 0.5f;
        // Place orbiter 1m below camera height
        orbiterPos.y = cameraPos.y - 1f;
        
        spawnedOrbiter = SpawnOrbiter(orbiterPos, orbiterMat);
    }

    GameObject SpawnSphere(Vector3 worldPosition, Material colorMat)
    {
        // Create empty GameObject to hold the anchor
        GameObject anchorGO = new GameObject("Anchor_" + colorMat.name);
        anchorGO.transform.position = worldPosition;
        anchorGO.transform.rotation = Quaternion.identity;

        // Add ARAnchor component (registers with ARAnchorManager automatically)
        ARAnchor anchor = anchorGO.AddComponent<ARAnchor>(); 

        if (anchor is null)
            return null;

        // Instantiate sphere as child of anchor
        GameObject obj = Instantiate(spherePrefab, anchor.transform);
        obj.GetComponent<Renderer>().material = colorMat;
        
        // Add AR interaction component for Simon Says game
        ARColorSphere colorSphere = obj.GetComponent<ARColorSphere>();
        if (colorSphere == null)
        {
            colorSphere = obj.AddComponent<ARColorSphere>();
        }
        
        // Add collider for interaction
        if (obj.GetComponent<Collider>() == null)
        {
            SphereCollider collider = obj.AddComponent<SphereCollider>();
            collider.isTrigger = true;
        }
        
        // Set the color based on material
        Color sphereColor = GetColorFromMaterial(colorMat);
        colorSphere.SetColor(sphereColor);
        
        // Add orb effect to the sphere based on color
        GameObject colorOrbPrefab = GetOrbPrefabForColor(colorMat);
        AddOrbEffectToSphere(obj, colorOrbPrefab);
        
        return anchorGO;
    }

    GameObject SpawnOrbiter(Vector3 worldPosition, Material colorMat)
    {
        // Create empty GameObject to hold the anchor
        GameObject anchorGO = new("Anchor_Orbiter");
        anchorGO.transform.position = worldPosition;
        anchorGO.transform.rotation = Quaternion.identity;

        // Add ARAnchor component (registers with ARAnchorManager automatically)
        ARAnchor anchor = anchorGO.AddComponent<ARAnchor>(); 

        if (anchor is null)
            return null;

        // Instantiate orbiter as child of anchor
        GameObject obj = Instantiate(orbiterPrefab, anchor.transform);
        var rootRenderer = obj.GetComponent<Renderer>();
        if (rootRenderer == null)
        {
            rootRenderer = obj.GetComponentInChildren<Renderer>(true);
        }
        if (rootRenderer != null && colorMat != null)
        {
            rootRenderer.material = colorMat;
        }
        
        // Debug the positions to see if prefab has local offset
        
        // Zero the local position to ensure orbiter spawns exactly at anchor position
        obj.transform.localPosition = Vector3.zero;
        
        // Add/get visual component via interface so we can swap implementations
        IAROrbitSphere orbitVisual = obj.GetComponent<IAROrbitSphere>();
        if (orbitVisual == null)
        {
            // Fallback to default AROrbitSphere if none present
            AROrbitSphere fallback = obj.GetComponent<AROrbitSphere>();
            if (fallback == null)
            {
                fallback = obj.AddComponent<AROrbitSphere>();
            }
            orbitVisual = fallback;
        }
        
        // Add orbiter controller for movement
        OrbiterController orbiterController = obj.GetComponent<OrbiterController>();
        if (orbiterController == null)
        {
            orbiterController = obj.AddComponent<OrbiterController>();
        }
        else
        {
            // Reset the existing component to ensure clean state and reset all values
            orbiterController.ResetOrbiterState();
        }
        
        // Set the player's initial position for the orbiter (camera position, not orbiter spawn position)
        Vector3 cameraPos = arCamera.position;
        // Set orbit center to match the orbiter spawn height (1m below camera)
        Vector3 orbitCenter = new Vector3(cameraPos.x, cameraPos.y - 1f, cameraPos.z);
        orbiterController.playerInitialPosition = orbitCenter;
        
        // Set orbit radius to match the orbiter spawn distance
        orbiterController.orbitRadius = distance * 0.5f;
        
        // Initialize the orbiter at a proper orbit position
        orbiterController.InitializeAfterSpawn();
        
        // Set the color based on material (no-op for implementations that ignore color)
        Color sphereColor = GetColorFromMaterial(colorMat);
        orbitVisual.SetColor(sphereColor);
        
        return anchorGO;
    }
    
    private Color GetColorFromMaterial(Material mat)
    {
        if (mat == null) return Color.white;
        string n = mat.name;
        if (n.Contains("Red")) return Color.red;
        if (n.Contains("Blue")) return Color.blue;
        if (n.Contains("Green")) return Color.green;
        if (n.Contains("Yellow")) return Color.yellow;
        return Color.white;
    }
    
    private GameObject GetOrbPrefabForColor(Material mat)
    {
        if (mat == null) return null;
        string n = mat.name;
        if (n.Contains("Red")) return redOrbEffect;
        if (n.Contains("Blue")) return blueOrbEffect;
        if (n.Contains("Green")) return greenOrbEffect;
        if (n.Contains("Yellow")) return yellowOrbEffect;
        return null;
    }
    
    private void AddOrbEffectToSphere(GameObject sphere, GameObject orbPrefab)
    {
        // Add OrbEffectManager component to the sphere
        OrbEffectManager orbEffectManager = sphere.GetComponent<OrbEffectManager>();
        if (orbEffectManager == null)
        {
            orbEffectManager = sphere.AddComponent<OrbEffectManager>();
        }
        
        // Attach the specific orb effect prefab
        if (orbPrefab != null)
        {
            orbEffectManager.AttachSpecificOrbEffect(sphere.transform, orbPrefab);
        }
    }
}
