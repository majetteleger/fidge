using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserLevelsPanel : Panel
{
    public enum UserActivity
    {
        Edit,
        Play
    }

    public static UserLevelsPanel Instance;

    private UserActivity _activity;

    void Awake()
    {
        Instance = this;
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
            MainMenuPanel.instance.Show();
        }
    }

    public void ShowWithActivity(UserActivity activity)
    {
        _activity = activity;
        Show();
    }

    public override void Show(Panel originPanel = null)
    {
        UpdateLevelButtons();

        base.Show(originPanel);
    }

    private void UpdateLevelButtons()
    {
        // Read the jsons of the user levels
        // populate a grid of level buttons
        // assigne functions to the buttons
    }

    public void UI_Back()
    {
        LevelEditorMenuPanel.instance.Show();
    }
}
