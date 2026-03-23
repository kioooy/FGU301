#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Tạo âm thanh SFX bằng code (procedural synthesis) và lưu thành .wav asset.
/// Menu: Window > Tower Defense > Generate SFX
/// </summary>
public static class ProceduralSoundGenerator
{
    private const int SAMPLE_RATE = 44100;
    private const string OUTPUT_FOLDER = "Assets/Audioi/Generated";

    [MenuItem("Window/Tower Defense/Generate SFX")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(Application.dataPath + "/Audioi/Generated"))
            Directory.CreateDirectory(Application.dataPath + "/Audioi/Generated");

        GenerateTowerShoot();
        GenerateEnemyDeath();
        GenerateCoinReward();
        GenerateExplosion();
        GenerateUpgrade();
        GenerateGameOver();
        GenerateVictory();

        AssetDatabase.Refresh();
        AutoAssignToSoundData();

        Debug.Log("[ProceduralSFX] ✓ Đã tạo và gán tất cả SFX!");
    }

    // ─────────────────────────────────────────
    // 1. TOWER SHOOT — "Pew!" laser/bullet sound
    //    Sine sweep từ tần số cao xuống thấp, nhanh, sắc
    // ─────────────────────────────────────────
    private static void GenerateTowerShoot()
    {
        float duration = 0.18f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = (float)i / samples;

            // Sweep từ 1200Hz → 300Hz
            float freq = Mathf.Lerp(1200f, 300f, progress * progress);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);

            // Thêm chút noise cho texture
            float noise = (UnityEngine.Random.value - 0.5f) * 0.08f;

            // Envelope: attack nhanh, decay mượt
            float env = Mathf.Pow(1f - progress, 1.5f);

            data[i] = (sine + noise) * env * 0.55f;
        }

        SaveWav("SFX_TowerShoot", data);
    }

    // ─────────────────────────────────────────
    // 2. ENEMY DEATH — "Splat!" + burst noise
    //    Low-freq thump + decaying noise burst
    // ─────────────────────────────────────────
    private static void GenerateEnemyDeath()
    {
        float duration = 0.35f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = (float)i / samples;

            // Thump: sine thấp sweep xuống
            float thumpFreq = Mathf.Lerp(220f, 60f, progress * 2f);
            float thump = Mathf.Sin(2f * Mathf.PI * thumpFreq * t);

            // Noise burst: giảm nhanh
            float noise = (UnityEngine.Random.value - 0.5f) * 2f;
            float noiseEnv = Mathf.Exp(-progress * 12f);

            // Thump envelope: punch đầu, tắt dần
            float thumpEnv = Mathf.Exp(-progress * 7f);

            data[i] = (thump * thumpEnv * 0.6f + noise * noiseEnv * 0.5f);
            data[i] = Mathf.Clamp(data[i], -1f, 1f);
        }

        SaveWav("SFX_EnemyDeath", data);
    }

    // ─────────────────────────────────────────
    // 3. COIN REWARD — "Bling!" coin jingle
    //    2 note arpeggios: C5 → E5 → G5 (major chord)
    // ─────────────────────────────────────────
    private static void GenerateCoinReward()
    {
        float duration = 0.45f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        // 3 notes: C5=523Hz, E5=659Hz, G5=784Hz
        float[] freqs = { 523f, 659f, 784f };
        float noteLen = duration / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = (float)i / samples;
            int noteIdx = Mathf.FloorToInt(t / noteLen);
            if (noteIdx >= freqs.Length) noteIdx = freqs.Length - 1;

            float localT = t - noteIdx * noteLen;
            float localProgress = localT / noteLen;

            float freq = freqs[noteIdx];
            // Sine + harmonic cho âm sắc trong trẻo
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.2f
                       + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.1f;

            // Envelope mỗi note: attack nhanh, decay mượt
            float env = Mathf.Pow(1f - localProgress, 1.2f)
                      * Mathf.Min(1f, localProgress * 20f);

            // Fade toàn bộ âm thanh ở cuối
            float globalFade = Mathf.Pow(1f - progress, 0.5f);

            data[i] = wave * env * globalFade * 0.5f;
        }

        SaveWav("SFX_CoinReward", data);
    }

    // ─────────────────────────────────────────
    // 4. EXPLOSION — Noise burst, low freq punch
    //    Dùng cho boss death hoặc area damage
    // ─────────────────────────────────────────
    private static void GenerateExplosion()
    {
        float duration = 0.6f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = (float)i / samples;

            float noise = (UnityEngine.Random.value - 0.5f) * 2f;
            float sub = Mathf.Sin(2f * Mathf.PI * 80f * t);

            float noiseEnv = Mathf.Exp(-progress * 5f);
            float subEnv   = Mathf.Exp(-progress * 4f);

            data[i] = noise * noiseEnv * 0.65f + sub * subEnv * 0.5f;
            data[i] = Mathf.Clamp(data[i], -1f, 1f);
        }

        SaveWav("SFX_Explosion", data);
    }

    // ─────────────────────────────────────────
    // 5. UPGRADE — Rising arpeggio "power up"
    //    4 notes tăng dần, bright & satisfying
    // ─────────────────────────────────────────
    private static void GenerateUpgrade()
    {
        float duration = 0.5f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        float[] freqs = { 440f, 554f, 659f, 880f }; // A4, C#5, E5, A5
        float noteLen = duration / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            int noteIdx = Mathf.FloorToInt(t / noteLen);
            if (noteIdx >= freqs.Length) noteIdx = freqs.Length - 1;

            float localT = t - noteIdx * noteLen;
            float localProgress = localT / noteLen;
            float freq = freqs[noteIdx];

            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.75f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.15f;

            float env = Mathf.Pow(1f - localProgress, 1.5f)
                      * Mathf.Min(1f, localProgress * 25f);

            data[i] = wave * env * 0.5f;
        }

        SaveWav("SFX_Upgrade", data);
    }

    // ─────────────────────────────────────────
    // 6. GAME OVER — Descending minor chord, dark
    // ─────────────────────────────────────────
    private static void GenerateGameOver()
    {
        float duration = 1.2f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        // Dm chord notes descending: D4 → A3 → F3
        float[] freqs = { 294f, 220f, 175f };
        float noteLen = duration / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = (float)i / samples;
            int noteIdx = Mathf.FloorToInt(t / noteLen);
            if (noteIdx >= freqs.Length) noteIdx = freqs.Length - 1;

            float localProgress = (t - noteIdx * noteLen) / noteLen;
            float freq = freqs[noteIdx];

            // Âm trầm hơn với nhiều harmonics thấp
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                       + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.3f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.1f;

            float env = Mathf.Pow(1f - localProgress, 0.8f)
                      * Mathf.Min(1f, localProgress * 15f);

            float globalFade = 1f - progress * 0.3f;
            data[i] = wave * env * globalFade * 0.4f;
        }

        SaveWav("SFX_GameOver", data);
    }

    // ─────────────────────────────────────────
    // 7. VICTORY — Bright fanfare ascending
    // ─────────────────────────────────────────
    private static void GenerateVictory()
    {
        float duration = 1.0f;
        int samples = Mathf.CeilToInt(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        // C major: C4 E4 G4 C5
        float[] freqs = { 261f, 330f, 392f, 523f };
        float noteLen = duration / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = (float)i / samples;
            int noteIdx = Mathf.FloorToInt(t / noteLen);
            if (noteIdx >= freqs.Length) noteIdx = freqs.Length - 1;

            float localProgress = (t - noteIdx * noteLen) / noteLen;
            float freq = freqs[noteIdx];

            // Bright: fundamental + overtones
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.2f
                       + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.1f
                       + Mathf.Sin(2f * Mathf.PI * freq * 4f * t) * 0.05f;

            float env = Mathf.Pow(1f - localProgress, 1.0f)
                      * Mathf.Min(1f, localProgress * 20f);

            // Crescendo nhẹ ở cuối
            float globalVol = Mathf.Lerp(0.45f, 0.55f, progress);
            data[i] = wave * env * globalVol;
        }

        SaveWav("SFX_Victory", data);
    }

    // ─────────────────────────────────────────
    // WAV writer
    // ─────────────────────────────────────────
    private static void SaveWav(string name, float[] data)
    {
        string relativePath = $"{OUTPUT_FOLDER}/{name}.wav";
        string fullPath = Application.dataPath.Replace("Assets", "") + relativePath;

        using (FileStream fs = new FileStream(fullPath, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            int numChannels  = 1;
            int bitsPerSample = 16;
            int byteRate     = SAMPLE_RATE * numChannels * bitsPerSample / 8;
            int blockAlign   = numChannels * bitsPerSample / 8;
            int dataSize     = data.Length * blockAlign;

            // RIFF header
            bw.Write(new char[] { 'R','I','F','F' });
            bw.Write(36 + dataSize);
            bw.Write(new char[] { 'W','A','V','E' });
            // fmt chunk
            bw.Write(new char[] { 'f','m','t',' ' });
            bw.Write(16);               // chunk size
            bw.Write((short)1);         // PCM
            bw.Write((short)numChannels);
            bw.Write(SAMPLE_RATE);
            bw.Write(byteRate);
            bw.Write((short)blockAlign);
            bw.Write((short)bitsPerSample);
            // data chunk
            bw.Write(new char[] { 'd','a','t','a' });
            bw.Write(dataSize);
            foreach (float s in data)
            {
                short val = (short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue);
                bw.Write(val);
            }
        }

        Debug.Log($"[ProceduralSFX] Saved: {relativePath}");
    }

    // ─────────────────────────────────────────
    // Gán vào SoundData
    // ─────────────────────────────────────────
    private static void AutoAssignToSoundData()
    {
        SoundData data = AssetDatabase.LoadAssetAtPath<SoundData>(
            "Assets/ScriptableObjects/Audio/SoundData.asset");
        if (data == null)
        {
            Debug.LogWarning("[ProceduralSFX] Không tìm thấy SoundData. Chạy 'Initialize Audio System' trước.");
            return;
        }

        data.towerShootClip    = Load("SFX_TowerShoot");
        data.enemyDeathClip    = Load("SFX_EnemyDeath");
        data.coinRewardClip    = Load("SFX_CoinReward");
        data.towerUpgradedClip = Load("SFX_Upgrade");
        data.gameOverClip      = Load("SFX_GameOver");
        data.victoryClip       = Load("SFX_Victory");

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }

    private static AudioClip Load(string name) =>
        AssetDatabase.LoadAssetAtPath<AudioClip>($"{OUTPUT_FOLDER}/{name}.wav");
}
#endif
