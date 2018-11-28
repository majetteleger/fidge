using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

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

    public List<LevelEditPanel.UserLevel> UserLevels { get; set; }
    public SequentialLevel[] Levels { get; set; }
    public EditableLevel LastLoadedLevel { get; private set; }
    public LevelEditPanel.UserLevel LastLoadedUserLevel { get; private set; }
    public bool DirtyMedals { get; set; }
    public DatabaseReference DatabaseReference { get; set; }
    public string UserLevelPath { get; set; }
    public bool AdjustUserLevelButtons { get; set; }

    private bool _lastLevelWasUserMade;
    private List<LevelEditPanel.UserLevel> _userLevelsOnDevice;

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

    private string userId;
    public string UserId
    {
        get
        {
            userId = PlayerPrefs.GetString("UserId");

            if (userId == null)
            {
                userId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("UserId", userId);
            }

            return userId;
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

    public int MaxLevelDifficulty
    {
        get
        {
            if (maxLevelDifficulty == 0)
            {
                foreach (var level in MainManager.Instance.Levels)
                {
                    if (level.Level.Difficulty > maxLevelDifficulty)
                    {
                        maxLevelDifficulty = level.Level.Difficulty;
                    }
                }
            }

            return maxLevelDifficulty;
        }
    }

    public int MaxLevelLength
    {
        get
        {
            if (maxLevelLength == 0)
            {
                foreach (var level in MainManager.Instance.Levels)
                {
                    if (level.Level.MinimumMovesWithFlag > maxLevelLength)
                    {
                        maxLevelLength = level.Level.MinimumMovesWithFlag;
                    }
                }
            }

            return maxLevelLength;
        }
    }

    public int MaxUserLevelDifficulty
    {
        get
        {
            if (maxUserLevelDifficulty == 0)
            {
                foreach (var level in MainManager.Instance.UserLevels)
                {
                    if (level.Difficulty > maxUserLevelDifficulty)
                    {
                        maxUserLevelDifficulty = level.Difficulty;
                    }
                }
            }

            return maxUserLevelDifficulty;
        }
    }

    public int MaxUserLevelLength
    {
        get
        {
            if (maxUserLevelLength == 0)
            {
                foreach (var level in MainManager.Instance.UserLevels)
                {
                    if (level.MinimumMovesWithFlag > maxUserLevelLength)
                    {
                        maxUserLevelLength = level.MinimumMovesWithFlag;
                    }
                }
            }

            return maxUserLevelLength;
        }
    }

    private int maxLevelDifficulty;
    private int maxLevelLength;
    private int maxUserLevelDifficulty;
    private int maxUserLevelLength;

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

#if UNITY_EDITOR
        UserLevelPath = Application.dataPath + "/UserLevels";
#else
        UserLevelPath = Application.persistentDataPath + "/UserLevels";
#endif

        DontDestroyOnLoad(gameObject);

        _userLevelsOnDevice = new List<LevelEditPanel.UserLevel>();
        UserLevels = new List<LevelEditPanel.UserLevel>();

        if (!Directory.Exists(UserLevelPath))
        {
            return;
        }

        foreach (var filePath in Directory.GetFiles(UserLevelPath))
        {
            if (filePath.Contains(".meta"))
            {
                continue;
            }

            var fileContent = File.ReadAllText(filePath);

            _userLevelsOnDevice.Add(JsonUtility.FromJson<LevelEditPanel.UserLevel>(fileContent));
        }

        // FIREBASE

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Debug.Log("Database loaded successfully");

                Firebase.FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://fidge-219217.firebaseio.com/");
                
                DatabaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                
                DatabaseReference.ChildAdded += HandleChildAdded;
                DatabaseReference.ChildChanged += HandleChildAdded;
                DatabaseReference.ChildRemoved += HandleChildRemoved;
            }
            else
            {
                Debug.Log("Can't access database, loading from device snapshot");

                foreach (var userLevel in _userLevelsOnDevice)
                {
                    UserLevels.Add(userLevel);
                }
            }
        });
        
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
    
    public string SaveLevelToDevice(LevelEditPanel.UserLevel level)
    {
        var output = JsonUtility.ToJson(level);

        if (!Directory.Exists(UserLevelPath))
        {
            Directory.CreateDirectory(UserLevelPath);
        }

        var filePath = UserLevelPath + "/" + level.Guid + ".json";

        File.WriteAllText(filePath, output);

        return output;
    }

    private void Start()
    {
        UIManager.Instance.MainMenuPanel.Show();
        UIManager.Instance.LevelSelectionPanel.Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
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

            UIManager.Instance.InGamePanel.ShowLevel(editableLevel);
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
            UIManager.Instance.LevelSelectionPanel.Show();
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

            UIManager.Instance.InGamePanel.ShowLevel(userLevel);
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
            UIManager.Instance.LevelSelectionPanel.Show();
        }
    }

    public void Pay()
    {
        Paid = true;

        UIManager.Instance.LevelSelectionPanel.UpdateSectionBlockers();
        UIManager.Instance.OptionsPanel.UpdatePayButton();
    }

    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        var level = JsonUtility.FromJson<LevelEditPanel.UserLevel>(args.Snapshot.GetRawJsonValue());

        var sameLevelInList = UserLevels.FirstOrDefault(x => x.Guid == level.Guid);

        if (sameLevelInList != null)
        {
            Debug.Log("Level " + level.Guid + " changed");

            var levelIndex = UserLevels.IndexOf(sameLevelInList);
            UserLevels.Remove(sameLevelInList);
            UserLevels.Insert(levelIndex, level);

            if (Directory.Exists(UserLevelPath) && File.Exists(UserLevelPath + "/" + sameLevelInList.Guid + ".json"))
            {
                File.Delete(UserLevelPath + "/" + sameLevelInList.Guid + ".json");
            }

            SaveLevelToDevice(level);
        }
        else
        {
            Debug.Log("Level " + level.Guid + " added");

            UserLevels.Add(level);
            SaveLevelToDevice(level);
        }

        AdjustUserLevelButtons = true;
    }

    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        var level = JsonUtility.FromJson<LevelEditPanel.UserLevel>(args.Snapshot.GetRawJsonValue());
        Debug.Log("Level " + level.Guid + " removed");

        var sameLevelInList = UserLevels.FirstOrDefault(x => x.Guid == level.Guid);

        if (sameLevelInList != null)
        {
            UserLevels.Remove(sameLevelInList);
        }

        if (Directory.Exists(UserLevelPath) && File.Exists(UserLevelPath + "/" + level.Guid + ".json"))
        {
            File.Delete(UserLevelPath + "/" + level.Guid + ".json");
        }

        AdjustUserLevelButtons = true;
    }
}
