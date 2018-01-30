using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionPanel : Panel
{
    public static LevelSelectionPanel instance = null;

    public GameObject LevelButtonPefab;
    public GameObject LevelSectionPefab;
    public GameObject LevelSectionTitlePefab;
    public ScrollRect LevelButtonScroll;
    public Transform LevelButtonContainer;

    private Button[] _levelButtons;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    void Start()
    {
        _levelButtons = new Button[MainManager.Instance.Levels.Length];

        var tempLevelIndex = 0;

        foreach (var levelSection in MainManager.Instance.Sections)
        {
            var sectionTitle = Instantiate(LevelSectionTitlePefab, LevelButtonContainer).GetComponentInChildren<Text>();
            sectionTitle.text = levelSection.Title;

            var section = Instantiate(LevelSectionPefab, LevelButtonContainer).transform;

            for (var i = 0; i < levelSection.Levels.Length; i++)
            {
                var levelIndex = tempLevelIndex;

                var button = Instantiate(LevelButtonPefab, section).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = (levelIndex + 1).ToString();
                button.onClick.AddListener(() => {
                    PopupPanel.instance.ShowConfirm(levelIndex);
                });

                _levelButtons[tempLevelIndex] = button;

                tempLevelIndex++;
            }
        }
        
        LevelButtonScroll.verticalNormalizedPosition = 1;
    }

    public override void Show()
    {
        UpdateLevelButtons();

        base.Show();
    }

    private void UpdateLevelButtons()
    {
        for (var i = 0; i < _levelButtons.Length; i++)
        {
            var previousLevelSavedValue = i > 0 ? Level.GetSavedValue(i - 1) : null;
            _levelButtons[i].interactable = previousLevelSavedValue == null || previousLevelSavedValue.IndexOf('1') != -1;
        }
    }

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
