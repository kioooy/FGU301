using UnityEngine;

/// <summary>
/// ScriptableObject lưu toàn bộ AudioClip cho game.
/// Tạo asset: chuột phải Project → Create → Scriptable Objects → SoundData
/// </summary>
[CreateAssetMenu(fileName = "SoundData", menuName = "Scriptable Objects/SoundData")]
public class SoundData : ScriptableObject
{
    [Header("Background Music")]
    [Tooltip("Danh sách nhạc nền (sẽ được play ngẫu nhiên hoặc theo thứ tự)")]
    public AudioClip[] bgmClips;
    [Range(0f, 1f)] public float bgmVolume = 0.4f;

    [Header("SFX — Game Events")]
    public AudioClip towerShootClip;       // trụ bắn đạn
    public AudioClip enemyDeathClip;       // quái chết
    public AudioClip livesLostClip;        // mất máu (quái qua điểm cuối)
    public AudioClip coinRewardClip;       // cộng tiền

    [Header("SFX — Tower Actions")]
    public AudioClip towerPlacedClip;      // đặt trụ thành công
    public AudioClip towerUpgradedClip;    // nâng cấp trụ
    public AudioClip towerSoldClip;        // bán trụ
    public AudioClip notEnoughCoinsClip;   // không đủ tiền

    [Header("SFX — Wave")]
    public AudioClip waveStartClip;        // bắt đầu wave mới
    public AudioClip waveCountdownClip;    // tick đếm ngược

    [Header("SFX — Game Result")]
    public AudioClip victoryClip;          // thắng
    public AudioClip gameOverClip;         // thua

    [Header("SFX — UI")]
    public AudioClip buttonClickClip;      // click button chung
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
}
