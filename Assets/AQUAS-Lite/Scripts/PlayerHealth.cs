using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Audio Settings")]
    public AudioClip damageSound; // Drag your damage sound clip here
    public AudioClip deathSound;   // <-- NEW: Drag your death sound clip here
    private AudioSource audioSource; // Reference to the AudioSource component

    [Header("Death Settings")]
    public float respawnDelay = 2f;
    public DeathScreen deathScreen;

    [Header("UI References")]
    public FirstPersonController playerController; // Reference to update health globe

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Get or Add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            // You might want to set default AudioSource settings here, e.g.,
            audioSource.playOnAwake = false;
        }

        // Auto-find FirstPersonController if not assigned
        if (playerController == null)
        {
            playerController = GetComponent<FirstPersonController>();
            if (playerController == null)
            {
                playerController = FindObjectOfType<FirstPersonController>();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // IMPORTANT: Check if the player is actually taking damage (e.g., damage > 0)
        if (damage > 0)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // ✅ Clamp to prevent negative

            Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

            // 🔊 Play Damage Sound
            if (damageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageSound);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("💀 Player died!");

        // 🔊 Play Death Sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Optional: Disable player controls
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this && script.enabled)
            {
                script.enabled = false;
            }
        }

        // Show death screen
        if (deathScreen != null)
        {
            Invoke("ShowDeathScreenDelayed", respawnDelay);
        }
    }

    void ShowDeathScreenDelayed()
    {
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
    }

    public void Respawn()
    {
        // Reset health
        currentHealth = maxHealth;
        isDead = false;

        Debug.Log("✅ Player respawned with full health");

        // Re-enable player controls
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = true;
            }
        }

        // Tell GameManager to respawn player at save point
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RespawnPlayer();
        }
    }

    // Optional: Heal function
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // ✅ Clamp to max

        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}");
    }
}