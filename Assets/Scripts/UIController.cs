using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text coinRewardText;
    [SerializeField] private TMP_Text notEnoughCoinsText;
    [SerializeField] private GameObject towerPanel;
    [SerializeField] private TowerCard towerCardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();
    private Platform _currentPlatform;
    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.blue;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;
    [SerializeField] private GameObject pausePanel;
    private bool _isGamePaused = false;
    [SerializeField] private GameObject gameoverPanel;
    private Spawner _spawner;
    [SerializeField] private TMP_Text menuCountdownText;
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private Button completedPlayAgainButton;
    [SerializeField] private Button completedBackToMapButton;
    private bool _levelCompleted = false;

    [Header("Tower Action Menu")]
    [SerializeField] private GameObject towerActionMenu;
    [SerializeField] private TMP_Text towerInfoText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text upgradePriceText;
    [SerializeField] private TMP_Text sellPriceText;

    private Tower _selectedTower;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnCoinRewardChanged += UpdateCoinRewardText;
        Platform.OnPlatformClicked += handlePlatformClicked;
        TowerCard.OnTowerSelected += handleTowerSelected;
        Tower.OnTowerClicked += handleTowerClicked;
        Enemy.OnEnemyDestroyed += OnEnemyDestroyed;
        
        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelectedTower);
        if (sellButton != null) sellButton.onClick.AddListener(SellSelectedTower);
        if (closeButton != null) closeButton.onClick.AddListener(HideTowerActionMenu);

        // Tự động tìm tất cả các nút đóng trong bảng (bao gồm cả nút nền)
        if (towerActionMenu != null)
        {
            Button[] allButtons = towerActionMenu.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.name.Contains("Close") && btn != closeButton)
                {
                    btn.onClick.AddListener(HideTowerActionMenu);
                }
            }
        }
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnCoinRewardChanged -= UpdateCoinRewardText;
        Platform.OnPlatformClicked -= handlePlatformClicked;
        TowerCard.OnTowerSelected -= handleTowerSelected;
        Tower.OnTowerClicked -= handleTowerClicked;
        Enemy.OnEnemyDestroyed -= OnEnemyDestroyed;

        if (upgradeButton != null) upgradeButton.onClick.RemoveListener(UpgradeSelectedTower);
        if (sellButton != null) sellButton.onClick.RemoveListener(SellSelectedTower);
        if (closeButton != null) closeButton.onClick.RemoveListener(HideTowerActionMenu);
        
        // Also remove from any other buttons in the menu that might be using it
        if (towerActionMenu != null)
        {
            Button[] allButtons = towerActionMenu.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.name.Contains("Close")) btn.onClick.RemoveListener(HideTowerActionMenu);
            }
        }

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    private void Start()
    {
        speed1Button.onClick.AddListener(() => SetGameSpeed(0.2f));
        speed2Button.onClick.AddListener(() => SetGameSpeed(1f));
        speed3Button.onClick.AddListener(() => SetGameSpeed(2f));
        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);

        _spawner = FindFirstObjectByType<Spawner>();

        if (menuCountdownText != null)
        {
            menuCountdownText.gameObject.SetActive(false);
        }

        if (completedPanel != null)
        {
            completedPanel.SetActive(false);
        }

        if (completedPlayAgainButton != null)
        {
            completedPlayAgainButton.onClick.AddListener(RestartLevel);
        }

        if (completedBackToMapButton != null)
        {
            completedBackToMapButton.onClick.AddListener(BackToMap);
        }

        if (towerActionMenu != null) towerActionMenu.SetActive(false);
    }

    private void Update()
    {
        UpdateMenuCountdownText();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Kiểm tra hoàn thành level
        if (!_levelCompleted && _spawner != null)
        {
            CheckLevelCompletion();
        }

        // Phát hiện click vào trụ bằng Physics2D.OverlapPoint
        if (Input.GetMouseButtonDown(0))
        {
            // Nếu đang click vào UI (bảng mua trụ, button...) thì bỏ qua hoàn toàn
            bool overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (overUI) return;

            // Chuyển tọa độ màn hình sang thế giới
            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = -Camera.main.transform.position.z;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            Vector2 clickPos = new Vector2(mouseWorld.x, mouseWorld.y);

            Collider2D[] hits = Physics2D.OverlapPointAll(clickPos);

            // ƯU TIÊN platform: nếu click trúng platform (ô đặt trụ trống)
            // thì để Platform.cs tự xử lý, không can thiệp tower detection
            foreach (Collider2D hit in hits)
            {
                if (hit.GetComponent<Platform>() != null)
                    return;
            }

            // Trong các collider trúng, chọn tower có tâm gần clickPos nhất
            Tower clickedTower = null;
            float minDist = float.MaxValue;
            foreach (Collider2D hit in hits)
            {
                Tower t = hit.GetComponent<Tower>();
                if (t == null) t = hit.GetComponentInParent<Tower>();
                if (t != null)
                {
                    float dist = Vector2.Distance(clickPos, (Vector2)t.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        clickedTower = t;
                    }
                }
            }

            if (clickedTower != null)
            {
                HideTowerPanel();
                if (_selectedTower != null && _selectedTower != clickedTower)
                    _selectedTower.Deselect();
                _selectedTower = clickedTower;
                _selectedTower.Select();
                ShowTowerActionMenu();
            }
            else
            {
                // Click vào vùng trống ngoài UI → đóng menu nếu đang mở
                if (towerActionMenu != null && towerActionMenu.activeSelf)
                {
                    HideTowerActionMenu();
                }
            }
        }
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }

    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $" {currentLives}";
        if (currentLives == 0)
        {
            ShowGameOver();
        }
    }

    private void UpdateCoinRewardText(int currentCoinRewards)
    {
        coinRewardText.text = $"{currentCoinRewards}";
    }

    private void handlePlatformClicked(Platform platform)
    {
        DeselectTower();
        _currentPlatform = platform;
        ShowTowerPanel();
    }

    private void handleTowerClicked(Tower tower)
    {
        HideTowerPanel();
        if (_selectedTower != null && _selectedTower != tower)
        {
            _selectedTower.Deselect();
        }

        _selectedTower = tower;
        _selectedTower.Select();
        ShowTowerActionMenu();
    }

    private void ShowTowerActionMenu()
    {
        if (towerActionMenu == null) return;

        towerActionMenu.SetActive(true);
        UpdateTowerActionMenuUI();
    }

    private void UpdateTowerActionMenuUI()
    {
        if (_selectedTower == null) return;

        towerInfoText.text = $"{_selectedTower.Data.name}\nLevel: {_selectedTower.Level}/{_selectedTower.MaxLevel}\nDamage: {_selectedTower.CurrentDamage:0.0}";
        upgradePriceText.text = _selectedTower.Level >= _selectedTower.MaxLevel ? "MAX" : $"{_selectedTower.UpgradeCost} coins";
        sellPriceText.text = $"{_selectedTower.SellValue} coins";
        
        upgradeButton.interactable = _selectedTower.Level < _selectedTower.MaxLevel && GameManager.Instance.Coins >= _selectedTower.UpgradeCost;
    }

    public void HideTowerActionMenu()
    {
        if (towerActionMenu != null) towerActionMenu.SetActive(false);
        DeselectTower();
    }

    private void DeselectTower()
    {
        if (_selectedTower != null)
        {
            _selectedTower.Deselect();
            _selectedTower = null;
        }
    }

    public void UpgradeSelectedTower()
    {
        Debug.Log("Upgrade Clicked");
        if (_selectedTower == null) return;

        if (_selectedTower.Level >= _selectedTower.MaxLevel)
        {
            Debug.Log("Tower already at Max Level");
            return;
        }

        if (GameManager.Instance.Coins >= _selectedTower.UpgradeCost)
        {
            GameManager.Instance.SpendCoins(_selectedTower.UpgradeCost);
            _selectedTower.Upgrade();
            UpdateTowerActionMenuUI();
            AudioManager.Instance?.PlayTowerUpgraded();
        }
        else
        {
            Debug.Log("Not enough coins for upgrade");
            StartCoroutine(ShowNotEnoughCoinsText());
            AudioManager.Instance?.PlayNotEnoughCoins();
        }
    }

    public void SellSelectedTower()
    {
        Debug.Log("Sell Clicked");
        if (_selectedTower != null)
        {
            GameManager.Instance.AddCoins(_selectedTower.SellValue);
            _selectedTower.Sell();
            HideTowerActionMenu();
            AudioManager.Instance?.PlayTowerSold();
        }
    }

    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        Platform.towerPanelOpen = true;
        PopulateTowerCards();
    }

    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
    }

    private void PopulateTowerCards()
    {
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        foreach (var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, cardsContainer).gameObject;
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initialize(data);
            activeCards.Add(cardGameObject);
        }
    }

    private void handleTowerSelected(TowerData towerData)
    {
        if (GameManager.Instance.Coins >= towerData.cost)
        {
            GameManager.Instance.SpendCoins(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
            HideTowerPanel();
            AudioManager.Instance?.PlayTowerPlaced();
        }
        else
        {
            StartCoroutine(ShowNotEnoughCoinsText());
            AudioManager.Instance?.PlayNotEnoughCoins();
        }
    }

    private IEnumerator ShowNotEnoughCoinsText()
    {
        notEnoughCoinsText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        notEnoughCoinsText.gameObject.SetActive(false);
    }

    private void SetGameSpeed(float timeScale)
    {
        HighlightSelectedSpeedButton(timeScale);
        GameManager.Instance.SetGameSpeed(timeScale);
    }

    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        button.image.color = isSelected ? selectedButtonColor : normalButtonColor;
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }

    private void HighlightSelectedSpeedButton(float selectedSpeed)
    {
        UpdateButtonVisual(speed1Button, selectedSpeed == 0.2f);
        UpdateButtonVisual(speed2Button, selectedSpeed == 1f);
        UpdateButtonVisual(speed3Button, selectedSpeed == 2f);
    }

    public void TogglePause()
    {
        if (_isGamePaused)
        {
            pausePanel.SetActive(false);
            _isGamePaused = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        }
        else
        {
            pausePanel.SetActive(true);
            _isGamePaused = true;
            GameManager.Instance.SetTimeScale(0f);
        }
    }

    public void RestartLevel()
    {
        GameManager.Instance.SetTimeScale(1f);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToMap()
    {
        GameManager.Instance.SetTimeScale(1f);
        SceneManager.LoadScene("LevelSelect");
    }

    public void MainMenu()
    {
        GameManager.Instance.SetTimeScale(1f);
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowGameOver()
    {
        GameManager.Instance.SetTimeScale(0.05f);
        gameoverPanel.SetActive(true);
        AudioManager.Instance?.PlayGameOver();
    }

    private void UpdateMenuCountdownText()
    {
        if (_spawner == null || menuCountdownText == null) return;

        if (_spawner.IsInitialDelayActive())
        {
            float remainingTime = _spawner.GetRemainingInitialDelay();
            int seconds = Mathf.CeilToInt(remainingTime);
            menuCountdownText.text = $"Wave begins at: {seconds}";
            menuCountdownText.gameObject.SetActive(true);
        }
        else if (_spawner._isBetweenWaves)
        {
            float remainingTime = _spawner.GetRemainingWaveCooldown();
            int seconds = Mathf.CeilToInt(remainingTime);
            menuCountdownText.text = $"Next wave at: {seconds}";
            menuCountdownText.gameObject.SetActive(true);
        }
        else
        {
            menuCountdownText.gameObject.SetActive(false);
        }
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (_levelCompleted || _spawner == null) return;

        // Kiểm tra xem có phải wave cuối không
        if (_spawner.CurrentWaveIndex == _spawner.TotalWaves - 1)
        {
            // Kiểm tra xem đã spawn đủ enemy và tiêu diệt đủ chưa
            if (_spawner.SpawnedEnemies >= 1 && _spawner.DestroyedEnemies >= 1)
            {
                // Kiểm tra xem còn enemy nào active không
                Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
                bool allEnemiesDefeated = true;

                foreach (Enemy enemy in enemies)
                {
                    if (enemy.gameObject.activeInHierarchy)
                    {
                        allEnemiesDefeated = false;
                        break;
                    }
                }

                if (allEnemiesDefeated)
                {
                    ShowLevelCompleted();
                }
            }
        }
    }

    public void ShowLevelCompleted()
    {
        if (completedPanel != null && !_levelCompleted)
        {
            _levelCompleted = true;

            // Tạm dừng game
            GameManager.Instance.SetTimeScale(0f);

            // Hiển thị panel
            completedPanel.SetActive(true);

            // Phát âm thanh thắng
            AudioManager.Instance?.PlayVictory();

            // Mở khóa level tiếp theo
            UnlockNextLevel();
        }
    }

    private void UnlockNextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName.StartsWith("Level "))
        {
            string levelNumberStr = currentSceneName.Replace("Level ", "");
            if (int.TryParse(levelNumberStr, out int currentLevel))
            {
                int nextLevel = currentLevel + 1;
                if (nextLevel <= 15)
                {
                    PlayerPrefs.SetInt("LevelUnlocked_" + nextLevel, 1);
                    PlayerPrefs.Save();
                }
            }
        }
    }

    public void NextLevel()
    {
        GameManager.Instance.SetTimeScale(1f);
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName.StartsWith("Level "))
        {
            string levelNumberStr = currentSceneName.Replace("Level ", "");
            if (int.TryParse(levelNumberStr, out int currentLevel))
            {
                int nextLevel = currentLevel + 1;
                if (nextLevel <= 15)
                {
                    SceneManager.LoadScene("Level " + nextLevel);
                    return;
                }
            }
        }
        BackToMap();
    }
}
