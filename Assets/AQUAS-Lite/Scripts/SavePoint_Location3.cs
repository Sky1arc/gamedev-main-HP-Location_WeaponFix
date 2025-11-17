using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint_Location3 : MonoBehaviour
{
    [Header("Save Point Settings")]
    public string savePointName = "Location 3";
    public bool showActivationMessage = true;
    public float messageDisplayTime = 3f;

    [Header("Barrier Integration")]
    public LocationBarrier_DefeatEnemy linkedBarrier; // Barrier that LOCKS when save point is triggered

    [Header("Enemy Integration")]
    public List<GameObject> enemiesToDefeat = new List<GameObject>(); // Manually assign enemies
    public bool autoFindEnemies = true; // Or automatically find them in barrier area

    [Header("Clue System")]
    public bool showClue = true;
    public string clueMessage = "Defeat all enemies to proceed!";
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

        // LOCK the barrier and assign enemies
        if (linkedBarrier != null)
        {
            // If manually assigned enemies, add them to barrier
            if (!autoFindEnemies && enemiesToDefeat.Count > 0)
            {
                linkedBarrier.enemiesInArea.Clear();
                foreach (GameObject enemy in enemiesToDefeat)
                {
                    if (enemy != null)
                    {
                        linkedBarrier.AddEnemy(enemy);
                    }
                }
            }

            // Lock the barrier (this will auto-find enemies if autoFindEnemies is true)
            linkedBarrier.LockBarrier();
            Debug.Log($"✅ Calling LockBarrier() for {savePointName}");

            // Show enemy count message
            if (gameManager != null)
            {
                int enemyCount = linkedBarrier.enemiesInArea.Count;
                gameManager.ShowMessage($"A Demonic Goblin has appeared! Defeat {enemyCount} enemies to proceed.", messageDisplayTime + 1f);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No linked barrier found for {savePointName}! Drag Location3_Barrier into the Linked Barrier field.");
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

        // Unlock barrier (no UnlockBarrier method needed - auto unlocks when enemies defeated)
        if (linkedBarrier != null)
        {
            linkedBarrier.isActive = false;
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

        // Draw lines to manually assigned enemies
        if (!autoFindEnemies && enemiesToDefeat.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (GameObject enemy in enemiesToDefeat)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
    }
}