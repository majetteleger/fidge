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
            var sectionTitle = Instantiate(LevelSectionTitlePefab, LevelButtonContainer);
            sectionTitle.GetComponentInChildren<Text>().text = levelSection.Title;

            var sectionTutorial = levelSection.Tutorial;
            var tutorialButton = sectionTitle.GetComponentInChildren<Button>();

            if (sectionTutorial != null)
            {
                tutorialButton.GetComponent<CanvasGroup>().alpha = 1;
                tutorialButton.onClick.AddListener(() => {
                    PopupPanel.instance.ShowConfirm(sectionTutorial);
                });
            }
            else
            {
                tutorialButton.GetComponent<CanvasGroup>().alpha = 0;
                tutorialButton.onClick.RemoveAllListeners();
            }
            
            var section = Instantiate(LevelSectionPefab, LevelButtonContainer).transform;

            for (var i = 0; i < levelSection.Levels.Length; i++)
            {
                var level = levelSection.Levels[i];

                var button = Instantiate(LevelButtonPefab, section).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = (level.Index + 1).ToString();
                button.onClick.AddListener(() => {
                    PopupPanel.instance.ShowConfirm(level);
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(LevelButtonContainer.GetComponent<RectTransform>());

        base.Show();
    }

    private void UpdateLevelButtons()
    {
        for (var i = 0; i < _levelButtons.Length; i++)
        {
            _levelButtons[i].interactable = MainManager.Instance.Medals >= Mathf.CeilToInt(i * MainManager.Instance.LevelUnlockMultiplier);
        }
    }

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
