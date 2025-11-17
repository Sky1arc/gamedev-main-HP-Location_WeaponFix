using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Spawn Settings")]
    public Transform defaultSpawnPoint;
    public Transform player;
    
    [Header("UI References")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    
    private Vector3 currentSpawnPosition;
    private Quaternion currentSpawnRotation;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Set default spawn point
        if (defaultSpawnPoint != null)
        {
            currentSpawnPosition = defaultSpawnPoint.position;
            currentSpawnRotation = defaultSpawnPoint.rotation;
        }
        else if (player != null)
        {
            currentSpawnPosition = player.position;
            currentSpawnRotation = player.rotation;
        }
        
        // Hide message panel at start
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    public void SetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        currentSpawnPosition = position;
        currentSpawnRotation = rotation;
        Debug.Log($"Spawn point updated to: {position}");
    }

    public void RespawnPlayer()
    {
        if (player != null)
        {
            // Disable character controller temporarily if it exists
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            // Move player to spawn point
            player.position = currentSpawnPosition;
            player.rotation = currentSpawnRotation;
            
            // Re-enable character controller
            if (controller != null)
            {
                controller.enabled = true;
            }
            
            // Reset player health
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.currentHealth = playerHealth.maxHealth;
            }
            
            Debug.Log("Player respawned at save point");
        }
    }

    public void ShowMessage(string message, float duration)
    {
        if (messagePanel != null && messageText != null)
        {
            StartCoroutine(DisplayMessage(message, duration));
        }
    }

    IEnumerator DisplayMessage(string message, float duration)
    {
        messagePanel.SetActive(true);
        messageText.text = message;
        
        yield return new WaitForSeconds(duration);
        
        messagePanel.SetActive(false);
    }

    public Vector3 GetCurrentSpawnPosition()
    {
        return currentSpawnPosition;
    }
}