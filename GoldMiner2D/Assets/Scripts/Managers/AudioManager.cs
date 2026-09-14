using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                _instance = audioObj.AddComponent<AudioManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Audio Sources")]
    public AudioSource musicSource;   // Phát nhạc nền
    public AudioSource sfxSource;     // Phát hiệu ứng âm thanh (tiếng click, tiếng gắp,...)

    [Header("Audio Clips")]
    public AudioClip bgmMenu;         // Nhạc nền màn hình Menu
    public AudioClip bgmPlaying;      // Nhạc nền khi đang chơi
    public AudioClip clickSound;      // Tiếng bấm nút
    public AudioClip hookShootSound;  // Tiếng phóng móc
    public AudioClip catchItemSound;  // Tiếng gắp được vàng/vật phẩm
    public AudioClip bombExplosionSound; // Tiếng nổ bom
    public AudioClip winSound;        // Tiếng qua màn (Thắng)
    public AudioClip loseSound;       // Tiếng thua cuộc

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        EnsureAudioSources();
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null || musicClip == null) return;

        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxSource != null && sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip);
        }
    }

    public void PlayClick() => PlaySFX(clickSound);
    public void PlayHookShoot() => PlaySFX(hookShootSound);
    public void PlayCatchItem() => PlaySFX(catchItemSound);
    public void PlayBombExplosion() => PlaySFX(bombExplosionSound != null ? bombExplosionSound : explosionSoundFallback);
    public void PlayWin() => PlaySFX(winSound);
    public void PlayLose() => PlaySFX(loseSound);

    private AudioClip explosionSoundFallback => bombExplosionSound != null ? bombExplosionSound : null;
}