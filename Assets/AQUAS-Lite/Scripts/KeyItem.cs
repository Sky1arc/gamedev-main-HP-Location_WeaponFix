using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyName = "Mysterious Key";
    public LocationBarrier linkedBarrier; // The barrier this key unlocks
    
    [Header("Spawn Settings")]
    public bool startHidden = true; // Key is hidden until save point activates
    public GameObject spawnEffect; // Optional: Effect when key appears
    public float spawnEffectDuration = 2f;
    
    [Header("Visual Effects")]
    public GameObject collectEffect; // Particle effect on collection
    public AudioClip collectSound; // Sound when collected
    public AudioClip spawnSound; // Sound when key appears
    public bool rotateObject = true;
    public float rotationSpeed = 50f;
    public bool floatObject = false;
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;
    
    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public bool showPrompt = true;
    
    private AudioSource audioSource;
    private bool isInRange = false;
    private bool isCollected = false;
    private bool isSpawned = false;
    private Vector3 startPosition;
    private MeshRenderer meshRenderer;
    private Collider keyCollider;

    void Start()
    {
        startPosition = transform.position;
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (collectSound != null || spawnSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Auto-find barrier if not assigned
        if (linkedBarrier == null)
        {
            linkedBarrier = FindObjectOfType<LocationBarrier>();
        }
        
        // Get mesh renderer
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        
        // Add trigger collider if missing
        keyCollider = GetComponent<SphereCollider>();
        if (keyCollider == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = interactionRange;
            keyCollider = col;
        }
        else
        {
            keyCollider.isTrigger = true;
        }
        
        // Hide key at start if needed
        if (startHidden)
        {
            HideKey();
        }
        else
        {
            isSpawned = true;
        }
    }

    void Update()
    {
        if (isCollected || !isSpawned) return;
        
        // Rotate the key
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Float animation
        if (floatObject)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        // Check for interaction
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            CollectKey();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected && isSpawned)
        {
            isInRange = true;
            
            if (showPrompt)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.ShowMessage($"Press {interactKey} to collect {keyName}", 2f);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }

    // Called by SavePoint to make key appear
    public void SpawnKey()
    {
        if (isSpawned) return;
        
        isSpawned = true;
        Debug.Log($"✨ {keyName} has appeared!");
        
        // Show key visuals
        ShowKey();
        
        // Play spawn sound
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        
        // Show spawn effect
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            Destroy(effect, spawnEffectDuration);
        }
        
        // Show message to player
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage($"The {keyName} has appeared!", 3f);
        }
    }

    void HideKey()
    {
        // Hide mesh
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        // Disable collider so player can't interact
        if (keyCollider != null)
        {
            keyCollider.enabled = false;
        }
    }

    void ShowKey()
    {
        // Show mesh
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
        
        // Enable collider for interaction
        if (keyCollider != null)
        {
            keyCollider.enabled = true;
        }
    }

    void CollectKey()
    {
        if (isCollected) return;
        
        isCollected = true;
        Debug.Log($"Collected: {keyName}");
        
        // Play sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Spawn particle effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Unlock the barrier
        if (linkedBarrier != null)
        {
            linkedBarrier.UnlockBarrier();
        }
        
        // Clear the clue since puzzle is complete
        ClueSystem clueSystem = FindObjectOfType<ClueSystem>();
        if (clueSystem != null)
        {
            clueSystem.ClearClue();
        }
        
        // Show collection message
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage($"You found the {keyName}! The barrier has been unlocked.", 4f);
        }
        
        // Destroy the key
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        // Hide visuals
        HideKey();
        
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to linked barrier
        if (linkedBarrier != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, linkedBarrier.transform.position);
        }
    }
}