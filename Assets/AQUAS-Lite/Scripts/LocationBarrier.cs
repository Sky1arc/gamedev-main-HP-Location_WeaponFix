using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationBarrier : MonoBehaviour
{
    [Header("Barrier Settings")]
    public string barrierName = "Location 1 Barrier";
    public bool isActive = false; // Barrier starts inactive until save point triggers it
    
    [Header("Visual Feedback")]
    public string blockedMessage = "You cannot leave yet. Find the key to unlock the barrier!";
    public float messageDisplayTime = 3f;
    public Color barrierGizmoColor = Color.red;
    
    [Header("Push Back Settings")]
    public float pushBackForce = 15f; // Stronger force for Rigidbody
    public float edgeBuffer = 3f; // Distance from edge to start blocking
    
    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private bool hasShownMessage = false;
    private float messageTimer = 0f;
    private SphereCollider sphereCollider;
    private Vector3 lastSafePosition;

    void Start()
    {
        // Get sphere collider
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 30f; // Default radius
        }
        sphereCollider.isTrigger = true;
        
        Debug.Log($"{barrierName} initialized. Radius: {sphereCollider.radius}, Active: {isActive}");
    }

    void Update()
    {
        // Reset message cooldown
        if (hasShownMessage)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer >= 2f)
            {
                hasShownMessage = false;
                messageTimer = 0f;
            }
        }

        // ONLY enforce boundary if barrier is ACTIVE and player is being tracked
        if (isActive && playerTransform != null)
        {
            EnforceBoundary();
        }
    }

    void EnforceBoundary()
    {
        if (sphereCollider == null || playerTransform == null) return;

        // Calculate distance from center (only XZ plane, ignore Y)
        Vector3 playerPos = playerTransform.position;
        Vector3 centerPos = transform.position;
        
        Vector3 playerPosFlat = new Vector3(playerPos.x, centerPos.y, playerPos.z);
        Vector3 centerPosFlat = new Vector3(centerPos.x, centerPos.y, centerPos.z);
        
        float distanceFromCenter = Vector3.Distance(playerPosFlat, centerPosFlat);
        float effectiveRadius = sphereCollider.radius * transform.lossyScale.x;
        float maxAllowedDistance = effectiveRadius - edgeBuffer;

        // If player is too close to edge, push them back
        if (distanceFromCenter >= maxAllowedDistance)
        {
            Debug.Log($"⚠️ Player at edge! Distance: {distanceFromCenter:F2} / Max: {maxAllowedDistance:F2}");
            
            // Calculate direction to center (only XZ plane)
            Vector3 directionToCenter = (centerPosFlat - playerPosFlat).normalized;
            
            // Push player back using Rigidbody force
            if (playerRigidbody != null)
            {
                // Cancel outward velocity
                Vector3 velocity = playerRigidbody.velocity;
                Vector3 velocityFlat = new Vector3(velocity.x, 0, velocity.z);
                
                // If moving away from center, cancel that velocity
                if (Vector3.Dot(velocityFlat.normalized, -directionToCenter) > 0.5f)
                {
                    playerRigidbody.velocity = new Vector3(0, velocity.y, 0);
                }
                
                // Apply push force toward center
                playerRigidbody.AddForce(directionToCenter * pushBackForce, ForceMode.VelocityChange);
            }
            else
            {
                // Fallback: direct position change
                Vector3 safePosition = centerPos + (-directionToCenter * (maxAllowedDistance - 1f));
                safePosition.y = playerPos.y; // Keep Y position
                playerTransform.position = safePosition;
            }
            
            // Show message
            ShowBlockedMessage();
        }
        else
        {
            // Store last safe position
            lastSafePosition = playerPos;
        }
    }

    void ShowBlockedMessage()
    {
        if (!hasShownMessage)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ShowMessage(blockedMessage, messageDisplayTime);
            }
            hasShownMessage = true;
            messageTimer = 0f;
            Debug.Log($"📢 Showed blocked message");
        }
    }

    // Called when key is found
    public void UnlockBarrier()
    {
        isActive = false;
        playerTransform = null; // Stop tracking player
        playerRigidbody = null;
        
        Debug.Log($"🔓 {barrierName} UNLOCKED! Player can now leave.");
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage("Barrier unlocked! You can now continue.", 4f);
        }
    }

    // Called when save point is activated
    public void LockBarrier()
    {
        isActive = true;
        
        // Find and track player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
            lastSafePosition = playerTransform.position;
            
            Debug.Log($"🔒 {barrierName} LOCKED! Player trapped in location.");
            Debug.Log($"✓ Player found and tracked. Rigidbody: {(playerRigidbody != null ? "Yes" : "No")}");
        }
        else
        {
            Debug.LogError("⚠️ Could not find Player! Make sure player is tagged 'Player'");
        }
    }

    // Draw the area boundary in Scene view
    void OnDrawGizmos()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        // Outer boundary
        Gizmos.color = isActive ? barrierGizmoColor : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
        
        // Inner safe zone (yellow)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius - edgeBuffer);
    }

    void OnDrawGizmosSelected()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        // Outer boundary - RED when active, GREEN when inactive
        Gizmos.color = isActive ? Color.red : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
        
        // Safe zone boundary - YELLOW
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius - edgeBuffer);
        
        // Draw cardinal directions (helps with positioning)
        Gizmos.color = Color.cyan;
        float r = sphereCollider.radius;
        Gizmos.DrawLine(Vector3.zero, new Vector3(r, 0, 0));  // East
        Gizmos.DrawLine(Vector3.zero, new Vector3(-r, 0, 0)); // West
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, r));  // North
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, -r)); // South
        
        // Center marker
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(Vector3.zero, 0.5f);
        
        // Status text
        GUIStyle style = new GUIStyle();
        style.normal.textColor = isActive ? Color.red : Color.green;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, 
            isActive ? "BARRIER ACTIVE" : "BARRIER INACTIVE", style);
    }
}