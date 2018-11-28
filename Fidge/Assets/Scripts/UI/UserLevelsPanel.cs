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
    
    public GameObject LevelButtonPrefab;
    public GameObject LevelButtonRowPrefab;
    public Transform LevelSection;
    public Transform LevelButtonContainer;
    public ScrollRect LevelButtonScroll;
    public GameObject FiltersContainer; 
    public Image[] DifficultyTicks;
    public Image[] LengthTicks;
    public float InactiveTickAlpha;
    public int ButtonsPerRow;
    public Text Header;
    [TextArea] public string EditHeader;
    [TextArea] public string PlayHeader;
    public Sprite ValidSprite;
    public Sprite InvalidSprite;

    private UserActivity _activity;
    private RectTransform _section;
    private int _difficultyFilterLevel;
    private int _lengthFilterLevel;
    private Dictionary<Button, LevelEditPanel.UserLevel> _buttons;

    void Start()
    {
        SetupSounds();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.MainMenuPanel.Show();
        }
    }

    public void ShowWithActivity(UserActivity activity, Panel originPanel)
    {
        _activity = activity;
        Show(originPanel);
    }

    public override void Show(Panel originPanel = null)
    {
        Header.text = _activity == UserActivity.Edit ? EditHeader : PlayHeader;
        FiltersContainer.SetActive(_activity == UserActivity.Play);

        UpdateLevelButtons();

        base.Show(originPanel);

        ForceLayoutRebuilding(_section);

        if (originPanel != UIManager.Instance.PopupPanel && originPanel != UIManager.Instance.InGamePanel)
        {
            LevelButtonScroll.verticalNormalizedPosition = 1;

            if (_activity == UserActivity.Play)
            {
                _difficultyFilterLevel = 0;
                _lengthFilterLevel = 0;
            }
        }

        if (_activity == UserActivity.Play)
        {
            AdjustDifficulty();
            AdjustLength();
        }
    }

    private void UpdateLevelButtons()
    {
        foreach (var buttonRow in LevelSection.GetComponentsInChildren<Image>())
        {
            Destroy(buttonRow.gameObject);
        }

        _buttons = new Dictionary<Button, LevelEditPanel.UserLevel>();

        _section = LevelSection.GetComponent<RectTransform>();

        var row = (Transform)null;
        
        for (var i = 0; i < MainManager.Instance.UserLevels.Count; i++)
        {
            var level = MainManager.Instance.UserLevels[i];

            if (_activity == UserActivity.Play && !level.Valid)
            {
                continue;
            }
            
            if (i % ButtonsPerRow == 0 || row == null)
            {
                row = Instantiate(LevelButtonRowPrefab, LevelSection).transform;
            }
            
            var button = Instantiate(LevelButtonPrefab, row).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = (i + 1).ToString();
            button.onClick.AddListener(() =>
            {
                UIManager.Instance.PopupPanel.ShowConfirm(level, _activity);
            });

            button.transform.GetChild(1).gameObject.SetActive(_activity == UserActivity.Play);
            button.transform.GetChild(2).gameObject.SetActive(false);
            button.transform.GetChild(3).gameObject.SetActive(_activity == UserActivity.Edit);

            if (_activity == UserActivity.Edit)
            {
                button.transform.GetChild(3).GetComponent<Image>().sprite = level.Valid ? ValidSprite : InvalidSprite;
            }
            
            var medals = button.transform.GetChild(1).GetComponentsInChildren<Image>();
            var savedMedals = Level.GetSavedValue(level.Guid);

            for (var j = 0; j < medals.Length; j++)
            {
                medals[j].color = !string.IsNullOrEmpty(savedMedals) && savedMedals[j] == '1' ? Color.black : new Color(0, 0, 0, 0.25f);
            }

            _buttons.Add(button, level);
        }
    }

    private void AdjustLength()
    {
        for (var i = 0; i < LengthTicks.Length; i++)
        {
            var newColor = LengthTicks[i].color;
            newColor.a = i < _lengthFilterLevel ? 1f : InactiveTickAlpha;

            LengthTicks[i].color = newColor;
        }

        foreach (var pair in _buttons)
        {
            AdjustButton(pair.Key, pair.Value);
        }
    }

    private void AdjustDifficulty()
    {
        for (var i = 0; i < DifficultyTicks.Length; i++)
        {
            var newColor = DifficultyTicks[i].color;
            newColor.a = i < _difficultyFilterLevel ? 1f : InactiveTickAlpha;

            DifficultyTicks[i].color = newColor;
        }

        foreach (var pair in _buttons)
        {
            AdjustButton(pair.Key, pair.Value);
        }
    }

    private void AdjustButton(Button button, LevelEditPanel.UserLevel level)
    {
        var lengthRatio = (float)level.MinimumMovesWithFlag / (float)MainManager.Instance.MaxUserLevelLength;
        var difficultyRatio = (float)level.Difficulty / (float)MainManager.Instance.MaxUserLevelDifficulty;

        var buttonEnabled = (_lengthFilterLevel == 0 || (lengthRatio > (_lengthFilterLevel - 1) / 3f && lengthRatio < (_lengthFilterLevel / 3f) + 0.01f)) &&
                            (_difficultyFilterLevel == 0 || (difficultyRatio > (_difficultyFilterLevel - 1) / 3f && difficultyRatio < (_difficultyFilterLevel / 3f) + 0.01f));

        button.GetComponent<Image>().color = buttonEnabled ? button.colors.normalColor : button.colors.disabledColor;
    }

    public void UI_CycleDifficulty()
    {
        _difficultyFilterLevel++;

        if (_difficultyFilterLevel > 3)
        {
            _difficultyFilterLevel = 0;
        }

        AdjustDifficulty();
    }

    public void UI_CycleLength()
    {
        _lengthFilterLevel++;

        if (_lengthFilterLevel > 3)
        {
            _lengthFilterLevel = 0;
        }

        AdjustLength();
    }

    public void UI_Back()
    {
        UIManager.Instance.LevelEditorMenuPanel.Show();
    }
}
