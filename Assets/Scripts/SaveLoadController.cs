using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingMenu;
    [SerializeField] private SaveLoadSlot[] saveSlots;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text statusText;

    private SaveLoadSlot selectedSlot;
    private bool isNewGameMode = false;

    private void Start()
    {
        InitializeSlots();
        SetupButtons();
        HideLoadingMenu();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            if (saveSlots[i] != null)
            {
                saveSlots[i].Initialize(i);
                saveSlots[i].OnSlotSelected += OnSlotSelected;
            }
        }
    }

    private void SetupButtons()
    {
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnSlotSelected(SaveLoadSlot slot)
    {
        // Bỏ chọn slot cũ
        if (selectedSlot != null && selectedSlot != slot)
        {
            selectedSlot.DeselectSlot();
        }

        // Chọn slot mới
        selectedSlot = slot;

        // Cập nhật UI
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (selectedSlot == null)
        {
            if (loadButton != null)
                loadButton.interactable = false;
            if (statusText != null)
                statusText.text = "Select a slot";
            return;
        }

        if (isNewGameMode)
        {
            // Chế độ New Game: luôn cho phép Save
            if (loadButton != null)
            {
                loadButton.interactable = true;
                loadButton.GetComponentInChildren<TMP_Text>().text = "SAVE NEW GAME";
            }
            if (statusText != null)
                statusText.text = $"New game will be saved to Slot {selectedSlot.SlotIndex + 1}";
        }
        else
        {
            // Chế độ Load: chỉ cho phép nếu slot có data
            if (loadButton != null)
            {
                loadButton.interactable = !selectedSlot.IsEmpty;
                loadButton.GetComponentInChildren<TMP_Text>().text = "LOAD";
            }
            if (statusText != null)
            {
                if (selectedSlot.IsEmpty)
                    statusText.text = "Empty slot";
                else
                    statusText.text = $"Slot {selectedSlot.SlotIndex + 1} selected";
            }
        }
    }

    private void OnLoadClicked()
    {
        if (selectedSlot == null)
        {
            if (statusText != null)
                statusText.text = "Please select a slot first!";
            return;
        }

        if (isNewGameMode)
        {
            // Tạo game mới và lưu vào slot đã chọn
            SaveLoadManager.Instance.CreateNewGame(selectedSlot.SlotIndex);
            HideLoadingMenu();
        }
        else
        {
            // Load game từ slot
            if (!selectedSlot.IsEmpty)
            {
                selectedSlot.LoadData();
                HideLoadingMenu();
            }
            else
            {
                if (statusText != null)
                    statusText.text = "This slot is empty!";
            }
        }
    }

    private void OnBackClicked()
    {
        HideLoadingMenu();
    }

    public void ShowLoadingMenuForLoad()
    {
        isNewGameMode = false;
        ShowLoadingMenu();
    }

    public void ShowLoadingMenuForNewGame()
    {
        isNewGameMode = true;
        ShowLoadingMenu();
    }

    private void ShowLoadingMenu()
    {
        if (loadingMenu != null)
            loadingMenu.SetActive(true);

        // Reset selection
        if (selectedSlot != null)
        {
            selectedSlot.DeselectSlot();
            selectedSlot = null;
        }

        // Refresh slots data
        foreach (var slot in saveSlots)
        {
            if (slot != null)
                slot.LoadSlotData();
        }

        // Update UI
        UpdateUI();
    }

    private void HideLoadingMenu()
    {
        if (loadingMenu != null)
            loadingMenu.SetActive(false);

        // Reset
        isNewGameMode = false;
        if (selectedSlot != null)
        {
            selectedSlot.DeselectSlot();
            selectedSlot = null;
        }
    }
}