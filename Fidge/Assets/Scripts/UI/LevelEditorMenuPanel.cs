using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEditorMenuPanel : Panel
{
    public Button EditButton;
    public Button PlayButton;
    public GameObject Blocker;
    public GameObject Spacer;
    public GameObject MessageBubble;
    public GameObject MessageBubbleBackground;

    void Start()
    {
        SetupSounds();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.MainMenuPanel.Show();
        }

        if (MainManager.Instance.AdjustUserLevelButtons)
        {
            AdjustButtons();
            MainManager.Instance.AdjustUserLevelButtons = false;
        }
    }

    public override void Show(Panel originPanel = null)
    {
        // Adjust the database and device levels if not in sync

        foreach (var filePath in Directory.GetFiles(MainManager.Instance.UserLevelPath))
        {
            if (filePath.Contains(".meta"))
            {
                continue;
            }

            var fileContent = File.ReadAllText(filePath);
            var level = JsonUtility.FromJson<LevelEditPanel.UserLevel>(fileContent);

            if (MainManager.Instance.UserLevels.All(x => x.Guid != level.Guid))
            {
                if (Directory.Exists(MainManager.Instance.UserLevelPath) && File.Exists(MainManager.Instance.UserLevelPath + "/" + level.Guid + ".json"))
                {
                    File.Delete(MainManager.Instance.UserLevelPath + "/" + level.Guid + ".json");
                }
            }
        }

        AdjustButtons();

        base.Show(originPanel);
    }
    
    public override void Hide()
    {
        base.Hide();
    }

    public void AdjustButtons()
    {
        EditButton.interactable = MainManager.Instance.UserLevels.Any(x => x.UserId == MainManager.Instance.UserId);
        PlayButton.interactable = MainManager.Instance.UserLevels.Any(x => x.Valid);
    }
    
    public void UI_New()
    {
        UIManager.Instance.LevelEditPanel.Show();
    }

    public void UI_Edit()
    {
        UIManager.Instance.UserLevelsPanel.ShowWithActivity(UserLevelsPanel.UserActivity.Edit, this);
    }

    public void UI_Play()
    {
        UIManager.Instance.UserLevelsPanel.ShowWithActivity(UserLevelsPanel.UserActivity.Play, this);
    }

    public void UI_Info()
    {
        MessageBubble.SetActive(true);
        MessageBubbleBackground.SetActive(true);
    }

    public void UI_Back()
    {
        UIManager.Instance.MainMenuPanel.Show();
    }
    
    public void UI_ClosePopup()
    {
        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
    }
}
