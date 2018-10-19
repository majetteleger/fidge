﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionPanel : Panel
{
    public GameObject LevelButtonPrefab;
    public GameObject LevelButtonRowPrefab;
    public GameObject LevelSectionPrefab;
    public GameObject LevelSectionTitlePrefab;
    public ScrollRect LevelButtonScroll;
    public Transform LevelButtonContainer;
    public Text MedalsObtainedText;
    public int ButtonsPerRow;
    public int FreeRows;

    private Button[] _levelButtons;
    private Button[] _payButtons;
    private CanvasGroup _canvasGroup;

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

    public void Initialize()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        StartCoroutine(DoInitialize());
    }
    
    public override void Show(Panel originPanel = null)
    {
        AudioManager.Instance.PlayBackgroundMusic(AudioManager.Instance.MenuMusic);
        
        UpdateLevelButtons();

        base.Show(originPanel);

        if (originPanel != UIManager.Instance.PopupPanel && originPanel != UIManager.Instance.InGamePanel)
        {
            LevelButtonScroll.verticalNormalizedPosition = 1;
        }
    }

    public void UpdateSectionBlockers()
    {
        var levelSections = FindObjectsOfType<LevelSection>();

        foreach (var section in levelSections)
        {
            section.Gap.gameObject.SetActive(!MainManager.Instance.Paid);
            section.Blocker.gameObject.SetActive(!MainManager.Instance.Paid);
        }
    }

    private IEnumerator DoInitialize()
    {
        _levelButtons = new Button[MainManager.Instance.Levels.Length];

        var tempLevelIndex = 0;

        for (var i = 0; i < MainManager.Instance.Sections.Length; i++)
        {
            var levelSection = MainManager.Instance.Sections[i];
            var sectionTitle = Instantiate(LevelSectionTitlePrefab, LevelButtonContainer);
            sectionTitle.GetComponentInChildren<Text>().text = levelSection.Title;

            var sectionTutorial = levelSection.Tutorial;
            var tutorialButton = sectionTitle.GetComponentInChildren<Button>();

            if (sectionTutorial != null)
            {
                tutorialButton.GetComponentsInParent<CanvasGroup>(true)[0].alpha = 1;
                tutorialButton.onClick.AddListener(() => { UIManager.Instance.PopupPanel.ShowConfirm(sectionTutorial); });
            }
            else
            {
                tutorialButton.GetComponentsInParent<CanvasGroup>(true)[0].alpha = 0;
                tutorialButton.onClick.RemoveAllListeners();
            }

            var section = Instantiate(LevelSectionPrefab, LevelButtonContainer).GetComponent<LevelSection>();
            var numberOfRows = 0;
            var rowHeight = 0f;
            var row = (Transform)null;

            for (var j = 0; j < levelSection.Levels.Length; j++)
            {
                if (j % ButtonsPerRow == 0)
                {
                    row = Instantiate(LevelButtonRowPrefab, section.transform).transform;
                    rowHeight = row.GetComponent<RectTransform>().sizeDelta.y;
                    numberOfRows++;
                }

                var level = levelSection.Levels[j];

                var button = Instantiate(LevelButtonPrefab, row).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = (level.Index + 1).ToString();
                button.onClick.AddListener(() => { UIManager.Instance.PopupPanel.ShowConfirm(level); });

                _levelButtons[tempLevelIndex] = button;

                tempLevelIndex++;
            }

            section.Blocker.SetAsLastSibling();

            var sectionSpacing = section.GetComponent<VerticalLayoutGroup>().spacing;
            var oldBlockerSize = section.Blocker.sizeDelta;
            var gapIndex = i == 0 ? FreeRows : 0;

            section.Gap.transform.SetSiblingIndex(gapIndex);

            var newBlockHeight = (numberOfRows - gapIndex) * rowHeight;
            newBlockHeight += (numberOfRows - (gapIndex + 1)) * sectionSpacing;
            newBlockHeight += (numberOfRows - gapIndex) * 24;
            newBlockHeight += (3 - (numberOfRows - gapIndex)) * 16;

            section.Blocker.sizeDelta = new Vector2(oldBlockerSize.x, newBlockHeight);
        }

        yield return new WaitForEndOfFrame();

        ForceLayoutRebuilding(LevelButtonContainer.GetComponent<RectTransform>());

        gameObject.SetActive(false);
        _canvasGroup.alpha = 1;
    }

    private void UpdateLevelButtons()
    {
        for (var i = 0; i < _levelButtons.Length; i++)
        {
            var unlocked = MainManager.Instance.Levels[i].Level.Unlocked;

            _levelButtons[i].interactable = unlocked;
            _levelButtons[i].transform.GetChild(1).gameObject.SetActive(unlocked);
            _levelButtons[i].transform.GetChild(2).gameObject.SetActive(!unlocked);
            _levelButtons[i].transform.GetChild(3).gameObject.SetActive(false);

            if (_levelButtons[i].interactable)
            {
                var medals = _levelButtons[i].transform.GetChild(1).GetComponentsInChildren<Image>();
                var savedMedals = Level.GetSavedValue(i);

                for (var j = 0; j < medals.Length; j++)
                {
                    medals[j].color = !string.IsNullOrEmpty(savedMedals) && savedMedals[j] == '1' ? Color.black : new Color(0, 0, 0, 0.25f);
                }
            }
            else
            {
                _levelButtons[i].transform.GetChild(2).GetComponentInChildren<Text>().text = MainManager.Instance.Levels[i].Level.MedalsNeededToUnlock.ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_levelButtons[i].GetComponent<RectTransform>());
            }
        }

        UpdateSectionBlockers();

        MedalsObtainedText.text = MainManager.Instance.Medals.ToString();
    }
    
    public void UI_Back()
    {
        UIManager.Instance.MainMenuPanel.Show();
    }
}
