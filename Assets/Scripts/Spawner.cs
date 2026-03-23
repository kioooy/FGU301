using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChanged;

    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    // --- Spawn state ---
    private int _currentGroupIndex = 0;
    private int _spawnedInGroup = 0;
    private float _spawnTimer = 0f;
    private float _groupDelayTimer = 0f;
    private bool _waitingBetweenGroups = false;

    // Tổng số quái đã spawn / đã bị loại bỏ trong wave hiện tại
    private int _totalSpawnedThisWave = 0;
    private int _enemiesRemoved = 0;

    // --- Timing ---
    public float _initialDelay = 25f;
    private float _initialTimer;
    private bool _isInitialDelay = true;

    public float _timeBetweenWaves = 15f;
    public float _waveCooldown;
    public bool _isBetweenWaves = false;

    private bool _isCountdownActive = false;
    [SerializeField] private TMP_Text countdownText;

    // --- Pools ---
    [SerializeField] private ObjectPooler SnakePool;
    [SerializeField] private ObjectPooler SpiderPool;
    [SerializeField] private ObjectPooler BearPool;
    [SerializeField] private ObjectPooler ShamanPool;
    [SerializeField] private ObjectPooler ThiefPool;
    [SerializeField] private ObjectPooler HarpoonFishPool;
    [SerializeField] private ObjectPooler LancerPool;
    [SerializeField] private ObjectPooler HarpoonFishBossPool;
    [SerializeField] private ObjectPooler ShamanBossPool;
    [SerializeField] private ObjectPooler BearBossPool;
    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    // --- Public properties ---
    public int CurrentWaveIndex => _currentWaveIndex;
    public int TotalWaves => waves.Length;
    public int SpawnedEnemies => _totalSpawnedThisWave;
    public int DestroyedEnemies => _enemiesRemoved;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            { EnemyType.Snake,           SnakePool          },
            { EnemyType.Bear,            BearPool           },
            { EnemyType.Spider,          SpiderPool         },
            { EnemyType.Shaman,          ShamanPool         },
            { EnemyType.Thief,           ThiefPool          },
            { EnemyType.HarpoonFish,     HarpoonFishPool    },
            { EnemyType.Lancer,          LancerPool         },
            { EnemyType.HarpoonFishBoss, HarpoonFishBossPool},
            { EnemyType.ShamanBoss,      ShamanBossPool     },
            { EnemyType.BearBoss,        BearBossPool       },
        };
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed  += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed  -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        _initialTimer = _initialDelay;
        _isInitialDelay = true;
        OnWaveChanged?.Invoke(_currentWaveIndex);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateCountdownDisplay();

        // --- Initial delay ---
        if (_isInitialDelay)
        {
            _initialTimer -= Time.deltaTime;
            if (_initialTimer <= 0f)
            {
                _isInitialDelay = false;
                StartWave();
            }
            return;
        }

        // --- Cooldown giữa các wave ---
        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0f)
            {
                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
                _waveCounter++;
                OnWaveChanged?.Invoke(_currentWaveIndex);
                _isBetweenWaves = false;
                StartWave();
            }
            return;
        }

        // --- Spawning ---
        SpawnUpdate();
    }

    /// <summary>Khởi tạo trạng thái đầu wave mới.</summary>
    private void StartWave()
    {
        _currentGroupIndex = 0;
        _spawnedInGroup = 0;
        _totalSpawnedThisWave = 0;
        _enemiesRemoved = 0;
        _spawnTimer = 0f;
        _groupDelayTimer = 0f;
        _waitingBetweenGroups = false;

        AudioManager.Instance?.PlayWaveStart();
    }

    /// <summary>Xử lý logic spawn từng group quái theo thứ tự.</summary>
    private void SpawnUpdate()
    {
        if (CurrentWave.enemyGroups == null || CurrentWave.enemyGroups.Length == 0)
        {
            EndWave();
            return;
        }

        // Đã hết tất cả groups
        if (_currentGroupIndex >= CurrentWave.enemyGroups.Length)
        {
            // Chờ cho đến khi tất cả quái bị loại bỏ rồi mới kết thúc wave
            if (_enemiesRemoved >= CurrentWave.TotalEnemies)
                EndWave();
            return;
        }

        EnemyGroup group = CurrentWave.enemyGroups[_currentGroupIndex];

        // --- Đang chờ delay giữa các group ---
        if (_waitingBetweenGroups)
        {
            _groupDelayTimer -= Time.deltaTime;
            if (_groupDelayTimer <= 0f)
                _waitingBetweenGroups = false;
            return;
        }

        // --- Spawn từng con trong group ---
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f && _spawnedInGroup < group.count)
        {
            _spawnTimer = group.spawnInterval;
            SpawnEnemy(group.enemyType);
            _spawnedInGroup++;
        }

        // Group này đã spawn đủ → sang group tiếp theo
        if (_spawnedInGroup >= group.count)
        {
            _currentGroupIndex++;
            _spawnedInGroup = 0;
            _spawnTimer = 0f;

            if (_currentGroupIndex < CurrentWave.enemyGroups.Length)
            {
                _waitingBetweenGroups = true;
                _groupDelayTimer = CurrentWave.delayBetweenGroups;
            }
        }
    }

    private void EndWave()
    {
        _isBetweenWaves = true;
        _waveCooldown = _timeBetweenWaves;
    }

    private void SpawnEnemy(EnemyType type)
    {
        if (!_poolDictionary.TryGetValue(type, out var pool)) return;

        GameObject obj = pool.GetPooledObject();
        if (obj == null) return;

        // Đặt z = 0 rõ ràng để tránh Z-fighting gây chớp nháy
        obj.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        float healthMultiplier = 1f + (_waveCounter * 0.09f);
        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.Initialize(healthMultiplier);
        obj.SetActive(true);

        _totalSpawnedThisWave++;
    }

    private void HandleEnemyReachedEnd(EnemyData data) => _enemiesRemoved++;
    private void HandleEnemyDestroyed(Enemy enemy) => _enemiesRemoved++;

    // --- Public API cho UIController ---
    public bool IsInitialDelayActive() => _isInitialDelay;
    public float GetRemainingInitialDelay() => Mathf.Max(0f, _initialTimer);
    public float GetRemainingWaveCooldown() => Mathf.Max(0f, _waveCooldown);
    public bool IsCountdownActive() => _isCountdownActive;

    private void UpdateCountdownDisplay()
    {
        if (countdownText == null) return;

        if (_isInitialDelay)
        {
            _isCountdownActive = true;
            countdownText.text = $"Wave begins at: {Mathf.CeilToInt(_initialTimer)}s";
            countdownText.gameObject.SetActive(true);
        }
        else if (_isBetweenWaves)
        {
            _isCountdownActive = true;
            countdownText.text = $"Next wave at: {Mathf.CeilToInt(_waveCooldown)}s";
            countdownText.gameObject.SetActive(true);
        }
        else
        {
            if (_isCountdownActive)
            {
                _isCountdownActive = false;
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}
