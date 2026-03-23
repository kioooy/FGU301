using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnCoinRewardChanged;

    private int _lives = 20;
    private int _coins = 150;
    public int Coins => _coins;

    private float _gameSpeed = 1f;
    public float GameSpeed => _gameSpeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(_lives);
        OnCoinRewardChanged?.Invoke(_coins);

        // TỰ ĐỘNG LƯU KHI VÀO GAME
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveToSlot(0);
        }
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _lives = Mathf.Max(0, _lives - data.damage);
        OnLivesChanged?.Invoke(_lives);
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AddCoins(Mathf.RoundToInt(enemy.Data.coinReward));
    }

    public void AddCoins(int amount)
    {
        _coins += amount;
        OnCoinRewardChanged?.Invoke(_coins);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public void SetGameSpeed(float newSpeed)
    {
        _gameSpeed = newSpeed;
        SetTimeScale(_gameSpeed);
    }

    public void SpendCoins(int amount)
    {
        if (_coins >= amount)
        {
            _coins -= amount;
            OnCoinRewardChanged?.Invoke(_coins);
        }
    }

    public void ResetLevelProgression()
    {
        for (int i = 2; i <= 15; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
        }
        PlayerPrefs.Save();
    }

    // THÊM PHƯƠNG THỨC ĐỂ UNLOCK LEVEL
    public void UnlockNextLevel(int currentLevel)
    {
        SaveLoadManager.Instance.UnlockNextLevel(currentLevel);
    }
}