using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deathScreenPanel;
    public TextMeshProUGUI deathText;
    public Button restartButton;
    public Button quitButton;
    
    [Header("Settings")]
    public float fadeInDuration = 1f;
    public string sceneToReload;
    
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Get or add canvas group for fade effect
        canvasGroup = deathScreenPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = deathScreenPanel.AddComponent<CanvasGroup>();
        }
        
        // Hide death screen at start
        deathScreenPanel.SetActive(false);
        
        // Get current scene name if not set
        if (string.IsNullOrEmpty(sceneToReload))
        {
            sceneToReload = SceneManager.GetActiveScene().name;
        }
        
        // Add button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowDeathScreen()
    {
        deathScreenPanel.SetActive(true);
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Start fade in
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    void RestartGame()
    {
        Time.timeScale = 1f; // Resume time in case it was paused
        
        // Hide death screen
        deathScreenPanel.SetActive(false);
        
        // Respawn player at last save point
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Respawn();
            }
        }
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}