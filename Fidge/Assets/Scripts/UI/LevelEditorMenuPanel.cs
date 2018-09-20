using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEditorMenuPanel : Panel
{
    public static LevelEditorMenuPanel instance = null;
    
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenuPanel.instance.Show();
        }
    }

    public override void Show(Panel originPanel = null)
    {
        base.Show(originPanel);
    }
    
    public override void Hide()
    {
        base.Hide();
    }

    public void UI_New()
    {
        LevelEditPanel.Instance.Show();
    }

    public void UI_Edit()
    {
        // DISABLE IF NO USER LEVEL

        UserLevelsPanel.Instance.ShowWithActivity(UserLevelsPanel.UserActivity.Edit);
    }

    public void UI_Play()
    {
        // DISABLE IF NO USER LEVEL

        UserLevelsPanel.Instance.ShowWithActivity(UserLevelsPanel.UserActivity.Play);
    }

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
