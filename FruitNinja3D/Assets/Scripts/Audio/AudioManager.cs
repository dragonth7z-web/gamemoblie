using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource comboSource; // ✨ Thêm AudioSource riêng biệt cho Combo SFX

    [Header("Audio Clips - Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip gameOverMusic;

    [Header("Audio Clips - Gameplay SFX")]
    public AudioClip[] swishSounds;
    public AudioClip[] cutSounds;
    public AudioClip bombSound;
    public AudioClip fuseSound;
    public AudioClip comboSound;

    [Header("Audio Clips - UI & Game State")]
    public AudioClip buttonClickSound;
    public AudioClip gameOverSound;

    [Header("Cài đặt Độ Trầm Bổng (Pitch Variance)")]
    [Range(0.8f, 1.2f)] public float minPitch = 0.85f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.15f;

    private float lastSwishTime = 0f;
    private float lastCutTime = 0f;
    private const float MIN_SOUND_INTERVAL = 0.05f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadAudioSettings();
        PlayMenuMusic();
    }

    // -------------------------------------------------------------
    // QUẢN LÝ NHẠC NỀN
    // -------------------------------------------------------------
    public void PlayMenuMusic() => PlayMusicClip(menuMusic, true, false);

    public void PlayGameplayMusic()
    {
        PlayMusicClip(gameplayMusic, true, true);
    }

    public void PlayGameOverMusic()
    {
        StopComboSound();
    StopSFX();
        PlayMusicClip(gameOverMusic, false, true);
    }

    private void PlayMusicClip(AudioClip clip, bool isLooping, bool forceRestart)
    {
        if (clip == null || musicSource == null) return;

        if (!forceRestart && musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = isLooping;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void StopSFX()
    {
        if (sfxSource != null) sfxSource.Stop();
        StopComboSound(); // Dừng luôn cả âm thanh Combo khi gọi ngắt SFX
    }

    // -------------------------------------------------------------
    // PHÂN LOẠI ÂM THANH SFX
    // -------------------------------------------------------------
    public void PlaySwishSound()
    {
        if (Time.time - lastSwishTime < MIN_SOUND_INTERVAL) return;
        lastSwishTime = Time.time;
        PlayRandomSFX(swishSounds);
    }

    public void PlayCutSound()
    {
        if (Time.time - lastCutTime < MIN_SOUND_INTERVAL) return;
        lastCutTime = Time.time;
        PlayRandomSFX(cutSounds);
    }

    private void PlayRandomSFX(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0 && sfxSource != null)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayFuseSound()
    {
        if (fuseSound != null && sfxSource != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(fuseSound, 0.7f);
        }
    }

    // ✨ QUẢN LÝ RIÊNG ÂM THANH COMBO
    public void PlayComboSound()
    {
        if (comboSound == null) return;

        // Nếu có comboSource riêng, phát trực tiếp để dễ quản lý ngắt
        if (comboSource != null)
        {
            comboSource.clip = comboSound;
            comboSource.pitch = 1.0f;
            comboSource.Play(); // Dùng Play() thay vì PlayOneShot() để có thể ngắt bất kỳ lúc nào
        }
        else if (sfxSource != null)
        {
            sfxSource.PlayOneShot(comboSound);
        }
    }

    // ✨ Hàm ngắt lập tức âm thanh Combo khi hết lượt chém/đủ điều kiện ngắt
    public void StopComboSound()
    {
        if (comboSource != null && comboSource.isPlaying)
        {
            comboSource.Stop();
        }
    }

    public void PlayBombSound() => PlaySFXFixedPitch(bombSound);
    public void PlayButtonClick() => PlaySFXFixedPitch(buttonClickSound);
    public void PlayGameOverSound() => PlaySFXFixedPitch(gameOverSound);

    private void PlaySFXFixedPitch(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(clip);
        }
    }

    // -------------------------------------------------------------
    // CÀI ĐẶT ÂM LƯỢNG
    // -------------------------------------------------------------
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = volume;
        if (comboSource != null) comboSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void ToggleMusic(bool isMuted)
    {
        if (musicSource != null) musicSource.mute = isMuted;
        PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX(bool isMuted)
    {
        if (sfxSource != null) sfxSource.mute = isMuted;
        if (comboSource != null) comboSource.mute = isMuted;
        PlayerPrefs.SetInt("SFXMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool sfxMute = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        if (musicSource != null)
        {
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSource.mute = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVol;
            sfxSource.mute = sfxMute;
        }

        if (comboSource != null)
        {
            comboSource.volume = sfxVol;
            comboSource.mute = sfxMute;
        }
    }
}