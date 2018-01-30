using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : Panel
{
    public static MainMenuPanel instance = null;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void UI_LevelSelection()
    {
        LevelSelectionPanel.instance.Show();
    }

    public void UI_Options()
    {
        OptionsPanel.instance.Show();
    }

    public void UI_Quit()
    {
        Application.Quit();
    }
}
