#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector cho WaveData — hiển thị bảng EnemyGroup dạng table,
/// với nút Add/Remove group và preview tổng số quái.
/// </summary>
[CustomEditor(typeof(WaveData))]
public class WaveDataEditor : Editor
{
    private WaveData _wave;
    private bool _showGroups = true;

    // Màu sắc cho từng hàng (zebra striping)
    private readonly Color _rowEven = new Color(0.22f, 0.22f, 0.22f);
    private readonly Color _rowOdd  = new Color(0.26f, 0.26f, 0.26f);
    private readonly Color _headerColor = new Color(0.15f, 0.15f, 0.15f);
    private readonly Color _addBtnColor = new Color(0.2f, 0.6f, 0.3f);
    private readonly Color _removeBtnColor = new Color(0.7f, 0.2f, 0.2f);

    private void OnEnable()
    {
        _wave = (WaveData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Header tổng quan ──
        EditorGUILayout.Space(4);
        DrawSummaryBox();
        EditorGUILayout.Space(6);

        // ── Delay settings ──
        SerializedProperty delayProp = serializedObject.FindProperty("delayBetweenGroups");
        EditorGUILayout.PropertyField(delayProp, new GUIContent("Delay Between Groups (s)"));
        EditorGUILayout.Space(6);

        // ── Bảng Enemy Groups ──
        _showGroups = EditorGUILayout.Foldout(_showGroups, "Enemy Groups", true, EditorStyles.foldoutHeader);
        if (_showGroups)
        {
            DrawGroupTable();
        }

        EditorGUILayout.Space(6);

        // ── Nút Add Group ──
        GUI.backgroundColor = _addBtnColor;
        if (GUILayout.Button("＋  Add Enemy Group", GUILayout.Height(28)))
        {
            Undo.RecordObject(_wave, "Add Enemy Group");
            ArrayUtility.Add(ref _wave.enemyGroups, new EnemyGroup());
            EditorUtility.SetDirty(_wave);
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSummaryBox()
    {
        int totalEnemies = _wave != null ? _wave.TotalEnemies : 0;
        int groupCount   = (_wave?.enemyGroups != null) ? _wave.enemyGroups.Length : 0;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        GUIStyle bold = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };
        EditorGUILayout.LabelField($"👥  {groupCount} group(s)", bold, GUILayout.Width(110));
        EditorGUILayout.LabelField($"⚔️  {totalEnemies} total enemies", bold);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawGroupTable()
    {
        if (_wave.enemyGroups == null)
            _wave.enemyGroups = new EnemyGroup[0];

        // ── Header row ──
        EditorGUILayout.BeginHorizontal();
        DrawHeader("#",           36);
        DrawHeader("Enemy Type",  130);
        DrawHeader("Count",        60);
        DrawHeader("Interval (s)", 90);
        DrawHeader("",             30); // remove button column
        EditorGUILayout.EndHorizontal();

        // ── Data rows ──
        int removeIndex = -1;
        EnemyGroup[] groups = _wave.enemyGroups;

        for (int i = 0; i < groups.Length; i++)
        {
            Color rowBg = (i % 2 == 0) ? _rowEven : _rowOdd;
            GUI.backgroundColor = rowBg;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            // # index
            EditorGUILayout.LabelField($" {i + 1}", GUILayout.Width(36));

            // Enemy Type
            EnemyGroup g = groups[i];
            EditorGUI.BeginChangeCheck();
            g.enemyType     = (EnemyType)EditorGUILayout.EnumPopup(g.enemyType,     GUILayout.Width(130));
            g.count         = EditorGUILayout.IntField(g.count,                       GUILayout.Width(60));
            g.spawnInterval = EditorGUILayout.FloatField(g.spawnInterval,             GUILayout.Width(90));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wave, "Edit Enemy Group");
                groups[i] = g;
                EditorUtility.SetDirty(_wave);
            }

            // Remove button
            GUI.backgroundColor = _removeBtnColor;
            if (GUILayout.Button("✕", GUILayout.Width(26), GUILayout.Height(18)))
                removeIndex = i;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        // Xóa group ngoài vòng lặp để tránh index shift
        if (removeIndex >= 0)
        {
            Undo.RecordObject(_wave, "Remove Enemy Group");
            ArrayUtility.RemoveAt(ref _wave.enemyGroups, removeIndex);
            EditorUtility.SetDirty(_wave);
        }

        if (groups.Length == 0)
        {
            EditorGUILayout.HelpBox("No groups yet. Click 'Add Enemy Group' to start.", MessageType.Info);
        }
    }

    private void DrawHeader(string label, float width)
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.gray }
        };

        GUI.backgroundColor = _headerColor;
        EditorGUILayout.LabelField(label, style, GUILayout.Width(width));
        GUI.backgroundColor = Color.white;
    }
}
#endif
