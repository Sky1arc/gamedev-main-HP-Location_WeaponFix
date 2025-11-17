using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueSystem : MonoBehaviour
{
    [Header("Clue Settings")]
    public KeyCode viewClueKey = KeyCode.C; // Press C to view clue
    public string currentClue = ""; // The active clue
    public float clueDisplayTime = 5f;
    
    [Header("UI Settings")]
    public bool showKeyPrompt = true;
    public string keyPromptText = "[C] View Clue";
    
    private bool hasActiveClue = false;
    private bool isShowingClue = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager == null)
        {
            Debug.LogError("⚠️ ClueSystem: GameManager not found!");
        }
    }

    void Update()
    {
        // Only allow viewing clue if there's an active clue and not currently showing it
        if (hasActiveClue && !isShowingClue && Input.GetKeyDown(viewClueKey))
        {
            ShowClue();
        }
    }

    void OnGUI()
    {
        // Show key prompt in bottom-left corner
        if (hasActiveClue && showKeyPrompt && !isShowingClue)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.LowerLeft;
            
            // Add shadow for better visibility
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(11, Screen.height - 51, 200, 50), keyPromptText, style);
            
            GUI.contentColor = Color.yellow;
            GUI.Label(new Rect(10, Screen.height - 50, 200, 50), keyPromptText, style);
        }
    }

    public void SetClue(string clue)
    {
        currentClue = clue;
        hasActiveClue = true;
        Debug.Log($"✓ Clue set: {clue}");
    }

    public void ClearClue()
    {
        currentClue = "";
        hasActiveClue = false;
        Debug.Log("✓ Clue cleared");
    }

    void ShowClue()
    {
        if (gameManager != null && !string.IsNullOrEmpty(currentClue))
        {
            isShowingClue = true;
            gameManager.ShowMessage($"CLUE: {currentClue}", clueDisplayTime);
            StartCoroutine(ResetClueDisplay());
            Debug.Log($"📢 Showing clue: {currentClue}");
        }
    }

    IEnumerator ResetClueDisplay()
    {
        yield return new WaitForSeconds(clueDisplayTime);
        isShowingClue = false;
    }

    public bool HasActiveClue()
    {
        return hasActiveClue;
    }
}