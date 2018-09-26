using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public GameObject LevelButtonPrefab;
    public GameObject LevelButtonRowPrefab;
    public Transform LevelButtonContainer;
    public int ButtonsPerRow;
    public Text Header;
    [TextArea] public string EditHeader;
    [TextArea] public string PlayHeader;

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
        Header.text = _activity == UserActivity.Edit ? EditHeader : PlayHeader;

        UpdateLevelButtons();

        base.Show(originPanel);
    }

    private void UpdateLevelButtons()
    {
        foreach (var buttonRow in LevelButtonContainer.GetComponentsInChildren<Image>())
        {
            Destroy(buttonRow.gameObject);
        }

        if (!Directory.Exists(Application.dataPath + "/UserLevels"))
        {
            return;
        }

        var userLevelList = new List<LevelEditPanel.UserLevel>();

        foreach (var filePath in Directory.GetFiles(Application.dataPath + "/UserLevels"))
        {
            if (filePath.Contains(".meta"))
            {
                continue;
            }

            var fileContent = File.ReadAllText(filePath);

            userLevelList.Add(JsonUtility.FromJson<LevelEditPanel.UserLevel>(fileContent));
        }

        var row = (Transform)null;

        for (var i = 0; i < userLevelList.Count; i++)
        {
            var level = userLevelList[i];

            if (_activity == UserActivity.Play && !level.Valid)
            {
                continue;
            }
            
            if (i % ButtonsPerRow == 0)
            {
                row = Instantiate(LevelButtonRowPrefab, LevelButtonContainer).transform;
            }
            
            var button = Instantiate(LevelButtonPrefab, row).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = (i + 1).ToString();
            button.onClick.AddListener(() =>
            {
                PopupPanel.instance.ShowConfirm(level, _activity);
            });

            button.transform.GetChild(1).gameObject.SetActive(true);
            button.transform.GetChild(2).gameObject.SetActive(false);

            var medals = button.transform.GetChild(1).GetComponentsInChildren<Image>();
            var savedMedals = Level.GetSavedValue(level.Guid);

            for (var j = 0; j < medals.Length; j++)
            {
                medals[j].color = !string.IsNullOrEmpty(savedMedals) && savedMedals[j] == '1' ? Color.black : new Color(0, 0, 0, 0.25f);
            }

            // add the validity indicator
        }
    }

    public void UI_Back()
    {
        LevelEditorMenuPanel.instance.Show();
    }
}
