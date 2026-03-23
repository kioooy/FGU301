using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManagerWithMixer : MonoBehaviour
{
    public AudioMixer audioMixer; // Gán AudioMixer trong Inspector
    public Slider masterSlider;
    public Slider musicSlider;

    private void Start()
    {
        float masterPref = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicPref = PlayerPrefs.GetFloat("MusicVolume", 0.75f);

        masterSlider.value = masterPref;
        musicSlider.value = musicPref;

        // Gọi trực tiếp hàm set volume để update mixer khi start
        SetMasterVolume(masterPref);
        SetMusicVolume(musicPref);

        // Đăng ký sự kiện
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void SetMasterVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("MasterVolume", dB);
        PlayerPrefs.SetFloat("MasterVolume", value);
        Debug.Log($"MasterVolume set to {value} ({dB} dB)");
    }

    public void SetMusicVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("MusicVolume", dB);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}

