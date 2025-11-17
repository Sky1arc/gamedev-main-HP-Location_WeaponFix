using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem_MultiKey : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyName = "Mysterious Key";
    public LocationBarrier_MultiKey linkedBarrier; // Multi-key barrier
    
    [Header("Spawn Settings")]
    public bool startHidden = true;
    public GameObject spawnEffect;
    public float spawnEffectDuration = 2f;
    
    [Header("Visual Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;
    public AudioClip spawnSound;
    public bool rotateObject = true;
    public float rotationSpeed = 50f;
    public bool floatObject = true;
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (collectSound != null || spawnSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (linkedBarrier == null)
        {
            linkedBarrier = FindObjectOfType<LocationBarrier_MultiKey>();
        }
        
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        
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
        
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        if (floatObject)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
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

    public void SpawnKey()
    {
        if (isSpawned) return;
        
        isSpawned = true;
        Debug.Log($"✨ {keyName} has appeared!");
        
        ShowKey();
        
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            Destroy(effect, spawnEffectDuration);
        }
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage($"The {keyName} has appeared!", 3f);
        }
    }

    void HideKey()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        if (keyCollider != null)
        {
            keyCollider.enabled = false;
        }
    }

    void ShowKey()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
        
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
        
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Register key with barrier (doesn't unlock yet if more keys needed)
        if (linkedBarrier != null)
        {
            linkedBarrier.RegisterKeyCollected();
        }
        
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        HideKey();
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (linkedBarrier != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, linkedBarrier.transform.position);
        }
    }
}