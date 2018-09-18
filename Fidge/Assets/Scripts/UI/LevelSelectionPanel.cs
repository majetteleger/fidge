using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionPanel : Panel
{
    public static LevelSelectionPanel Instance;

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

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
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
                tutorialButton.GetComponentInParent<CanvasGroup>().alpha = 1;
                tutorialButton.onClick.AddListener(() => { PopupPanel.instance.ShowConfirm(sectionTutorial); });
            }
            else
            {
                tutorialButton.GetComponentInParent<CanvasGroup>().alpha = 0;
                tutorialButton.onClick.RemoveAllListeners();
            }

            var section = Instantiate(LevelSectionPrefab, LevelButtonContainer).GetComponent<LevelSection>();
            var numberOfRows = 0;
            var rowHeight = 0f;
            var row = (Transform) null;

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
                button.onClick.AddListener(() => { PopupPanel.instance.ShowConfirm(level); });

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

        SetupSounds();
    }

    void Update()
    {
        if (!IsActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenuPanel.instance.Show();
        }
    }

    public override void Show(Panel originPanel = null)
    {
        AudioManager.Instance.PlayBackgroundMusic(AudioManager.Instance.MenuMusic);

        UpdateLevelButtons();

        if (originPanel != PopupPanel.instance)
        {
            LevelButtonScroll.verticalNormalizedPosition = 1;
        }
        
        base.Show();
    }

    public void UpdateSectionBlockers()
    {
        var levelSections = FindObjectsOfType<LevelSection>();

        foreach (var section in levelSections)
        {
            section.Gap.gameObject.SetActive(!MainManager.Instance.Paid);
            section.Blocker.gameObject.SetActive(!MainManager.Instance.Paid);
        }

        LevelButtonScroll.verticalNormalizedPosition = 1;
    }

    private void UpdateLevelButtons()
    {
        for (var i = 0; i < _levelButtons.Length; i++)
        {
            var unlocked = MainManager.Instance.Levels[i].Level.Unlocked;

            _levelButtons[i].interactable = unlocked;
            _levelButtons[i].transform.GetChild(1).gameObject.SetActive(unlocked);
            _levelButtons[i].transform.GetChild(2).gameObject.SetActive(!unlocked);

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
        MainMenuPanel.instance.Show();
    }
}
