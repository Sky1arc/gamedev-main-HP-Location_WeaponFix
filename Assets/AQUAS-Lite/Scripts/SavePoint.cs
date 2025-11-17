using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [Header("Save Point Settings")]
    public string savePointName = "Location 1";
    public bool showActivationMessage = true;
    public float messageDisplayTime = 3f;
    
    [Header("Barrier Integration")]
    public LocationBarrier linkedBarrier; // Barrier that LOCKS when save point is triggered
    
    [Header("Key Integration")]
    public KeyItem linkedKey; // Key that SPAWNS when save point is triggered
    
    [Header("Clue System")]
    public bool showClue = true;
    public string clueMessage = "It is sweet, it's oh so plenty! and oh it's red.";
    public float clueDelay = 2f; // Show clue X seconds after activation
    
    [Header("Visual Feedback (Optional)")]
    public GameObject activationEffect; // Particle effect or visual indicator
    public AudioClip activationSound; // Sound when activated
    
    private bool hasBeenActivated = false;
    private AudioSource audioSource;

    void Start()
    {
        // Get or add audio source for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (other.CompareTag("Player") && !hasBeenActivated)
        {
            ActivateSavePoint(other.gameObject);
        }
    }

    void ActivateSavePoint(GameObject player)
    {
        hasBeenActivated = true;
        
        // Set this as the new spawn point
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetSpawnPoint(transform.position, transform.rotation);
            
            // Show save point message
            if (showActivationMessage)
            {
                gameManager.ShowMessage($"Save Point Activated: {savePointName}", messageDisplayTime);
            }
            
            Debug.Log($"✅ Save point activated: {savePointName}");
        }
        
        // LOCK the barrier - trap player inside location
        if (linkedBarrier != null)
        {
            linkedBarrier.LockBarrier();
            Debug.Log($"✅ Calling LockBarrier() for {savePointName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No linked barrier found for {savePointName}! Drag Location1_Barrier into the Linked Barrier field.");
        }
        
        // SPAWN the key - make it appear in the world
        if (linkedKey != null)
        {
            linkedKey.SpawnKey();
            Debug.Log($"✅ Spawning key for {savePointName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No linked key found for {savePointName}! Drag the Key GameObject into the Linked Key field.");
        }
        
        // Play sound effect
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Show visual effect
        if (activationEffect != null)
        {
            activationEffect.SetActive(true);
        }
        
        // Show clue after a delay
        if (showClue && !string.IsNullOrEmpty(clueMessage))
        {
            StartCoroutine(ShowClueAfterDelay());
        }
    }

    IEnumerator ShowClueAfterDelay()
    {
        yield return new WaitForSeconds(clueDelay);
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage($"CLUE: {clueMessage}", 6f);
        }
        
        // Register clue with ClueSystem so player can view it again
        ClueSystem clueSystem = FindObjectOfType<ClueSystem>();
        if (clueSystem != null)
        {
            clueSystem.SetClue(clueMessage);
            Debug.Log($"✅ Clue registered with ClueSystem");
        }
    }

    // Optional: Reset save point (for testing)
    public void ResetSavePoint()
    {
        hasBeenActivated = false;
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
        
        // Unlock barrier
        if (linkedBarrier != null)
        {
            linkedBarrier.UnlockBarrier();
        }
    }

    // Draw gizmo in scene view to see save point location
    void OnDrawGizmos()
    {
        Gizmos.color = hasBeenActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        
        // Draw line to linked barrier
        if (linkedBarrier != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, linkedBarrier.transform.position);
        }
        
        // Draw line to linked key
        if (linkedKey != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, linkedKey.transform.position);
        }
    }
}