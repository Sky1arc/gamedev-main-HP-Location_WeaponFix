using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // *** FIX: Required for EventSystem access ***

public class GrimoireWeapon : MonoBehaviour
{
    [Header("Grimoire References")]
    public GameObject grimoireModel;
    public Animator grimoireAnimator;
    public Transform fireSpawnPoint;
    
    [Header("Shooting Settings")]
    public KeyCode shootKey = KeyCode.Mouse0;
    public GameObject fireballPrefab;
    public float fireballSpeed = 20f;
    public float fireballDamage = 34f;
    public float fireballLifetime = 5f;
    public float shootCooldown = 0.5f;
    
    [Header("Animation Settings")]
    public string shootAnimationTrigger = "Shoot";
    public bool hasShootAnimation = true;
    
    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip pageFlipSound;
    
    [Header("Visual Effects")]
    public GameObject muzzleFlashEffect;
    public float muzzleFlashDuration = 0.1f;
    
    [Header("Ammo System (Optional)")]
    public bool unlimitedAmmo = true;
    public int maxAmmo = 50;
    public int currentAmmo;

    [Header("Game State")]
    public StartScreen startScreenManager;
    
    // Internal variables
    private AudioSource audioSource;
    private float lastShootTime = 0f;
    private Camera playerCamera;
    private bool isEquipped = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (!unlimitedAmmo)
        {
            currentAmmo = maxAmmo;
        }
        
        if (grimoireAnimator == null && grimoireModel != null)
        {
            grimoireAnimator = grimoireModel.GetComponent<Animator>();
        }
        
        Debug.Log("✅ Grimoire Weapon initialized");
    }

    void Update()
    {
        if (!isEquipped) return;
        
        if (startScreenManager != null && !startScreenManager.isGameStarted)
        {
            return;
        }
        
        if (!IsPlayerAlive())
        {
            return;
        }

        // *** FIX: Prevent shooting if the mouse is over a UI element ***
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // Ignore input if it's meant for the UI
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }
    
    bool IsPlayerAlive()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                return playerHealth.currentHealth > 0;
            }
        }
        
        return true;
    }

    void TryShoot()
    {
        if (Time.time < lastShootTime + shootCooldown)
        {
            return;
        }
        
        if (!unlimitedAmmo && currentAmmo <= 0)
        {
            Debug.Log("⚠️ Out of ammo!");
            return;
        }
        
        Shoot();
        lastShootTime = Time.time;
        
        if (!unlimitedAmmo)
        {
            currentAmmo--;
        }
    }

    void Shoot()
    {
        Debug.Log("🔥 Grimoire shooting fireball!");
        
        if (hasShootAnimation && grimoireAnimator != null)
        {
            grimoireAnimator.SetTrigger(shootAnimationTrigger);
        }
        
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        if (muzzleFlashEffect != null && fireSpawnPoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashEffect, fireSpawnPoint.position, fireSpawnPoint.rotation);
            Destroy(flash, muzzleFlashDuration);
        }
        
        SpawnFireball();
    }

    void SpawnFireball()
    {
        if (fireballPrefab == null)
        {
            Debug.LogError("⚠️ Fireball prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos;
        Quaternion spawnRot;
        
        if (fireSpawnPoint != null)
        {
            spawnPos = fireSpawnPoint.position;
            spawnRot = fireSpawnPoint.rotation;
        }
        else
        {
            spawnPos = playerCamera.transform.position + playerCamera.transform.forward * 1f;
            spawnRot = playerCamera.transform.rotation;
        }
        
        GameObject fireball = Instantiate(fireballPrefab, spawnPos, spawnRot);
        
        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript == null)
        {
            fireballScript = fireball.AddComponent<Fireball>();
        }
        
        fireballScript.damage = fireballDamage;
        fireballScript.speed = fireballSpeed;
        fireballScript.lifetime = fireballLifetime;
        
        Vector3 shootDirection = playerCamera.transform.forward;
        fireballScript.Initialize(shootDirection);
        
        Debug.Log($"✅ Fireball spawned at {spawnPos}");
    }

    public void AddAmmo(int amount)
    {
        if (!unlimitedAmmo)
        {
            currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
            Debug.Log($"✅ Added {amount} ammo. Current: {currentAmmo}/{maxAmmo}");
        }
    }

  // Replace the existing SetEquipped method with this one
public void SetEquipped(bool equipped)
{
    isEquipped = equipped;
    if (grimoireModel != null)
    {
        grimoireModel.SetActive(equipped);
    }
    
    // *** NEW: Play the page flip sound when equipping or unequipping ***
    if (audioSource != null && pageFlipSound != null)
    {
        audioSource.PlayOneShot(pageFlipSound);
    }

    // Reset animation to idle when equipping/unequipping
    if (grimoireAnimator != null)
    {
        if (equipped)
        {
            // Force reset to Idle state when equipping
            grimoireAnimator.Play("Idle", 0, 0f);
            grimoireAnimator.ResetTrigger(shootAnimationTrigger);
        }
        else
        {
            // Reset trigger when unequipping to prevent stuck animation
            grimoireAnimator.ResetTrigger(shootAnimationTrigger);
        }
    }
    
    Debug.Log($"Grimoire {(equipped ? "equipped" : "unequipped")} - Animation reset to Idle");
}
    void OnGUI()
    {
        if (!unlimitedAmmo && isEquipped)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.LowerRight;
            
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(Screen.width - 151, Screen.height - 51, 150, 50), $"Ammo: {currentAmmo}/{maxAmmo}", style);
            
            GUI.contentColor = currentAmmo > 10 ? Color.white : Color.red;
            GUI.Label(new Rect(Screen.width - 150, Screen.height - 50, 150, 50), $"Ammo: {currentAmmo}/{maxAmmo}", style);
        }
    }
}