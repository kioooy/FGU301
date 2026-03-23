using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton quản lý toàn bộ âm thanh trong game.
/// Kéo SoundData vào Inspector. AudioManager tự tồn tại xuyên scene nếu cần.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] public SoundData soundData;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private int _currentBGMIndex = 0;
    private int _previousCoins = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Không DontDestroyOnLoad — AudioManager thuộc về scene cụ thể

        // Tạo 2 AudioSource component
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        ApplyVolume();
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed  += HandleEnemyDestroyed;
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        GameManager.OnCoinRewardChanged += HandleCoinChanged;
        GameManager.OnLivesChanged      += HandleLivesChanged;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed  -= HandleEnemyDestroyed;
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        GameManager.OnCoinRewardChanged -= HandleCoinChanged;
        GameManager.OnLivesChanged      -= HandleLivesChanged;
    }

    private void Start()
    {
        PlayBGM(0);
    }

    // ─────────────────────────────────────────
    //  Event handlers
    // ─────────────────────────────────────────

    private void HandleEnemyDestroyed(Enemy enemy)     => PlaySFX(soundData.enemyDeathClip);
    private void HandleEnemyReachedEnd(EnemyData data) => PlaySFX(soundData.livesLostClip);

    private void HandleCoinChanged(int newCoins)
    {
        // Chỉ phát âm khi tiền tăng (nhận thưởng), không phát khi tiêu tiền
        if (_previousCoins >= 0 && newCoins > _previousCoins)
            PlaySFX(soundData.coinRewardClip);
        _previousCoins = newCoins;
    }

    private void HandleLivesChanged(int newLives)
    {
        // Chỉ phát khi máu giảm
        PlaySFX(soundData.livesLostClip);
    }

    // ─────────────────────────────────────────
    //  Public API — dùng từ UIController, Spawner
    // ─────────────────────────────────────────

    public void PlayTowerPlaced()      => PlaySFX(soundData.towerPlacedClip);
    public void PlayTowerUpgraded()    => PlaySFX(soundData.towerUpgradedClip);
    public void PlayTowerSold()        => PlaySFX(soundData.towerSoldClip);
    public void PlayNotEnoughCoins()   => PlaySFX(soundData.notEnoughCoinsClip);
    public void PlayButtonClick()      => PlaySFX(soundData.buttonClickClip);
    public void PlayWaveStart()        => PlaySFX(soundData.waveStartClip);
    public void PlayWaveCountdown()    => PlaySFX(soundData.waveCountdownClip);
    public void PlayVictory()
    {
        StopBGM();
        PlaySFX(soundData.victoryClip);
    }
    public void PlayGameOver()
    {
        StopBGM();
        PlaySFX(soundData.gameOverClip);
    }

    // ─────────────────────────────────────────
    //  BGM control
    // ─────────────────────────────────────────

    public void PlayBGM(int index)
    {
        if (soundData == null || soundData.bgmClips == null || soundData.bgmClips.Length == 0) return;
        index = Mathf.Clamp(index, 0, soundData.bgmClips.Length - 1);
        _currentBGMIndex = index;

        if (soundData.bgmClips[index] == null) return;
        _bgmSource.clip = soundData.bgmClips[index];
        _bgmSource.volume = soundData.bgmVolume;
        _bgmSource.Play();
    }

    public void PlayRandomBGM()
    {
        if (soundData == null || soundData.bgmClips == null || soundData.bgmClips.Length == 0) return;
        int idx = Random.Range(0, soundData.bgmClips.Length);
        PlayBGM(idx);
    }

    public void StopBGM() => _bgmSource.Stop();

    public void SetBGMVolume(float v)
    {
        if (soundData != null) soundData.bgmVolume = v;
        _bgmSource.volume = v;
    }

    public void SetSFXVolume(float v)
    {
        if (soundData != null) soundData.sfxVolume = v;
        _sfxSource.volume = v;
    }

    // ─────────────────────────────────────────
    //  Core SFX player
    // ─────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || soundData == null) return;
        if (_sfxSource == null) return; // AudioSource destroyed on scene unload
        _sfxSource.PlayOneShot(clip, soundData.sfxVolume);
    }


    private void ApplyVolume()
    {
        if (soundData == null) return;
        _bgmSource.volume = soundData.bgmVolume;
    }
}
