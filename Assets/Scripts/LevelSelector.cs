using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public string level;

    private UnityEngine.UI.Button button;
    private TMP_Text levelText;
    private GameObject lockIcon;

    void Start()
    {
        // Tìm button component
        button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            button = GetComponentInChildren<UnityEngine.UI.Button>();
        }

        // Tìm level text
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.text == level.ToString() || text.name.Contains("Level") || text.name.Contains("Text"))
            {
                levelText = text;
                break;
            }
        }

        // Tìm lock icon
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Lock") || child.name.Contains("lock"))
            {
                lockIcon = child.gameObject;
                break;
            }
        }

        UpdateLevelState();
    }

    public void OpenScene()
    {
        // Kiểm tra null và valid level
        if (string.IsNullOrEmpty(level))
        {
            Debug.LogError("Level number is not set on " + gameObject.name + "!");
            return;
        }

        // Kiểm tra xem level có phải là số hợp lệ không
        if (!int.TryParse(level, out int levelNumber))
        {
            Debug.LogError("Invalid level number: " + level + " on " + gameObject.name);
            return;
        }

        if (IsLevelUnlocked(levelNumber))
        {
            // Kiểm tra và lưu game trước khi vào level
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveToSlot(0);
            }
            else
            {
                Debug.LogWarning("SaveLoadManager.Instance is null!");
            }

            // Load scene
            string sceneName = "Level " + level;
            Debug.Log("Loading scene: " + sceneName);

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("Scene not found: " + sceneName);
            }
        }
        else
        {
            Debug.Log("Level " + level + " is locked!");
        }
    }

    private void UpdateLevelState()
    {
        // Kiểm tra level có được set không
        if (string.IsNullOrEmpty(level))
        {
            Debug.LogError("Level number is not set on " + gameObject.name + "!");
            return;
        }

        // Parse level number
        if (!int.TryParse(level, out int levelNumber))
        {
            Debug.LogError("Invalid level number: " + level + " on " + gameObject.name);
            return;
        }

        bool isUnlocked = IsLevelUnlocked(levelNumber);

        // Cập nhật button interactable
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
        else
        {
            Debug.LogWarning("Button component not found on " + gameObject.name);
        }

        // Cập nhật lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // LUÔN hiển thị số level (khắc phục mất số 5)
        if (levelText != null)
        {
            levelText.text = level.ToString();
            levelText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("LevelText not found on " + gameObject.name + ". Creating one...");

            // Tạo Text mới nếu không tìm thấy
            GameObject textObj = new GameObject("LevelText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localScale = Vector3.one;

            TMP_Text newText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            newText.text = level.ToString();
            newText.alignment = TMPro.TextAlignmentOptions.Center;
            newText.fontSize = 24;
            newText.color = Color.white;
            levelText = newText;
        }
    }

    private bool IsLevelUnlocked(int levelNumber)
    {
        // Level 1 luôn mở
        if (levelNumber == 1) return true;

        // Kiểm tra PlayerPrefs
        string key = "LevelUnlocked_" + levelNumber;
        int unlocked = PlayerPrefs.GetInt(key, 0);

        // Debug để kiểm tra
        Debug.Log("Checking level " + levelNumber + ": key=" + key + ", value=" + unlocked);

        return unlocked == 1;
    }

    // Phương thức để gỡ lỗi - kiểm tra PlayerPrefs
    [ContextMenu("Check Unlock Status")]
    private void CheckUnlockStatus()
    {
        if (string.IsNullOrEmpty(level)) return;

        if (int.TryParse(level, out int levelNumber))
        {
            string key = "LevelUnlocked_" + levelNumber;
            int value = PlayerPrefs.GetInt(key, -1);
            Debug.Log("Level " + levelNumber + " (" + gameObject.name + "): " +
                     "Key=" + key + ", Value=" + value + ", IsUnlocked=" + (value == 1));
        }
    }

    [ContextMenu("Force Unlock This Level")]
    private void ForceUnlockThisLevel()
    {
        if (string.IsNullOrEmpty(level)) return;

        if (int.TryParse(level, out int levelNumber))
        {
            PlayerPrefs.SetInt("LevelUnlocked_" + levelNumber, 1);
            PlayerPrefs.Save();
            Debug.Log("Force unlocked Level " + levelNumber);
            UpdateLevelState();
        }
    }

    [ContextMenu("Lock This Level")]
    private void LockThisLevel()
    {
        if (string.IsNullOrEmpty(level)) return;

        if (int.TryParse(level, out int levelNumber))
        {
            PlayerPrefs.SetInt("LevelUnlocked_" + levelNumber, 0);
            PlayerPrefs.Save();
            Debug.Log("Locked Level " + levelNumber);
            UpdateLevelState();
        }
    }
}