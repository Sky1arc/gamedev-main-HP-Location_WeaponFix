using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationBarrier_MultiKey : MonoBehaviour
{
    [Header("Barrier Settings")]
    public string barrierName = "Location 2 Barrier";
    public bool isActive = false;
    public int requiredKeys = 2; // Number of keys needed
    private int keysCollected = 0;
    
    [Header("Visual Feedback")]
    public string blockedMessage = "You need {0} more key(s) to unlock this barrier!";
    public float messageDisplayTime = 3f;
    public Color barrierGizmoColor = Color.red;
    
    [Header("Push Back Settings")]
    public float pushBackForce = 15f;
    public float edgeBuffer = 3f;
    
    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private bool hasShownMessage = false;
    private float messageTimer = 0f;
    private SphereCollider sphereCollider;
    private Vector3 lastSafePosition;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 30f;
        }
        sphereCollider.isTrigger = true;
        
        Debug.Log($"{barrierName} initialized. Required keys: {requiredKeys}");
    }

    void Update()
    {
        if (hasShownMessage)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer >= 2f)
            {
                hasShownMessage = false;
                messageTimer = 0f;
            }
        }

        if (isActive && playerTransform != null)
        {
            EnforceBoundary();
        }
    }

    void EnforceBoundary()
    {
        if (sphereCollider == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 centerPos = transform.position;
        
        Vector3 playerPosFlat = new Vector3(playerPos.x, centerPos.y, playerPos.z);
        Vector3 centerPosFlat = new Vector3(centerPos.x, centerPos.y, centerPos.z);
        
        float distanceFromCenter = Vector3.Distance(playerPosFlat, centerPosFlat);
        float effectiveRadius = sphereCollider.radius * transform.lossyScale.x;
        float maxAllowedDistance = effectiveRadius - edgeBuffer;

        if (distanceFromCenter >= maxAllowedDistance)
        {
            Vector3 directionToCenter = (centerPosFlat - playerPosFlat).normalized;
            
            if (playerRigidbody != null)
            {
                Vector3 velocity = playerRigidbody.velocity;
                Vector3 velocityFlat = new Vector3(velocity.x, 0, velocity.z);
                
                if (Vector3.Dot(velocityFlat.normalized, -directionToCenter) > 0.5f)
                {
                    playerRigidbody.velocity = new Vector3(0, velocity.y, 0);
                }
                
                playerRigidbody.AddForce(directionToCenter * pushBackForce, ForceMode.VelocityChange);
            }
            else
            {
                Vector3 safePosition = centerPos + (-directionToCenter * (maxAllowedDistance - 1f));
                safePosition.y = playerPos.y;
                playerTransform.position = safePosition;
            }
            
            ShowBlockedMessage();
        }
        else
        {
            lastSafePosition = playerPos;
        }
    }

    void ShowBlockedMessage()
    {
        if (!hasShownMessage)
        {
            int keysNeeded = requiredKeys - keysCollected;
            string message = string.Format(blockedMessage, keysNeeded);
            
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ShowMessage(message, messageDisplayTime);
            }
            hasShownMessage = true;
            messageTimer = 0f;
            Debug.Log($"🔒 Keys collected: {keysCollected}/{requiredKeys}");
        }
    }

    // Called when a key is collected
    public void RegisterKeyCollected()
    {
        keysCollected++;
        Debug.Log($"🔑 Key collected! Progress: {keysCollected}/{requiredKeys}");
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage($"Key found! ({keysCollected}/{requiredKeys})", 3f);
        }
        
        // Check if all keys collected
        if (keysCollected >= requiredKeys)
        {
            UnlockBarrier();
        }
    }

    public void UnlockBarrier()
    {
        isActive = false;
        playerTransform = null;
        playerRigidbody = null;
        
        Debug.Log($"🔓 {barrierName} UNLOCKED! All keys collected.");
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage("All keys found! Barrier unlocked! You can now continue.", 4f);
        }
    }

    public void LockBarrier()
    {
        isActive = true;
        keysCollected = 0; // Reset key count
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
            lastSafePosition = playerTransform.position;
            
            Debug.Log($"🔒 {barrierName} LOCKED! Player needs {requiredKeys} keys.");
        }
        else
        {
            Debug.LogError("⚠️ Could not find Player!");
        }
    }

    void OnDrawGizmos()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        Gizmos.color = isActive ? barrierGizmoColor : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius - edgeBuffer);
    }

    void OnDrawGizmosSelected()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        Gizmos.color = isActive ? Color.red : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius - edgeBuffer);
        
        Gizmos.color = Color.cyan;
        float r = sphereCollider.radius;
        Gizmos.DrawLine(Vector3.zero, new Vector3(r, 0, 0));
        Gizmos.DrawLine(Vector3.zero, new Vector3(-r, 0, 0));
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, r));
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, -r));
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(Vector3.zero, 0.5f);
        
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = isActive ? Color.red : Color.green;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, 
            isActive ? $"ACTIVE ({keysCollected}/{requiredKeys} keys)" : "BARRIER INACTIVE", style);
#endif
    }
}