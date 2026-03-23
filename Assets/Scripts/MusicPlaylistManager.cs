using UnityEngine;

public class MusicPlaylistManager : MonoBehaviour
{
    public AudioSource audioSource;        // AudioSource dùng để phát nhạc
    public AudioClip[] musicClips;         // Mảng chứa các bài nhạc
    private int currentIndex = 0;

    void Start()
    {
        if (musicClips.Length == 0)
        {
            Debug.LogWarning("Chưa có nhạc trong playlist!");
            return;
        }

        PlayCurrentTrack();
    }

    void Update()
    {
        // Nếu bài hiện tại đã phát hết, chuyển bài tiếp theo
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    void PlayCurrentTrack()
    {
        audioSource.clip = musicClips[currentIndex];
        audioSource.Play();
        Debug.Log($"Đang phát bài: {audioSource.clip.name}");
    }

    void NextTrack()
    {
        currentIndex++;
        if (currentIndex >= musicClips.Length)
        {
            currentIndex = 0; // Quay lại bài đầu nếu hết playlist
        }
        PlayCurrentTrack();
    }
}

