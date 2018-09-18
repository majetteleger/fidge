using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGamePanel : Panel
{
    public static InGamePanel instance = null;

    public Transform CollectablesContainer;
    public GameObject CollectablePrefab;
    public CanvasGroup ObjectiveBars;
    public CanvasGroup TraversalInput;
    public CanvasGroup TimerIcon;
    public CanvasGroup MovesIcon;
    public RectTransform Timer;
    public RectTransform Moves;
    public GameObject MoreMenu;
    public Button UpButton;
    public Button RightButton;
    public Button DownButton;
    public Button LeftButton;
    public Button GoButton;
    public Image MusicImage;
    public Image SoundImage;
    public Sprite MusicOnSprite;
    public Sprite MusicOffSprite;
    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;

    private float _maxObjectiveWidth;
    private float _minObjectiveWidth;
    private Button[] _traversalInputs;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _maxObjectiveWidth = Timer.sizeDelta.x;
        _minObjectiveWidth = Timer.sizeDelta.y;

        _traversalInputs = new[]
        {
            UpButton,
            RightButton,
            DownButton,
            LeftButton,
            GoButton
        };
    }

    void Start()
    {
        Button[] buttons = {UpButton, RightButton, DownButton, LeftButton, GoButton};

        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }

        SetupSounds();
    }

    public void ShowLevel(EditableLevel level)
    {
        if (Application.isPlaying)
        {
            UpdateCollectables(null);
            ToggleTraversalInputs(true);

            if (!level.Scripted)
            {
                ObjectiveBars.gameObject.SetActive(true);
                Timer.gameObject.SetActive(true);
                Moves.gameObject.SetActive(true);
                TimerIcon.alpha = 1;
                MovesIcon.alpha = 1;
                TimerIcon.GetComponentInParent<Shadow>().enabled = true;
                MovesIcon.GetComponentInParent<Shadow>().enabled = true;
            }
            else
            {
                ObjectiveBars.gameObject.SetActive(false);
            }
        }

        UI_Less();

        Show();
    }

    public override void Show(Panel originPanel = null)
    {
        UpdateButtonSprite(MusicImage, MusicOnSprite, MusicOffSprite, AudioManager.Instance.MusicOn);
        UpdateButtonSprite(SoundImage, SoundOnSprite, SoundOffSprite, AudioManager.Instance.SoundOn);

        AudioManager.Instance.PlayBackgroundMusic(AudioManager.Instance.LevelMusic);

        base.Show(originPanel);
    }

    public override void Hide()
    {
        TraversalManager.Instance.CancelTraversal();

        base.Hide();
    }
    
    public void UpdateTimer(float value = 0f)
    {
        var remainingValue = MainManager.Instance.ActiveLevel.ExpectedTime - value;

        if (remainingValue <= 0)
        {
            Timer.gameObject.SetActive(false);
            TimerIcon.alpha = 0.4f;
            TimerIcon.GetComponentInParent<Shadow>().enabled = false;
            return;
        }

        var normalizedValue = remainingValue / MainManager.Instance.ActiveLevel.ExpectedTime * (_maxObjectiveWidth - _minObjectiveWidth);
        
        Timer.sizeDelta = new Vector2(normalizedValue + _minObjectiveWidth, Timer.sizeDelta.y);
    }

    public void UpdateMoves(int value = 0)
    {
        var remainingValue = MainManager.Instance.ActiveLevel.ExpectedMoves - value;

        if (remainingValue <= 0)
        {
            Moves.gameObject.SetActive(false);
            MovesIcon.alpha = 0.4f;
            MovesIcon.GetComponentInParent<Shadow>().enabled = false;
            return;
        }

        var normalizedValue = (float)remainingValue / MainManager.Instance.ActiveLevel.ExpectedMoves * (_maxObjectiveWidth - _minObjectiveWidth);
        
        Moves.sizeDelta = new Vector2(normalizedValue + _minObjectiveWidth, Timer.sizeDelta.y);
    }

    public void UpdateCollectables(Collectable[] collectables)
    {
        foreach (Transform child in CollectablesContainer)
        {
            Destroy(child.gameObject);
        }

        if (collectables == null || collectables.Length == 0)
        {
            CollectablesContainer.GetComponent<CanvasGroup>().alpha = 0;
            return;
        }

        CollectablesContainer.GetComponent<CanvasGroup>().alpha = 1;
        
        for (var i = 0; i < collectables.Length; i++)
        {
            var newCollectableImage = Instantiate(CollectablePrefab, CollectablesContainer).GetComponent<Image>();
            newCollectableImage.sprite = collectables[i].GetComponent<SpriteRenderer>().sprite;
        }
    }

    public void ToggleTraversalInputs(bool toggle)
    {
        foreach (var traversalInput in _traversalInputs)
        {
            traversalInput.interactable = toggle;
        }
    }

    public void UI_More()
    {
        MoreMenu.SetActive(true);
    }

    public void UI_Less()
    {
        MoreMenu.SetActive(false);
    }

    public void UI_Back()
    {
        TraversalManager.Instance.CancelTraversal();

        Destroy(MainManager.Instance.ActiveLevel.gameObject);
        LevelSelectionPanel.Instance.Show();
    }

    public void UI_Reset()
    {
        TraversalManager.Instance.CancelTraversal();
        MainManager.Instance.ReloadLevel();
        UI_Less();
    }

    public void UI_ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
        UpdateButtonSprite(MusicImage, MusicOnSprite, MusicOffSprite, AudioManager.Instance.MusicOn);
    }

    public void UI_ToggleSound()
    {
        AudioManager.Instance.ToggleSound();
        UpdateButtonSprite(SoundImage, SoundOnSprite, SoundOffSprite, AudioManager.Instance.SoundOn);
    }

    public void UI_Go()
    {
        TraversalManager.Instance.ConfirmTraversal();
        UI_Less();
    }

    public void UI_RegisterTraversalMove(string traversalMoveInput)
    {
        TraversalManager.TraversalMove traversalMove = TraversalManager.TraversalMove.NONE;

        switch (traversalMoveInput)
        {
            case "Up":
                traversalMove = TraversalManager.TraversalMove.Up;
                break;
            case "Right":
                traversalMove = TraversalManager.TraversalMove.Right;
                break;
            case "Down":
                traversalMove = TraversalManager.TraversalMove.Down;
                break;
            case "Left":
                traversalMove = TraversalManager.TraversalMove.Left;
                break;
        }

        TraversalManager.Instance.RegisterTraversalMove(traversalMove);

        UI_Less();
    }
}
