using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    [Serializable]
    public class Section
    {
        public string Title;
        public EditableLevel[] Levels;
    }
    
    public Section[] Sections;

    public static MainManager Instance;
    
    public EditableLevel[] Levels { get; set; }

    public int LastLoadedLevelIndex { get; private set; }
    
    private Player player;
    public Player Player
    {
        get
        {
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }

            return player;
        }
    }

    private Level activeLevel;
    public Level ActiveLevel
    {
        get
        {
            if(activeLevel == null)
            {
                activeLevel = FindObjectOfType<Level>();
            }
            
            return activeLevel;
        }
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // Load levels

        var editableLevels = new List<EditableLevel>();

        var tempLevelIndex = 0;

        for (var i = 0; i < Sections.Length; i++)
        {
            for (var j = 0; j < Sections[i].Levels.Length; j++)
            {
                Sections[i].Levels[j].Index = tempLevelIndex;
                editableLevels.Add(Sections[i].Levels[j]);

                tempLevelIndex++;
            }
        }

        Levels = editableLevels.ToArray();
    }

    private void Start()
    {
        MainMenuPanel.instance.Show();
    }

    public void ReloadLevel()
    {
        LoadLevel(LastLoadedLevelIndex);
    }

    public void LoadNextLevel()
    {
        LoadLevel(LastLoadedLevelIndex + 1);
    }

    public void LoadLevel(int levelIndex)
    {
        if (ActiveLevel != null)
        {
            DestroyImmediate(ActiveLevel.gameObject);
        }
        
        if (levelIndex <= Levels.Length - 1)
        {
            var level = Levels[levelIndex].InstantiateLevel();
            InGamePanel.instance.Show();

            level.Index = levelIndex;
            LastLoadedLevelIndex = levelIndex;
        }
        else
        {
            LevelSelectionPanel.instance.Show();
        }
    }
}
