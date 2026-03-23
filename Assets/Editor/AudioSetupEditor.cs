#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Editor Window để setup âm thanh cho game nhanh chóng.
/// Mở qua: Window > Tower Defense > Audio Setup
/// </summary>
public class AudioSetupEditor : EditorWindow
{
    private SoundData _soundData;
    private Vector2 _scroll;
    private int _activeTab = 0;
    private readonly string[] _tabs = { "🎵 BGM", "🔊 SFX" };

    // Preview
    private AudioSource _previewSource;

    // Style cache
    private GUIStyle _headerStyle;
    private GUIStyle _sectionStyle;
    private bool _stylesInit;

    [MenuItem("Window/Tower Defense/Audio Setup")]
    public static void ShowWindow()
    {
        var w = GetWindow<AudioSetupEditor>("🔊 Audio Setup");
        w.minSize = new Vector2(480, 600);
    }

    private void OnEnable()
    {
        // Tự tìm SoundData asset đầu tiên trong project
        string[] guids = AssetDatabase.FindAssets("t:SoundData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _soundData = AssetDatabase.LoadAssetAtPath<SoundData>(path);
        }
    }

    private void OnDisable()
    {
        StopPreview();
    }

    private void InitStyles()
    {
        if (_stylesInit) return;
        _stylesInit = true;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleLeft
        };
        _sectionStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 8)
        };
    }

    private void OnGUI()
    {
        InitStyles();

        // ── Top bar ──
        DrawTopBar();
        EditorGUILayout.Space(4);

        if (_soundData == null)
        {
            DrawNoDataWarning();
            return;
        }

        // ── Tabs ──
        _activeTab = GUILayout.Toolbar(_activeTab, _tabs, GUILayout.Height(32));
        EditorGUILayout.Space(4);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUI.BeginChangeCheck();

        if (_activeTab == 0) DrawBGMTab();
        else                 DrawSFXTab();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(_soundData);

        EditorGUILayout.EndScrollView();

        // ── Bottom actions ──
        EditorGUILayout.Space(6);
        DrawBottomActions();
    }

    // ─────────────────────────────────────────
    //  Top bar
    // ─────────────────────────────────────────

    private void DrawTopBar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.toolbar);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("SoundData Asset:", GUILayout.Width(110));
        _soundData = (SoundData)EditorGUILayout.ObjectField(_soundData, typeof(SoundData), false);

        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            CreateNewSoundData();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    // ─────────────────────────────────────────
    //  BGM Tab
    // ─────────────────────────────────────────

    private void DrawBGMTab()
    {
        EditorGUILayout.BeginVertical(_sectionStyle);
        EditorGUILayout.LabelField("Background Music", _headerStyle);
        EditorGUILayout.Space(4);

        _soundData.bgmVolume = EditorGUILayout.Slider("BGM Volume", _soundData.bgmVolume, 0f, 1f);
        EditorGUILayout.Space(6);

        // BGM clips array
        int removeIdx = -1;
        if (_soundData.bgmClips == null) _soundData.bgmClips = new AudioClip[0];

        for (int i = 0; i < _soundData.bgmClips.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Track {i + 1}", GUILayout.Width(60));
            _soundData.bgmClips[i] = (AudioClip)EditorGUILayout.ObjectField(_soundData.bgmClips[i], typeof(AudioClip), false);

            DrawPreviewButton(_soundData.bgmClips[i]);

            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("✕", GUILayout.Width(26))) removeIdx = i;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        if (removeIdx >= 0) ArrayUtility.RemoveAt(ref _soundData.bgmClips, removeIdx);

        EditorGUILayout.Space(4);
        GUI.backgroundColor = new Color(0.2f, 0.6f, 0.3f);
        if (GUILayout.Button("＋  Add BGM Track", GUILayout.Height(26)))
            ArrayUtility.Add(ref _soundData.bgmClips, null);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    // ─────────────────────────────────────────
    //  SFX Tab
    // ─────────────────────────────────────────

    private void DrawSFXTab()
    {
        SerializedObject so = new SerializedObject(_soundData);
        so.Update();

        _soundData.sfxVolume = EditorGUILayout.Slider("SFX Volume", _soundData.sfxVolume, 0f, 1f);
        EditorGUILayout.Space(6);

        DrawSFXSection(so, "⚔️  Tower Actions", new[]
        {
            ("Tower Placed",     "towerPlacedClip"),
            ("Tower Upgraded",   "towerUpgradedClip"),
            ("Tower Sold",       "towerSoldClip"),
            ("Tower Shot",       "towerShootClip"),
            ("Not Enough Coins", "notEnoughCoinsClip"),
        });

        DrawSFXSection(so, "👾  Enemy Events", new[]
        {
            ("Enemy Death",  "enemyDeathClip"),
            ("Lives Lost",   "livesLostClip"),
            ("Coin Reward",  "coinRewardClip"),
        });

        DrawSFXSection(so, "🌊  Wave", new[]
        {
            ("Wave Start",     "waveStartClip"),
            ("Wave Countdown", "waveCountdownClip"),
        });

        DrawSFXSection(so, "🏆  Game Result", new[]
        {
            ("Victory",   "victoryClip"),
            ("Game Over", "gameOverClip"),
        });

        DrawSFXSection(so, "🖱️  UI", new[]
        {
            ("Button Click", "buttonClickClip"),
        });

        so.ApplyModifiedProperties();
    }

    private void DrawSFXSection(SerializedObject so, string title, (string label, string propName)[] slots)
    {
        EditorGUILayout.BeginVertical(_sectionStyle);
        EditorGUILayout.LabelField(title, _headerStyle);
        EditorGUILayout.Space(4);

        foreach (var (label, propName) in slots)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.PropertyField(prop, GUIContent.none);
            AudioClip currentClip = prop.objectReferenceValue as AudioClip;
            DrawPreviewButton(currentClip);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    // ─────────────────────────────────────────
    //  Bottom actions
    // ─────────────────────────────────────────

    private void DrawBottomActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        // Auto-assign
        GUI.backgroundColor = new Color(0.3f, 0.5f, 0.9f);
        if (GUILayout.Button("⚡  Auto-Assign Suggestions", GUILayout.Height(30)))
            AutoAssign();
        GUI.backgroundColor = Color.white;

        // Add to scene
        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.4f);
        if (GUILayout.Button("🎮  Add AudioManager to Scene", GUILayout.Height(30)))
            AddAudioManagerToScene();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // Save / Stop preview
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💾  Save SoundData", GUILayout.Height(26)))
        {
            EditorUtility.SetDirty(_soundData);
            AssetDatabase.SaveAssets();
            Debug.Log("[AudioSetup] SoundData saved.");
        }
        GUI.backgroundColor = new Color(0.7f, 0.3f, 0.3f);
        if (GUILayout.Button("⏹  Stop Preview", GUILayout.Height(26)))
            StopPreview();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    // ─────────────────────────────────────────
    //  Auto-Assign
    // ─────────────────────────────────────────

    private void AutoAssign()
    {
        if (_soundData == null) return;

        Undo.RecordObject(_soundData, "Auto-Assign Audio");

        string bgmFolder = "Assets/Audioi/8Bit Music - 062022";
        string sfxFolder = "Assets/Audioi/Free UI Click Sound Effects Pack/AUDIO";

        // ── BGM: load Track 1-10 ──
        var bgmList = new System.Collections.Generic.List<AudioClip>();
        for (int i = 1; i <= 10; i++)
        {
            string path = $"{bgmFolder}/{i}. Track {i}.wav";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null) bgmList.Add(clip);
        }
        _soundData.bgmClips = bgmList.ToArray();

        // ── SFX: gán clip phù hợp theo chức năng ──
        // Tower placed → Metallic select (cảm giác "chắc chắn")
        _soundData.towerPlacedClip    = Load($"{sfxFolder}/Metallic/SFX_UI_Click_Organic_Metallic_Plastic_Select_1.wav");
        // Tower upgraded → Pop open (cảm giác "mở ra thứ gì đó")  
        _soundData.towerUpgradedClip  = Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Open_2.wav");
        // Tower sold → Metallic negative
        _soundData.towerSoldClip      = Load($"{sfxFolder}/Metallic/SFX_UI_Click_Designed_Metallic_Negative_1.wav");
        // Tower shoot → Button thick (nhanh, sắc)
        _soundData.towerShootClip     = Load($"{sfxFolder}/Button/SFX_UI_Button_Mouse_Thick_Generic_1.wav");
        // Not enough coins → Pop negative
        _soundData.notEnoughCoinsClip = Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Negative_Close_1.wav");
        // Enemy death → Pop thick
        _soundData.enemyDeathClip     = Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Thick_Generic_1.wav");
        // Lives lost → Metallic dirty negative (nặng nề)
        _soundData.livesLostClip      = Load($"{sfxFolder}/Metallic/SFX_UI_Click_Designed_Metallic_Dirty_Negative_1.wav");
        // Coin reward → Pop mallet open (nhẹ nhàng, vui)
        _soundData.coinRewardClip     = Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Mallet_Open_1.wav");
        // Wave start → Button keyboard enter thick
        _soundData.waveStartClip      = Load($"{sfxFolder}/Button/SFX_UI_Button_Keyboard_Enter_Thick_1.wav");
        // Wave countdown → Pop open (nhẹ)
        _soundData.waveCountdownClip  = Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Generic_1.wav");
        // Victory → Pop movement open (rộng, thoáng)
        _soundData.victoryClip        = Load($"{sfxFolder}/Pop/SFX_UI_Click_Designed_Pop_Movement_Open_1.wav");
        // Game over → Metallic pop negative locked (nặng, tĩnh lặng)
        _soundData.gameOverClip       = Load($"{sfxFolder}/Metallic/SFX_UI_Click_Designed_Metallic_Pop_Negative_Locked_1.wav");
        // Button click → Button organic plastic thin
        _soundData.buttonClickClip    = Load($"{sfxFolder}/Button/SFX_UI_Button_Organic_Plastic_Thin_Generic_1.wav");

        EditorUtility.SetDirty(_soundData);
        AssetDatabase.SaveAssets();
        Debug.Log("[AudioSetup] Auto-assigned audio clips.");
    }

    private AudioClip Load(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);

    // ─────────────────────────────────────────
    //  Add AudioManager to Scene
    // ─────────────────────────────────────────

    private void AddAudioManagerToScene()
    {
        // Nếu đã có thì không tạo thêm
        AudioManager existing = FindFirstObjectByType<AudioManager>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            Debug.Log("[AudioSetup] AudioManager đã tồn tại trong scene.");
            return;
        }

        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create AudioManager");

        // Gán SoundData nếu có
        if (_soundData != null)
        {
            var am = go.GetComponent<AudioManager>();
            SerializedObject so = new SerializedObject(am);
            so.FindProperty("soundData").objectReferenceValue = _soundData;
            so.ApplyModifiedProperties();
        }

        Selection.activeGameObject = go;
        Debug.Log("[AudioSetup] AudioManager đã được thêm vào scene.");
    }

    // ─────────────────────────────────────────
    //  Preview audio
    // ─────────────────────────────────────────

    private void DrawPreviewButton(AudioClip clip)
    {
        GUI.enabled = clip != null;
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("▶", GUILayout.Width(26), GUILayout.Height(18)))
            PreviewClip(clip);
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
    }

    private void PreviewClip(AudioClip clip)
    {
        if (clip == null) return;
        StopPreview();

        // Tạo AudioSource ẩn trong scene để preview
        GameObject previewGO = EditorUtility.CreateGameObjectWithHideFlags(
            "AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource));
        _previewSource = previewGO.GetComponent<AudioSource>();
        _previewSource.clip = clip;
        _previewSource.Play();
    }

    private void StopPreview()
    {
        if (_previewSource != null)
        {
            _previewSource.Stop();
            DestroyImmediate(_previewSource.gameObject);
            _previewSource = null;
        }
    }



    // ─────────────────────────────────────────
    //  Create new SoundData asset
    // ─────────────────────────────────────────

    private void CreateNewSoundData()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create SoundData", "SoundData", "asset",
            "Chọn nơi lưu SoundData asset",
            "Assets/ScriptableObjects/Audio");

        if (string.IsNullOrEmpty(path)) return;

        SoundData asset = CreateInstance<SoundData>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        _soundData = asset;
        Debug.Log($"[AudioSetup] Đã tạo SoundData tại: {path}");
    }

    private void DrawNoDataWarning()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.HelpBox(
            "Chưa có SoundData asset.\n\n" +
            "• Kéo asset có sẵn vào ô SoundData ở trên, hoặc\n" +
            "• Bấm 'New' để tạo asset mới.",
            MessageType.Warning);

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
        if (GUILayout.Button("Tạo SoundData mới", GUILayout.Height(36)))
            CreateNewSoundData();
        GUI.backgroundColor = Color.white;
    }
}
#endif
