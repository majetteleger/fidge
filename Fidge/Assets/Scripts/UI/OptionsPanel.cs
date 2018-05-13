using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsPanel : Panel
{
    public static OptionsPanel instance = null;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        if(!IsActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenuPanel.instance.Show();
        }
    }

    public void UI_ResetProgress()
    {
        MainManager.Instance.Medals = 0;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        MainManager.Instance.DirtyMedals = true;
    }

    public void UI_UnlockAllLevels()
    {
        for (var i = 0; i < MainManager.Instance.Levels.Length; i++)
        {
            PlayerPrefs.SetString("Level" + i, "111");
        }

        MainManager.Instance.DirtyMedals = true;
    }

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
