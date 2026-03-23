using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button slotButton;
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private TMP_Text dateTimeText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject emptySlotText;
    [SerializeField] private GameObject dataPanel;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.blue;

    private int slotIndex;
    private SaveLoadData slotData;
    private bool isEmpty = true;

    public int SlotIndex => slotIndex;
    public bool IsEmpty => isEmpty;
    public SaveLoadData SlotData => slotData;

    public event System.Action<SaveLoadSlot> OnSlotSelected;

    private void Start()
    {
        slotButton.onClick.AddListener(OnSlotClicked);
        LoadSlotData();
    }

    public void Initialize(int index)
    {
        slotIndex = index;
        slotNameText.text = $"Slot {index + 1}";
        LoadSlotData();
    }

    public void LoadSlotData()
    {
        string key = $"SaveSlot_{slotIndex}";
        string json = PlayerPrefs.GetString(key, "");

        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                slotData = JsonUtility.FromJson<SaveLoadData>(json);
                isEmpty = false;
                UpdateUIWithData();
            }
            catch
            {
                SetEmpty();
            }
        }
        else
        {
            SetEmpty();
        }
    }

    private void UpdateUIWithData()
    {
        if (emptySlotText != null)
            emptySlotText.SetActive(false);
        if (dataPanel != null)
            dataPanel.SetActive(true);

        if (dateTimeText != null)
            dateTimeText.text = slotData.saveTime;
        if (levelText != null)
            levelText.text = $"Level: {slotData.currentLevel}";
        if (coinsText != null)
            coinsText.text = $"Coins: {slotData.playerCoins}";
    }

    private void SetEmpty()
    {
        isEmpty = true;
        slotData = null;

        if (emptySlotText != null)
            emptySlotText.SetActive(true);
        if (dataPanel != null)
            dataPanel.SetActive(false);

        if (dateTimeText != null)
            dateTimeText.text = "";
        if (levelText != null)
            levelText.text = "";
        if (coinsText != null)
            coinsText.text = "";
    }

    public void SaveData(SaveLoadData data)
    {
        if (data == null) return;

        slotData = data;
        slotData.saveSlotIndex = slotIndex;
        slotData.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        string json = JsonUtility.ToJson(slotData, true);
        PlayerPrefs.SetString($"SaveSlot_{slotIndex}", json);
        PlayerPrefs.Save();

        isEmpty = false;
        UpdateUIWithData();
    }

    private void OnSlotClicked()
    {
        OnSlotSelected?.Invoke(this);
        SelectSlot();
    }

    public void SelectSlot()
    {
        if (backgroundImage != null)
            backgroundImage.color = selectedColor;
    }

    public void DeselectSlot()
    {
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    public void LoadData()
    {
        if (isEmpty) return;
        SaveLoadManager.Instance.LoadGame(slotData);
    }
}