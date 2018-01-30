using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupPanel : Panel
{
    public static PopupPanel instance = null;

    public Button BackButton;
    public Button RetryButton;
    public Button ReplayButton;
    public Button NextButton;
    public Button ContinueButton;
    public Button ConfirmButton;
    public Button PlayButton;
    public GameObject Medals;
    public Image TimeMedal;
    public Image MovesMedal;
    public Image FlagMedal;
    public Text Message;
    public string TutorialMessage;
    public string WonMessage;
    public string LostMessage;

    private int _levelToLoad;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void ShowConfirm(int levelIndex)
    {
        _levelToLoad = levelIndex;
        
        Message.text = MainManager.Instance.Levels[levelIndex].name.Substring(2);

        var scripted = MainManager.Instance.Levels[levelIndex].Scripted;

        Medals.SetActive(!scripted);

        var levelSavedValue = Level.GetSavedValue(levelIndex);

        if (!scripted)
        {
            TimeMedal.color = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[0] == '1' ? Color.white : Color.black;
            MovesMedal.color = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[1] == '1' ? Color.white : Color.black;
            FlagMedal.color = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[2] == '1' ? Color.white : Color.black;
        }
        
        BackButton.gameObject.SetActive(true);
        RetryButton.gameObject.SetActive(false);
        ReplayButton.gameObject.SetActive(false);
        NextButton.gameObject.SetActive(false);
        ContinueButton.gameObject.SetActive(false);
        ConfirmButton.gameObject.SetActive(!scripted);
        PlayButton.gameObject.SetActive(scripted);

        Show();
    }

    public void ShowWon(int levelIndex)
    {
        var scripted = MainManager.Instance.Levels[levelIndex].Scripted;

        Message.text = scripted ? TutorialMessage : WonMessage;

        Medals.SetActive(!scripted);

        var levelSavedValue = Level.GetSavedValue(levelIndex);

        if (!scripted)
        {
            TimeMedal.color = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[0] == '1' ? Color.white : Color.black;
            MovesMedal.color = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[1] == '1' ? Color.white : Color.black;
            FlagMedal.color = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[2] == '1' ? Color.white : Color.black;
        }

        BackButton.gameObject.SetActive(true);
        RetryButton.gameObject.SetActive(!scripted);
        ReplayButton.gameObject.SetActive(scripted);
        NextButton.gameObject.SetActive(!scripted && !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue.IndexOf('1') != -1);
        ContinueButton.gameObject.SetActive(scripted);
        ConfirmButton.gameObject.SetActive(false);
        PlayButton.gameObject.SetActive(false);

        Show();
    }

    public void ShowLost(int levelIndex)
    {
        var scripted = MainManager.Instance.Levels[levelIndex].Scripted;

        Message.text = scripted ? TutorialMessage : LostMessage;

        Medals.SetActive(false);

        BackButton.gameObject.SetActive(true);
        RetryButton.gameObject.SetActive(!scripted);
        ReplayButton.gameObject.SetActive(scripted);
        NextButton.gameObject.SetActive(false);
        ContinueButton.gameObject.SetActive(scripted);
        ConfirmButton.gameObject.SetActive(false);
        PlayButton.gameObject.SetActive(false);

        Show();
    }
    
    public void UI_Back()
    {
        LevelSelectionPanel.instance.Show();
    }

    public void UI_Retry()
    {
        InGamePanel.instance.Show();
        MainManager.Instance.ReloadLevel();
    }

    public void UI_Next()
    {
        InGamePanel.instance.Show();
        MainManager.Instance.LoadNextLevel();
    }

    public void UI_Confirm()
    {
        InGamePanel.instance.Show();
        MainManager.Instance.LoadLevel(_levelToLoad);
    }
}
