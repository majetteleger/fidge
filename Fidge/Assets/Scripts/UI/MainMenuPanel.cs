using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : Panel
{
    public EditableLevel HowToLevel;
    
    void Start()
    {
        SetupSounds();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UIManager.Instance.LevelSelectionPanel.Show();
        }
    }

    public void UI_LevelSelection()
    {
        UIManager.Instance.LevelSelectionPanel.Show();
    }

    public void UI_Options()
    {
        UIManager.Instance.OptionsPanel.Show();
    }

    public void UI_Credits()
    {
        UIManager.Instance.CreditsPanel.Show();
    }

    public void UI_HowTo()
    {
        MainManager.Instance.LoadLevel(HowToLevel);
    }

    public void UI_Editor()
    {
        UIManager.Instance.LevelEditorMenuPanel.Show();
    }
}
