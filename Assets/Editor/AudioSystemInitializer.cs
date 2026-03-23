#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tạo SoundData asset và gán clip mặc định khi chạy lần đầu.
/// KHÔNG tự chạy khi Play — chỉ chạy qua menu item.
/// Nếu slot đã có clip (ví dụ procedural sounds) thì KHÔNG ghi đè.
/// </summary>
public static class AudioSystemInitializer
{
    // Không còn [InitializeOnLoad] — tránh auto-run mỗi khi domain reload / Play
    [MenuItem("Window/Tower Defense/Initialize Audio System")]
    public static void CreateSoundDataIfMissing()
    {
        const string assetPath = "Assets/ScriptableObjects/Audio/SoundData.asset";
        const string bgmFolder  = "Assets/Audioi/8Bit Music - 062022";
        const string sfxFolder  = "Assets/Audioi/Free UI Click Sound Effects Pack/AUDIO";

        SoundData data = AssetDatabase.LoadAssetAtPath<SoundData>(assetPath);

        if (data == null)
        {
            if (!System.IO.Directory.Exists("Assets/ScriptableObjects/Audio"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Audio");

            data = ScriptableObject.CreateInstance<SoundData>();
            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log("[AudioSystem] Đã tạo SoundData asset tại: " + assetPath);
        }

        // ── BGM: chỉ gán nếu mảng trống ──
        if (data.bgmClips == null || data.bgmClips.Length == 0)
        {
            var bgmList = new System.Collections.Generic.List<AudioClip>();
            for (int i = 1; i <= 10; i++)
            {
                var clip = Load($"{bgmFolder}/{i}. Track {i}.wav");
                if (clip != null) bgmList.Add(clip);
            }
            data.bgmClips  = bgmList.ToArray();
            data.bgmVolume = 0.4f;
            data.sfxVolume = 0.7f;
        }

        // ── SFX: chỉ gán nếu slot đang null (không ghi đè clip đã có) ──
        AssignIfNull(ref data.towerPlacedClip,    Load($"{sfxFolder}/Metallic/SFX_UI_Click_Organic_Metallic_Plastic_Select_1.wav"));
        AssignIfNull(ref data.towerUpgradedClip,  Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Open_2.wav"));
        AssignIfNull(ref data.towerSoldClip,      Load($"{sfxFolder}/Metallic/SFX_UI_Click_Designed_Metallic_Negative_1.wav"));
        AssignIfNull(ref data.towerShootClip,     Load($"{sfxFolder}/Button/SFX_UI_Button_Mouse_Thick_Generic_1.wav"));
        AssignIfNull(ref data.notEnoughCoinsClip, Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Negative_Close_1.wav"));
        AssignIfNull(ref data.enemyDeathClip,     Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Thick_Generic_1.wav"));
        AssignIfNull(ref data.livesLostClip,      Load($"{sfxFolder}/Metallic/SFX_UI_Click_Designed_Metallic_Dirty_Negative_1.wav"));
        AssignIfNull(ref data.coinRewardClip,     Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Mallet_Open_1.wav"));
        AssignIfNull(ref data.waveStartClip,      Load($"{sfxFolder}/Button/SFX_UI_Button_Keyboard_Enter_Thick_1.wav"));
        AssignIfNull(ref data.waveCountdownClip,  Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Generic_1.wav"));
        AssignIfNull(ref data.victoryClip,        Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Movement_Open_1.wav"));
        AssignIfNull(ref data.gameOverClip,       Load($"{sfxFolder}/Metallic/SFX_UI_Click_Designed_Metallic_Pop_Negative_Locked_1.wav"));
        AssignIfNull(ref data.buttonClickClip,    Load($"{sfxFolder}/Button/SFX_UI_Button_Organic_Plastic_Thin_Generic_1.wav"));

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        EnsureAudioManagerInScene(data);
        Debug.Log("[AudioSystem] ✓ SoundData đã được khởi tạo.");
    }

    private static void AssignIfNull(ref AudioClip slot, AudioClip candidate)
    {
        if (slot == null) slot = candidate;
    }

    private static void EnsureAudioManagerInScene(SoundData data)
    {
        AudioManager existing = Object.FindFirstObjectByType<AudioManager>();
        if (existing != null)
        {
            if (existing.soundData == null)
            {
                existing.soundData = data;
                EditorUtility.SetDirty(existing);
            }
            return;
        }

        GameObject go = new GameObject("AudioManager");
        AudioManager am = go.AddComponent<AudioManager>();
        am.soundData = data;
        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[AudioSystem] ✓ Đã thêm AudioManager vào scene. Nhớ Ctrl+S.");
    }

    private static AudioClip Load(string path) =>
        AssetDatabase.LoadAssetAtPath<AudioClip>(path);
}
#endif
