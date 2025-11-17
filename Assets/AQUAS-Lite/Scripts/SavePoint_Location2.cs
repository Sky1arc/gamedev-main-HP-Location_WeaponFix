using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint_Location2 : MonoBehaviour
{
    [Header("Save Point Settings")]
    public string savePointName = "Location 2";
    public bool showActivationMessage = true;
    public float messageDisplayTime = 3f;
    
    [Header("Barrier Integration")]
    public LocationBarrier_MultiKey linkedBarrier;
    
    [Header("Key Integration - 2 Keys Required")]
    public KeyItem_MultiKey key1; // First key
    public KeyItem_MultiKey key2; // Second key
    
    [Header("Clue System")]
    public bool showClue = true;
    public string clueMessage = "Two keys lie hidden in this forest. Seek the ancient oak and the forgotten shrine.";
    public float clueDelay = 2f;
    
    [Header("Visual Feedback")]
    public GameObject activationEffect;
    public AudioClip activationSound;
    
    private bool hasBeenActivated = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenActivated)
        {
            ActivateSavePoint(other.gameObject);
        }
    }

    void ActivateSavePoint(GameObject player)
    {
        hasBeenActivated = true;
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetSpawnPoint(transform.position, transform.rotation);
            
            if (showActivationMessage)
            {
                gameManager.ShowMessage($"Save Point Activated: {savePointName}", messageDisplayTime);
            }
            
            Debug.Log($"✅ Save point activated: {savePointName}");
        }
        
        // LOCK the barrier
        if (linkedBarrier != null)
        {
            linkedBarrier.LockBarrier();
            Debug.Log($"✅ Barrier locked - 2 keys required");
        }
        else
        {
            Debug.LogWarning($"⚠️ No linked barrier found for {savePointName}!");
        }
        
        // SPAWN both keys
        if (key1 != null)
        {
            key1.SpawnKey();
            Debug.Log($"✅ Key 1 spawned");
        }
        else
        {
            Debug.LogWarning($"⚠️ Key 1 not assigned!");
        }
        
        if (key2 != null)
        {
            key2.SpawnKey();
            Debug.Log($"✅ Key 2 spawned");
        }
        else
        {
            Debug.LogWarning($"⚠️ Key 2 not assigned!");
        }
        
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        if (activationEffect != null)
        {
            activationEffect.SetActive(true);
        }
        
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
        
        ClueSystem clueSystem = FindObjectOfType<ClueSystem>();
        if (clueSystem != null)
        {
            clueSystem.SetClue(clueMessage);
            Debug.Log($"✅ Clue registered with ClueSystem");
        }
    }

    public void ResetSavePoint()
    {
        hasBeenActivated = false;
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
        
        if (linkedBarrier != null)
        {
            linkedBarrier.UnlockBarrier();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = hasBeenActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        
        if (linkedBarrier != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, linkedBarrier.transform.position);
        }
        
        if (key1 != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, key1.transform.position);
        }
        
        if (key2 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, key2.transform.position);
        }
    }
}