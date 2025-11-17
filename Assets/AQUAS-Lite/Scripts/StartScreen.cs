using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreen : MonoBehaviour
{
    public GameObject startScreenPanel;
    public Button startButton;
    
    [Header("Player Control")]
    public FirstPersonController playerController;
    public Transform playerTransform; 
    public Vector3 playerStartPosition;

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public float dialogueDuration = 4f;
    
    [Header("Video")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public Button skipButton;
    public VideoClip introVideo;
    
    [Header("Audio")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip buttonClickSound;
    
    [Header("Volume Control")]
    public GameObject volumePanel;
    public Slider musicVolumeSlider;
    public Button restartButton;
    public Button quitButton;

    private bool videoSkipped = false;
    public bool isGameStarted = false;
    private bool isCutscenePlaying = false;

    private AudioSource menuMusicSource;
    private AudioSource gameplayMusicSource;
    private AudioSource uiAudioSource;
    
    public static float MusicVolume { get; private set; } = 1.0f;
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        LoadVolumeSettings();
        SetupMenuMusic();
        SetupUIAudio();
        SetupVolumeControls();
        
        ResetToInitialState();
        
        startButton.onClick.AddListener(StartGame);
        startButton.onClick.AddListener(PlayClickSound);
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipVideo);
            skipButton.onClick.AddListener(PlayClickSound);
            skipButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // The 'V' key is the ONLY way to toggle the menu
        if (!isCutscenePlaying && Input.GetKeyDown(KeyCode.V))
        {
            ToggleVolumePanel();
        }
    }

    void SetupMenuMusic()
    {
        menuMusicSource = gameObject.AddComponent<AudioSource>();
        if (menuMusic != null)
        {
            menuMusicSource.clip = menuMusic;
            menuMusicSource.loop = true;
            menuMusicSource.playOnAwake = false;
            menuMusicSource.spatialBlend = 0f;
            menuMusicSource.volume = MusicVolume;
            menuMusicSource.Play();
        }
        else
        {
            Debug.LogWarning("Menu Music clip is not assigned!");
        }
    }

    void SetupUIAudio()
    {
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;
        uiAudioSource.volume = MusicVolume;
    }
    
    void SetupVolumeControls()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = MusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.onClick.AddListener(PlayClickSound);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            quitButton.onClick.AddListener(PlayClickSound);
        }
    }
    
    void LoadVolumeSettings()
    {
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
    }
    
    public void UpdateMusicVolume(float value)
    {
        MusicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        if (menuMusicSource != null) menuMusicSource.volume = value;
        if (gameplayMusicSource != null) gameplayMusicSource.volume = value;
        if (uiAudioSource != null) uiAudioSource.volume = value;
    }
    
    public void PlayClickSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    // *** CLEANED UP: No longer needs to manage a button's visibility ***
    public void ToggleVolumePanel()
    {
        bool isNowOpen = !volumePanel.activeSelf;
        volumePanel.SetActive(isNowOpen);

        if (isNowOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerController != null) playerController.cameraCanMove = false;
        }
        else
        {
            Time.timeScale = 1f;
            if (isGameStarted)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (playerController != null) playerController.cameraCanMove = true;
            }
            else
            {
                Cursor.visible = true;
            }
        }
    }
    
    void StartGame()
    {
        isCutscenePlaying = true;
        // Ensure the volume panel is closed and time is normal
        if (volumePanel != null && volumePanel.activeSelf) volumePanel.SetActive(false);
        Time.timeScale = 1f;
        if (menuMusicSource != null && menuMusicSource.isPlaying) menuMusicSource.Stop();
        
        startScreenPanel.SetActive(false);
        StartCoroutine(PlayIntroVideo());
    }
    
    IEnumerator PlayIntroVideo()
    {
        videoSkipped = false;
        if (videoPanel != null) videoPanel.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(true);
        
        if (videoPlayer != null && introVideo != null)
        {
            videoPlayer.clip = introVideo;
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared) yield return null;
            videoPlayer.Play();
            while (videoPlayer.isPlaying && !videoSkipped)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SkipVideo();
                yield return null;
            }
        }
        
        if (videoPanel != null) videoPanel.SetActive(false);
        if (skipButton != null) skipButton.gameObject.SetActive(false);
        
        StartCoroutine(ShowDialogue());
    }
    
    void SkipVideo()
    {
        videoSkipped = true;
        if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Stop();
    }
    
    IEnumerator ShowDialogue()
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "I've got to find my cat. These woods cannot be safe for him. I've got to hurry.";
            yield return new WaitForSeconds(dialogueDuration);
            dialoguePanel.SetActive(false);
        }
        
        isCutscenePlaying = false;
        isGameStarted = true;

        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.cameraCanMove = true; 
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartGameplayMusic();
    }
    
    void StartGameplayMusic()
    {
        gameplayMusicSource = GameObject.Find("GameplayMusicPlayer")?.GetComponent<AudioSource>();

        if (gameplayMusicSource == null)
        {
            GameObject musicPlayer = new GameObject("GameplayMusicPlayer");
            gameplayMusicSource = musicPlayer.AddComponent<AudioSource>();
            DontDestroyOnLoad(musicPlayer);
        }

        if (gameplayMusic != null)
        {
            gameplayMusicSource.clip = gameplayMusic;
            gameplayMusicSource.loop = true;
            gameplayMusicSource.playOnAwake = false;
            gameplayMusicSource.spatialBlend = 0f;
            gameplayMusicSource.volume = MusicVolume;
            gameplayMusicSource.Play();
        }
        else
        {
            Debug.LogWarning("Gameplay Music clip is not assigned!");
        }
    }

    public void RestartGame()
    {
        if (volumePanel != null && volumePanel.activeSelf)
        {
            volumePanel.SetActive(false);
        }
        ResetToInitialState();
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed. Application will close.");
        Application.Quit();
    }

    // *** CLEANED UP: No longer needs to manage a button's visibility ***
    private void ResetToInitialState()
    {
        isGameStarted = false;
        isCutscenePlaying = false;
        videoSkipped = false;

        if (gameplayMusicSource != null && gameplayMusicSource.isPlaying)
        {
            gameplayMusicSource.Stop();
        }

        if (menuMusicSource != null && !menuMusicSource.isPlaying)
        {
            menuMusicSource.Play();
        }

        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.cameraCanMove = false;
        }

        if (playerTransform != null)
        {
            playerTransform.position = playerStartPosition;
        }

        Time.timeScale = 1f;

        if (startScreenPanel != null) startScreenPanel.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (videoPanel != null) videoPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);

        if (skipButton != null) skipButton.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}