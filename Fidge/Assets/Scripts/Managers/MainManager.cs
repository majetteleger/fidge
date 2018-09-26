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
    public EditableLevel LastLoadedLevel { get; private set; }
    public LevelEditPanel.UserLevel LastLoadedUserLevel { get; private set; }
    public bool DirtyMedals { get; set; }

    private bool _lastLevelWasUserMade;

    public bool Paid
    {
        get
        {
            return PlayerPrefs.GetInt("Paid") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Paid", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
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
            if (DirtyMedals)
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
            DirtyMedals = true;
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

        DirtyMedals = true;
    }

    private void Start()
    {
        MainMenuPanel.instance.Show();
    }

    public void ReloadLevel()
    {
        if (_lastLevelWasUserMade)
        {
            LoadLevel(LastLoadedUserLevel);
        }
        else
        {
            LoadLevel(LastLoadedLevel);
        }
    }

    public void ReloadUserLevel()
    {
        LoadLevel(LastLoadedUserLevel);
    }

    public void LoadNextLevel()
    {
        LoadLevel(Levels[LastLoadedLevel.Index].NextLevel);
    }

    public void LoadLevel(EditableLevel editableLevel)
    {
        _lastLevelWasUserMade = false;

        if (ActiveLevel != null)
        {
            DestroyImmediate(ActiveLevel.gameObject);
        }
        
        if (editableLevel != null)
        {
            TutorialManager.Instance.ListeningForTutorialChecks = true;

            InGamePanel.instance.ShowLevel(editableLevel);
            var level = editableLevel.InstantiateLevel();
            
            level.Index = editableLevel.Index;
            LastLoadedLevel = editableLevel;

            var tutorialTaggers = FindObjectsOfType<TutorialTagger>();

            foreach (var tutorialTagger in tutorialTaggers)
            {
                tutorialTagger.CheckForTutorial();
            }

            TutorialManager.Instance.ListeningForTutorialChecks = false;
        }
        else
        {
            LevelSelectionPanel.Instance.Show();
        }
    }

    public void LoadLevel(LevelEditPanel.UserLevel userLevel)
    {
        _lastLevelWasUserMade = true;

        if (ActiveLevel != null)
        {
            DestroyImmediate(ActiveLevel.gameObject);
        }

        if (userLevel != null)
        {
            TutorialManager.Instance.ListeningForTutorialChecks = true;

            InGamePanel.instance.ShowLevel(userLevel);
            var level = userLevel.InstantiateLevel();

            level.Guid = userLevel.Guid;
            LastLoadedUserLevel = userLevel;

            var tutorialTaggers = FindObjectsOfType<TutorialTagger>();

            foreach (var tutorialTagger in tutorialTaggers)
            {
                tutorialTagger.CheckForTutorial();
            }

            TutorialManager.Instance.ListeningForTutorialChecks = false;
        }
        else
        {
            LevelSelectionPanel.Instance.Show();
        }
    }

    public void Pay()
    {
        Paid = true;

        LevelSelectionPanel.Instance.UpdateSectionBlockers();
        OptionsPanel.instance.UpdatePayButton();
    }
}
