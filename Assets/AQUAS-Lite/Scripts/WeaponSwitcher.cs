using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapon References")]
    public GameObject grimoireWeapon; // Your grimoire GameObject
    public GrimoireWeapon grimoireScript; // Reference to the grimoire script
    
    [Header("Hands (Optional)")]
    public GameObject handsModel; // Optional: Empty hands model/viewmodel
    
    [Header("Weapon Switching")]
    public KeyCode handsKey = KeyCode.Alpha1; // Press 1 for hands (no weapon)
    public KeyCode grimoireKey = KeyCode.Alpha2; // Press 2 for grimoire
    
    [Header("Settings")]
    public bool startWithGrimoire = false; // Start with hands or grimoire?
    public float switchDelay = 0.2f; // Small delay between switches
    
    [Header("Audio (Optional)")]
    public AudioClip equipSound;
    public AudioClip unequipSound;
    
    private AudioSource audioSource;
    private bool canSwitch = true;
    private int currentWeapon = 1; // 1 = hands, 2 = grimoire

    void Start()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Auto-find grimoire if not assigned
        if (grimoireWeapon == null && grimoireScript != null)
        {
            grimoireWeapon = grimoireScript.gameObject;
        }
        
        if (grimoireWeapon != null && grimoireScript == null)
        {
            grimoireScript = grimoireWeapon.GetComponent<GrimoireWeapon>();
        }
        
        // Set initial weapon state
        if (startWithGrimoire)
        {
            SwitchToGrimoire();
        }
        else
        {
            SwitchToHands();
        }
    }

    void Update()
    {
        if (!canSwitch) return;
        
        // Switch to hands (1 key)
        if (Input.GetKeyDown(handsKey) && currentWeapon != 1)
        {
            SwitchToHands();
        }
        
        // Switch to grimoire (2 key)
        if (Input.GetKeyDown(grimoireKey) && currentWeapon != 2)
        {
            SwitchToGrimoire();
        }
        
        // Alternative: Scroll wheel switching (optional)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // Scroll up
        {
            CycleWeapon(1);
        }
        else if (scroll < 0f) // Scroll down
        {
            CycleWeapon(-1);
        }
    }

    void SwitchToHands()
    {
        if (!canSwitch) return;
        
        currentWeapon = 1;
        Debug.Log("🖐️ Switched to hands");
        
        // Hide grimoire
        if (grimoireWeapon != null)
        {
            grimoireWeapon.SetActive(false);
        }
        
        // Disable grimoire script
        if (grimoireScript != null)
        {
            grimoireScript.SetEquipped(false);
        }
        
        // Show hands model (if you have one)
        if (handsModel != null)
        {
            handsModel.SetActive(true);
        }
        
        // Play unequip sound
        if (audioSource != null && unequipSound != null)
        {
            audioSource.PlayOneShot(unequipSound);
        }
        
        StartCoroutine(SwitchCooldown());
    }

    void SwitchToGrimoire()
    {
        if (!canSwitch) return;
        
        currentWeapon = 2;
        Debug.Log("📖 Switched to grimoire");
        
        // Show grimoire
        if (grimoireWeapon != null)
        {
            grimoireWeapon.SetActive(true);
        }
        
        // Enable grimoire script
        if (grimoireScript != null)
        {
            grimoireScript.SetEquipped(true);
        }
        
        // Hide hands model
        if (handsModel != null)
        {
            handsModel.SetActive(false);
        }
        
        // Play equip sound
        if (audioSource != null && equipSound != null)
        {
            audioSource.PlayOneShot(equipSound);
        }
        
        StartCoroutine(SwitchCooldown());
    }

    void CycleWeapon(int direction)
    {
        if (!canSwitch) return;
        
        currentWeapon += direction;
        
        // Wrap around (1 to 2, 2 to 1)
        if (currentWeapon > 2)
            currentWeapon = 1;
        else if (currentWeapon < 1)
            currentWeapon = 2;
        
        // Switch to the weapon
        if (currentWeapon == 1)
            SwitchToHands();
        else if (currentWeapon == 2)
            SwitchToGrimoire();
    }

    IEnumerator SwitchCooldown()
    {
        canSwitch = false;
        yield return new WaitForSeconds(switchDelay);
        canSwitch = true;
    }

    // Display current weapon on screen
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.LowerLeft;
        
        string weaponName = currentWeapon == 1 ? "Hands" : "Grimoire";
        string keyHint = currentWeapon == 1 ? "[2] Grimoire" : "[1] Hands";
        
        // Shadow
        GUI.contentColor = Color.black;
        GUI.Label(new Rect(11, Screen.height - 91, 200, 50), $"Weapon: {weaponName}", style);
        GUI.Label(new Rect(11, Screen.height - 71, 200, 50), keyHint, style);
        
        // Main text
        GUI.contentColor = Color.yellow;
        GUI.Label(new Rect(10, Screen.height - 90, 200, 50), $"Weapon: {weaponName}", style);
        
        GUI.contentColor = Color.gray;
        GUI.Label(new Rect(10, Screen.height - 70, 200, 50), keyHint, style);
    }

    // Public method to force switch (useful for cutscenes, etc.)
    public void ForceWeapon(int weaponNumber)
    {
        if (weaponNumber == 1)
            SwitchToHands();
        else if (weaponNumber == 2)
            SwitchToGrimoire();
    }
}