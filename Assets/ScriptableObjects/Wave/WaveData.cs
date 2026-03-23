using UnityEngine;

/// <summary>
/// Một nhóm quái trong wave: loại quái, số lượng, khoảng cách spawn.
/// </summary>
[System.Serializable]
public class EnemyGroup
{
    public EnemyType enemyType;
    [Min(1)] public int count = 5;
    [Min(0.1f)] public float spawnInterval = 1f;
}

/// <summary>
/// Dữ liệu một wave: gồm nhiều nhóm quái khác nhau.
/// Các nhóm spawn tuần tự, cách nhau delayBetweenGroups giây.
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    public EnemyGroup[] enemyGroups = new EnemyGroup[1];

    [Tooltip("Thời gian chờ (giây) giữa các group quái trong cùng một wave")]
    [Min(0f)] public float delayBetweenGroups = 3f;

    /// <summary>Tổng số quái của toàn wave (tính từ tất cả groups)</summary>
    public int TotalEnemies
    {
        get
        {
            int total = 0;
            if (enemyGroups == null) return 0;
            foreach (var g in enemyGroups) total += g.count;
            return total;
        }
    }
}
