﻿using System.Collections;
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

    public void UI_ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public void UI_UnlockAllLevels()
    {
        for (var i = 0; i < MainManager.Instance.Levels.Length; i++)
        {
            PlayerPrefs.SetString("Level" + i, "111");
        }
    }

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
