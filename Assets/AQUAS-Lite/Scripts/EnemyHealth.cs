using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Death Settings")]
    public GameObject deathEffect; // Optional: Particle effect on death
    public AudioClip deathSound;
    public float deathEffectLifetime = 2f;
    public bool dropLoot = false;
    public GameObject lootPrefab;
    
    [Header("Hit Feedback")]
    public GameObject hitEffect; // Optional: Small effect when hit
    public AudioClip hitSound;
    public float hitEffectLifetime = 1f;
    
    private AudioSource audioSource;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Show hit effect
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, hitEffectLifetime);
        }
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"💀 {gameObject.name} died!");
        
        // Spawn death effect
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, deathEffectLifetime);
        }
        
        // Play death sound
        if (deathSound != null)
        {
            // Create temporary object to play sound
            GameObject soundObj = new GameObject($"{gameObject.name}_DeathSound");
            soundObj.transform.position = transform.position;
            AudioSource tempAudio = soundObj.AddComponent<AudioSource>();
            tempAudio.clip = deathSound;
            tempAudio.Play();
            Destroy(soundObj, deathSound.length);
        }
        
        // Drop loot
        if (dropLoot && lootPrefab != null)
        {
            Instantiate(lootPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        
        // Destroy enemy
        Destroy(gameObject);
    }

    // Optional: Heal enemy
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    // Optional: Show health bar above enemy
    void OnGUI()
    {
        if (isDead) return;
        
        // Only show health bar if damaged
        if (currentHealth < maxHealth)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
            
            if (screenPos.z > 0) // Only if in front of camera
            {
                float healthPercent = currentHealth / maxHealth;
                float barWidth = 100f;
                float barHeight = 10f;
                
                Rect bgRect = new Rect(screenPos.x - barWidth / 2, Screen.height - screenPos.y - barHeight / 2, barWidth, barHeight);
                Rect fgRect = new Rect(screenPos.x - barWidth / 2, Screen.height - screenPos.y - barHeight / 2, barWidth * healthPercent, barHeight);
                
                // Background (black)
                GUI.color = Color.black;
                GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
                
                // Foreground (red to green)
                GUI.color = Color.Lerp(Color.red, Color.green, healthPercent);
                GUI.DrawTexture(fgRect, Texture2D.whiteTexture);
                
                GUI.color = Color.white;
            }
        }
    }
}