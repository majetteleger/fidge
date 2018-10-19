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

            if (level.Uploaded && MainManager.Instance.UserLevels.All(x => x.Guid != level.Guid))
            {
                if (Directory.Exists(MainManager.Instance.UserLevelPath) && File.Exists(MainManager.Instance.UserLevelPath + "/" + level.Guid + ".json"))
                {
                    File.Delete(MainManager.Instance.UserLevelPath + "/" + level.Guid + ".json");
                }
            }
            else if (!level.Uploaded)
            {
                MainManager.Instance.DatabaseReference.Child(level.Guid).SetRawJsonValueAsync(fileContent).ContinueWith(x => {
                    if (x.IsCompleted)
                    {
                        Debug.Log("level " + level.Guid + " uploaded");
                        level.Uploaded = true;
                        MainManager.Instance.DatabaseReference.Child(level.Guid).Child("Uploaded").SetValueAsync(true);
                        MainManager.Instance.SaveLevelToDevice(level);
                    }
                });
            }
        }

        EditButton.interactable = MainManager.Instance.UserLevels.Any(x => x.UserId == MainManager.Instance.UserId);
        PlayButton.interactable = MainManager.Instance.UserLevels.Any(x => x.Valid);

        base.Show(originPanel);
    }
    
    public override void Hide()
    {
        base.Hide();
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

    public void UI_Back()
    {
        UIManager.Instance.MainMenuPanel.Show();
    }
}
