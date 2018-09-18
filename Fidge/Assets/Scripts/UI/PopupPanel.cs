using System;
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
    public Button ConfirmButton;
    public Button PlayButton;
    public GameObject MedalsParent;
    public CanvasGroup[] Medals;
    public Text Message;
    public Text MedalsObtainedText;
    public string TutorialMessage;
    public string WonMessage;
    public string LostMessage;
    public float UnobtainedOpacity;

    private EditableLevel _levelToLoad;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        SetupSounds();
    }

    void Update()
    {
        if (!IsActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LevelSelectionPanel.Instance.Show(instance);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ConfirmButton.IsActive())
            {
                InGamePanel.instance.Show();
                MainManager.Instance.LoadLevel(_levelToLoad);
            }
            else if (NextButton.IsActive())
            {
                InGamePanel.instance.Show();
                MainManager.Instance.LoadNextLevel();
            }
            else if (RetryButton.IsActive())
            {
                InGamePanel.instance.Show();
                MainManager.Instance.ReloadLevel();
            }
        }
    }

    public void ShowConfirm(EditableLevel level)
    {
        _levelToLoad = level;
        
        var scripted = level.Scripted;

        Message.text = scripted ? level.name : "level " + (level.Index + 1);

        MedalsParent.SetActive(!scripted);

        var levelSavedValue = Level.GetSavedValue(level.Index);

        if (!scripted)
        {
            for (int i = 0; i < Medals.Length; i++)
            {
                var gotMedal = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[i] == '1';

                foreach (var shadow in Medals[i].GetComponentsInChildren<Shadow>())
                {
                    shadow.enabled = gotMedal;
                }

                Medals[i].transform.GetChild(1).GetComponent<Image>().enabled = gotMedal;
                Medals[i].alpha = gotMedal ? 1f : UnobtainedOpacity;
            }
        }
        
        BackButton.gameObject.SetActive(true);
        RetryButton.gameObject.SetActive(false);
        ReplayButton.gameObject.SetActive(false);
        NextButton.gameObject.SetActive(false);
        ConfirmButton.gameObject.SetActive(!scripted);
        PlayButton.gameObject.SetActive(scripted);
        MedalsObtainedText.transform.parent.gameObject.SetActive(false);

        Show();
    }

    public void ShowWon(EditableLevel level)
    {
        var scripted = level == null || level.Scripted;

        Message.text = scripted ? TutorialMessage : WonMessage;

        MedalsParent.SetActive(!scripted);

        var levelSavedValue = level != null ? Level.GetSavedValue(level.Index) : string.Empty;

        var nextLevelButtonActive = !scripted;
        
        if (!scripted)
        {
            for (int i = 0; i < Medals.Length; i++)
            {
                var gotMedal = !string.IsNullOrEmpty(levelSavedValue) && levelSavedValue[i] == '1';

                foreach (var shadow in Medals[i].GetComponentsInChildren<Shadow>())
                {
                    shadow.enabled = gotMedal;
                }

                Medals[i].transform.GetChild(1).GetComponent<Image>().enabled = gotMedal;
                Medals[i].alpha = gotMedal ? 1f : UnobtainedOpacity;
            }

            var nextLevel = level.Index + 1 < MainManager.Instance.Levels.Length - 1 ? MainManager.Instance.Levels[level.Index + 1].Level : null;

            if (nextLevel != null)
            {
                var nextLevelUnlocked = nextLevel != null && nextLevel.Unlocked;

                NextButton.interactable = nextLevelUnlocked;
                NextButton.transform.GetChild(0).gameObject.SetActive(nextLevelUnlocked);
                NextButton.transform.GetChild(1).gameObject.SetActive(!nextLevelUnlocked);
                NextButton.transform.GetChild(2).gameObject.SetActive(!nextLevelUnlocked);

                if (!nextLevelUnlocked)
                {
                    NextButton.transform.GetChild(2).GetComponentInChildren<Text>().text = nextLevel.MedalsNeededToUnlock.ToString();
                }
            }
            else
            {
                nextLevelButtonActive = false;
            }
        }

        NextButton.gameObject.SetActive(nextLevelButtonActive);

        BackButton.gameObject.SetActive(true);

        RetryButton.gameObject.SetActive(!scripted);

        ReplayButton.gameObject.SetActive(scripted);
        
        ConfirmButton.gameObject.SetActive(false);

        PlayButton.gameObject.SetActive(false);

        MedalsObtainedText.transform.parent.gameObject.SetActive(true);
        MedalsObtainedText.text = MainManager.Instance.Medals.ToString();

        Show();
    }

    /*public void ShowLost(EditableLevel level)
    {
        var scripted = level.Scripted;

        Message.text = scripted ? TutorialMessage : LostMessage;

        MedalsParent.SetActive(false);

        BackButton.gameObject.SetActive(true);
        RetryButton.gameObject.SetActive(!scripted);
        ReplayButton.gameObject.SetActive(scripted);
        NextButton.gameObject.SetActive(false);
        ConfirmButton.gameObject.SetActive(false);
        PlayButton.gameObject.SetActive(false);
        MedalsObtainedText.transform.parent.gameObject.SetActive(false);

        Show();
    }*/
    
    public void UI_Back()
    {
        LevelSelectionPanel.Instance.Show(instance);
    }

    public void UI_Retry()
    {
        MainManager.Instance.ReloadLevel();
    }

    public void UI_Next()
    {
        MainManager.Instance.LoadNextLevel();
    }

    public void UI_Confirm()
    {
        MainManager.Instance.LoadLevel(_levelToLoad);
    }
}
