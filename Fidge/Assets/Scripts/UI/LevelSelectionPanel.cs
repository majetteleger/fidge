using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionPanel : Panel
{
    public static LevelSelectionPanel instance = null;

    public GameObject LevelButtonPrefab;
    public GameObject LevelButtonRowPrefab;
    //public GameObject LevelDetailsPrefab;
    public GameObject LevelSectionPrefab;
    public GameObject LevelSectionTitlePrefab;
    public ScrollRect LevelButtonScroll;
    public Transform LevelButtonContainer;
    public Text MedalsObtainedText;
    public int ButtonsPerRow;

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
            var sectionTitle = Instantiate(LevelSectionTitlePrefab, LevelButtonContainer);
            sectionTitle.GetComponentInChildren<Text>().text = levelSection.Title;

            var sectionTutorial = levelSection.Tutorial;
            var tutorialButton = sectionTitle.GetComponentInChildren<Button>();

            if (sectionTutorial != null)
            {
                tutorialButton.GetComponent<CanvasGroup>().alpha = 1;
                tutorialButton.onClick.AddListener(() => 
                {
                    PopupPanel.instance.ShowConfirm(sectionTutorial);
                });
            }
            else
            {
                tutorialButton.GetComponent<CanvasGroup>().alpha = 0;
                tutorialButton.onClick.RemoveAllListeners();
            }
            
            var section = Instantiate(LevelSectionPrefab, LevelButtonContainer).transform;
            var row = (Transform)null;
            //var details = (Transform)null;

            for (var i = 0; i < levelSection.Levels.Length; i++)
            {
                if (i % ButtonsPerRow == 0)
                {
                    row = Instantiate(LevelButtonRowPrefab, section).transform;
                    //details = Instantiate(LevelDetailsPrefab, section).transform;
                }

                //var actualDetails = details;
                var level = levelSection.Levels[i];

                var button = Instantiate(LevelButtonPrefab, row).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = (level.Index + 1).ToString();
                button.onClick.AddListener(() =>
                {
                    //OpenLevelDetails(actualDetails, level);
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
            var medalsNeededToUnlock = Mathf.CeilToInt(i * MainManager.Instance.LevelUnlockMultiplier);
            var unlocked = MainManager.Instance.Medals >= medalsNeededToUnlock;

            _levelButtons[i].interactable = unlocked;
            _levelButtons[i].transform.GetChild(1).gameObject.SetActive(unlocked);
            _levelButtons[i].transform.GetChild(2).gameObject.SetActive(!unlocked);

            if (_levelButtons[i].interactable)
            {
                var medals = _levelButtons[i].transform.GetChild(1).GetComponentsInChildren<Image>();
                var savedMedals = Level.GetSavedValue(i);

                for (var j = 0; j < medals.Length; j++)
                {
                    medals[j].color = !string.IsNullOrEmpty(savedMedals) && savedMedals[j] == '1' ? Color.white : new Color(1, 1, 1, 0.25f);
                }
            }
            else
            {
                _levelButtons[i].transform.GetChild(2).GetComponentInChildren<Text>().text = medalsNeededToUnlock.ToString();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_levelButtons[i].GetComponent<RectTransform>());
        }

        MedalsObtainedText.text = MainManager.Instance.Medals.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(MedalsObtainedText.transform.parent.GetComponent<RectTransform>());
    }

    /*private void OpenLevelDetails(Transform details, EditableLevel level)
    {
        details.GetComponent<RectTransform>().sizeDelta = new Vector2(details.GetComponent<RectTransform>().sizeDelta.x, 100);
    }*/

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
