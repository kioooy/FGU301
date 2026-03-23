using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private SaveLoadData currentGameData;
    private const int TOTAL_SLOTS = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasAnySave()
    {
        return GetSlotData(0) != null;
    }

    public void CreateNewGame(int slotIndex = 0)
    {
        ResetAllLevels();

        currentGameData = new SaveLoadData();
        currentGameData.SetDefaultNewGame();

        SaveToSlot(slotIndex);

        SceneManager.LoadScene("LevelSelect");
    }

    public void ContinueToLevelSelect()
    {
        SaveLoadData savedGame = GetSlotData(0);

        if (savedGame != null)
        {
            ApplyUnlockedLevels(savedGame);
            currentGameData = savedGame;
            SceneManager.LoadScene("LevelSelect");
        }
        else
        {
            CreateNewGame(0);
        }
    }

    public void LoadGame(SaveLoadData data)
    {
        if (data == null) return;

        currentGameData = data;
        ApplyUnlockedLevels(data);

        if (data.currentLevel >= 1)
        {
            SceneManager.LoadScene($"Level {data.currentLevel}");
        }
        else
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }

    private void ResetAllLevels()
    {
        for (int i = 1; i <= 15; i++)
        {
            if (i == 1)
                PlayerPrefs.SetInt($"LevelUnlocked_{i}", 1);
            else
                PlayerPrefs.SetInt($"LevelUnlocked_{i}", 0);
        }
        PlayerPrefs.Save();
    }

    private void ApplyUnlockedLevels(SaveLoadData data)
    {
        if (data == null || data.unlockedLevels == null) return;

        for (int i = 1; i <= 15; i++)
        {
            PlayerPrefs.SetInt($"LevelUnlocked_{i}", 0);
        }

        foreach (int level in data.unlockedLevels)
        {
            if (level >= 1 && level <= 15)
            {
                PlayerPrefs.SetInt($"LevelUnlocked_{level}", 1);
            }
        }
        PlayerPrefs.Save();
    }

    public void SaveToSlot(int slotIndex)
    {
        if (currentGameData == null)
        {
            currentGameData = new SaveLoadData();
            currentGameData.SetDefaultNewGame();
        }

        currentGameData.saveSlotIndex = slotIndex;
        currentGameData.saveTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level "))
        {
            string levelStr = sceneName.Replace("Level ", "");
            if (int.TryParse(levelStr, out int level))
            {
                currentGameData.currentLevel = level;
            }
        }

        string json = JsonUtility.ToJson(currentGameData, true);
        PlayerPrefs.SetString($"SaveSlot_{slotIndex}", json);
        PlayerPrefs.Save();
    }

    public SaveLoadData GetSlotData(int slotIndex)
    {
        string key = $"SaveSlot_{slotIndex}";
        string json = PlayerPrefs.GetString(key, "");

        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<SaveLoadData>(json);
        }

        return null;
    }

    public void UnlockNextLevel(int currentLevel)
    {
        if (currentGameData == null)
        {
            currentGameData = new SaveLoadData();
            currentGameData.SetDefaultNewGame();
        }

        int nextLevel = currentLevel + 1;
        if (nextLevel <= 15)
        {
            if (!currentGameData.unlockedLevels.Contains(nextLevel))
            {
                currentGameData.unlockedLevels.Add(nextLevel);
                PlayerPrefs.SetInt($"LevelUnlocked_{nextLevel}", 1);
                PlayerPrefs.Save();

                SaveToSlot(0);
            }
        }
    }
}