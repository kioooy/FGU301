using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerActionMenuCreator : EditorWindow
{
    [MenuItem("Tools/TD/Create Tower Action Menu")]
    public static void ShowWindow()
    {
        GetWindow<TowerActionMenuCreator>("Tower Action Menu Creator");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Create Tower Action Menu in Canvas"))
        {
            CreateMenu();
        }
    }

    private void CreateMenu()
    {
        UIController uiController = FindFirstObjectByType<UIController>();
        Canvas canvas = null;

        if (uiController != null)
        {
            // Try to find the canvas that towerPanel is in
            SerializedObject so = new SerializedObject(uiController);
            var towerPanelProp = so.FindProperty("towerPanel");
            if (towerPanelProp.objectReferenceValue != null)
            {
                canvas = ((GameObject)towerPanelProp.objectReferenceValue).GetComponentInParent<Canvas>();
            }
        }

        if (canvas == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.isRootCanvas || c.name.Contains("Canvas"))
                {
                    canvas = c;
                    break;
                }
            }
        }
        
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        // Create Panel (Bảng to)
        GameObject menuPanel = new GameObject("TowerActionMenu", typeof(RectTransform), typeof(Image));
        menuPanel.transform.SetParent(canvas.transform, false);
        menuPanel.transform.SetAsLastSibling(); 
        RectTransform rect = menuPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 400);
        rect.anchoredPosition = new Vector2(0, 0); // Center of screen
        Image panelImage = menuPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Deep dark blue/gray

        // Add a Title
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(menuPanel.transform, false);
        TextMeshProUGUI titleText = titleGO.GetComponent<TextMeshProUGUI>();
        titleText.text = "TOWER MANAGEMENT";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 160);

        // Info Text (Large)
        GameObject infoGO = new GameObject("InfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
        infoGO.transform.SetParent(menuPanel.transform, false);
        TextMeshProUGUI infoText = infoGO.GetComponent<TextMeshProUGUI>();
        infoText.fontSize = 28;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.text = "Tower Name\nDamage: 0";
        infoGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);

        // Upgrade Button (GREEN)
        GameObject upgradeGO = CreateButton(menuPanel.transform, "UpgradeButton", "UPGRADE", new Vector2(-150, -80), new Color(0.2f, 0.8f, 0.2f));
        Button upgradeBtn = upgradeGO.GetComponent<Button>();
        TextMeshProUGUI upgradePrice = CreateText(upgradeGO.transform, "Price", "100", new Vector2(0, -50));
        upgradePrice.color = Color.yellow;

        // Sell Button (RED)
        GameObject sellGO = CreateButton(menuPanel.transform, "SellButton", "SELL", new Vector2(150, -80), new Color(0.8f, 0.2f, 0.2f));
        Button sellBtn = sellGO.GetComponent<Button>();
        TextMeshProUGUI sellPrice = CreateText(sellGO.transform, "Price", "50", new Vector2(0, -50));
        sellPrice.color = new Color(1, 0.8f, 0.8f);

        // Close Button
        GameObject closeGO = CreateButton(menuPanel.transform, "CloseButton", "X", new Vector2(275, 175), new Color(0.7f, 0.2f, 0.2f));
        closeGO.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        Button closeBtn = closeGO.GetComponent<Button>();
        
        // NOTE: Background click-to-close is handled via Update in UIController

        // Link to UIController if possible
        if (uiController != null)
        {
            var serializedUI = new SerializedObject(uiController);
            serializedUI.Update();
            serializedUI.FindProperty("towerActionMenu").objectReferenceValue = menuPanel;
            serializedUI.FindProperty("towerInfoText").objectReferenceValue = infoText;
            serializedUI.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;
            serializedUI.FindProperty("sellButton").objectReferenceValue = sellBtn;
            serializedUI.FindProperty("closeButton").objectReferenceValue = closeBtn;
            serializedUI.FindProperty("upgradePriceText").objectReferenceValue = upgradePrice;
            serializedUI.FindProperty("sellPriceText").objectReferenceValue = sellPrice;
            serializedUI.ApplyModifiedProperties();
            
            EditorUtility.DisplayDialog("Success", "Tower Action Menu created!\nPLEASE RESTART THE GAME (STOP & PLAY) TO SYNC BUTTONS.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Menu Created", "Menu created but UIController not found. Please link it manually.", "OK");
        }
    }

    private GameObject CreateButton(Transform parent, string name, string label, Vector2 pos, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 60);
        go.GetComponent<Image>().color = color;
        
        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.color = Color.white;
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        
        return go;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string label, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }
}
