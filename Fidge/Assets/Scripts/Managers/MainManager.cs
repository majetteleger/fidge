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
        public EditableLevel Tutorial;
        public EditableLevel[] Levels;
    }

    public class SequentialLevel
    {
        public EditableLevel Level;
        public EditableLevel NextLevel;

        public SequentialLevel(EditableLevel level, EditableLevel nextLevel)
        {
            Level = level;
            NextLevel = nextLevel;
        }
    }
    
    public Section[] Sections;
    public float LevelUnlockMultiplier;

    public static MainManager Instance;
    
    public SequentialLevel[] Levels { get; set; }
    public bool DirtyMedals { get; set; }

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

    private int medals;
    public int Medals
    {
        get
        {
            if (DirtyMedals || medals == 0)
            {
                medals = 0;

                for (var i = 0; i < Levels.Length; i++)
                {
                    var savedValue = PlayerPrefs.GetString("Level" + i);

                    for (var j = 0; j < savedValue.Length; j++)
                    {
                        if (savedValue[j] == '1')
                        {
                            medals++;
                        }
                    }
                }

                DirtyMedals = false;
            }
            
            return medals;
        }
        set 
        { 
            medals = value; 
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

        var editableLevels = new List<SequentialLevel>();

        var tempLevelIndex = 0;

        for (var i = 0; i < Sections.Length; i++)
        {
            if (Sections[i].Tutorial != null)
            {
                Sections[i].Tutorial.Index = -1;
            }
            
            for (var j = 0; j < Sections[i].Levels.Length; j++)
            {
                Sections[i].Levels[j].Index = tempLevelIndex;
                var nextLevel = (EditableLevel) null;

                if (j < Sections[i].Levels.Length - 1)
                {
                    nextLevel = Sections[i].Levels[j + 1];
                }
                else if(i < Sections.Length - 1)
                {
                    nextLevel = Sections[i + 1].Levels[0];
                }

                editableLevels.Add(new SequentialLevel(Sections[i].Levels[j], nextLevel));

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
        LoadLevel(Levels[LastLoadedLevelIndex].Level);
    }

    public void LoadNextLevel()
    {
        LoadLevel(Levels[LastLoadedLevelIndex].NextLevel);
    }

    public void LoadLevel(EditableLevel editableLevel)
    {
        if (ActiveLevel != null)
        {
            DestroyImmediate(ActiveLevel.gameObject);
        }
        
        if (editableLevel != null)
        {
            var level = editableLevel.InstantiateLevel();
            InGamePanel.instance.Show();

            level.Index = editableLevel.Index;

            if (editableLevel.Index > 0)
            {
                LastLoadedLevelIndex = editableLevel.Index;
            }
        }
        else
        {
            LevelSelectionPanel.instance.Show();
        }
    }
}
