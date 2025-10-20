using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource;
    public AudioSource spatialAudioSource; // For 3D spatial audio in AR
    
    [Header("Audio File")]
    public AudioClip baseNote;  // Single audio file (e.g., C note)
    
    [Header("Pitch Settings for Do-Re-Mi Scale")]
    public float redPitch = 1.0f;     // Do (C) - base pitch
    public float bluePitch = 1.125f;  // Re (D) - +1 semitone
    public float greenPitch = 1.25f;  // Mi (E) - +2 semitones  
    public float yellowPitch = 1.33f; // Fa (F) - +3 semitones
    
    [Header("Audio Settings")]
    public float volume = 0.7f;
    public float pitch = 1f;
    public float audioDuration = 0.1f; // How long to play the audio (in seconds)
    
    [Header("Spatial Audio Settings")]
    public float spatialVolume = 0.8f;
    public float spatialRange = 10f;
    
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    instance = audioManagerObj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void SetupAudioSource()
    {
        // Setup 2D audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio
        
        // Setup 3D spatial audio source
        if (spatialAudioSource == null)
        {
            spatialAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        spatialAudioSource.volume = spatialVolume;
        spatialAudioSource.pitch = pitch;
        spatialAudioSource.playOnAwake = false;
        spatialAudioSource.spatialBlend = 1f; // 3D audio
        spatialAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        spatialAudioSource.maxDistance = spatialRange;
        spatialAudioSource.minDistance = 1f; // Minimum distance for 3D audio
    }
    
    public void PlayColorNote(Color color)
    {
        if (baseNote != null)
        {
            float pitchValue = GetPitchForColor(color);
            audioSource.pitch = pitchValue;
            audioSource.PlayOneShot(baseNote, volume);
            
            // Stop the audio after the specified duration
            StartCoroutine(StopAudioAfterDelay(audioDuration));
            
        }
        else
        {
        }
    }
    
    public void PlayColorNoteSpatial(Color color, Vector3 position)
    {
        if (baseNote != null)
        {
            float pitchValue = GetPitchForColor(color);
            // Position the spatial audio source at the sphere's location
            spatialAudioSource.transform.position = position;
            spatialAudioSource.pitch = pitchValue;
            spatialAudioSource.PlayOneShot(baseNote, spatialVolume);
            
            // Stop the spatial audio after the specified duration
            StartCoroutine(StopSpatialAudioAfterDelay(audioDuration));
            
        }
        else
        {
        }
    }
    
    float GetPitchForColor(Color color)
    {
        // Match colors to pitch values (Do-Re-Mi-Fa)
        if (IsColorMatch(color, Color.red))
            return redPitch;
        else if (IsColorMatch(color, Color.blue))
            return bluePitch;
        else if (IsColorMatch(color, Color.green))
            return greenPitch;
        else if (IsColorMatch(color, Color.yellow))
            return yellowPitch;
        else
            return 1.0f; // Default pitch
    }
    
    bool IsColorMatch(Color color1, Color color2)
    {
        // Compare colors with some tolerance for floating point precision
        return Vector4.Distance(new Vector4(color1.r, color1.g, color1.b, color1.a), 
                               new Vector4(color2.r, color2.g, color2.b, color2.a)) < 0.1f;
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    
    public void SetPitch(float newPitch)
    {
        pitch = Mathf.Clamp(newPitch, 0.1f, 3f);
        if (audioSource != null)
        {
            audioSource.pitch = pitch;
        }
    }
    
    System.Collections.IEnumerator StopAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    System.Collections.IEnumerator StopSpatialAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (spatialAudioSource.isPlaying)
        {
            spatialAudioSource.Stop();
        }
    }
}
