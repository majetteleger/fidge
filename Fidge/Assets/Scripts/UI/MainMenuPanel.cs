using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : Panel
{
    public static MainMenuPanel instance = null;

    public EditableLevel HowToLevel;
    
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LevelSelectionPanel.Instance.Show();
        }
    }

    public void UI_LevelSelection()
    {
        LevelSelectionPanel.Instance.Show();
    }

    public void UI_Options()
    {
        OptionsPanel.instance.Show();
    }

    public void UI_Credits()
    {
        CreditsPanel.instance.Show();
    }

    public void UI_HowTo()
    {
        MainManager.Instance.LoadLevel(HowToLevel);
    }

    public void UI_Editor()
    {
        LevelEditorMenuPanel.instance.Show();
    }
}
