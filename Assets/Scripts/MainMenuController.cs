using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    private void Start()
    {
        UpdateContinueButton();
    }

    public void NewGame()
    {
        SaveLoadManager.Instance.CreateNewGame(0);
    }

    public void ContinueGame()
    {
        SaveLoadManager.Instance.ContinueToLevelSelect();
    }

    public void Setting()
    {
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void UpdateContinueButton()
    {
        if (continueButton != null && continueButtonText != null)
        {
            continueButton.interactable = true;
            bool hasSave = SaveLoadManager.Instance.HasAnySave();
            continueButtonText.text = hasSave ? "CONTINUE" : "NEW GAME";
        }
    }
}