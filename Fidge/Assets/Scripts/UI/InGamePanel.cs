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
    public Image TimerImage;
    //public Text TimerText;
    public Image MovesImage;
    //public Text MovesText;
    public Button ResetButton;
    public Button UpButton;
    public Button RightButton;
    public Button DownButton;
    public Button LeftButton;
    public Button GoButton;
    public RectTransform Menu;
    public Button MenuButton;
    public Image MenuButtonImage;
    public Sprite OpenSprite;
    public Sprite CloseSprite;
    public Image Blocker;
    public CanvasGroup CinematicBars;

    private bool _initialized;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        Button[] buttons = {UpButton, RightButton, DownButton, LeftButton, GoButton};

        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    void Update()
    {
        if (MainManager.Instance.ActiveLevel != null && !_initialized)
        {
            var scripted = MainManager.Instance.ActiveLevel.Scripted;
            
            if (Application.isPlaying)
            {
                UpdateCollectables(null);
                Blocker.raycastTarget = scripted;
                ObjectiveBars.alpha = !scripted ? 1 : 0;
                CinematicBars.alpha = scripted ? 1 : 0;
                ResetButton.gameObject.SetActive(!scripted);
            }

            _initialized = true;
        }
    }

    public override void Show()
    {
        UI_CloseMenu();
        _initialized = false;

        base.Show();
    }

    public void UpdateTimer(float value = 0f)
    {
        var remainingValue = MainManager.Instance.ActiveLevel.ExpectedTime - value;

        if (remainingValue < 0)
        {
            return;
        }

        var normalizedValue = remainingValue / MainManager.Instance.ActiveLevel.ExpectedTime;
        
        TimerImage.fillAmount = normalizedValue;
        //TimerText.text = Mathf.RoundToInt(remainingValue).ToString();
    }

    public void UpdateMoves(int value = 0)
    {
        var remainingValue = MainManager.Instance.ActiveLevel.ExpectedMoves - value;

        if (remainingValue < 0)
        {
            return;
        }

        var normalizedValue = (float)remainingValue / MainManager.Instance.ActiveLevel.ExpectedMoves;
        
        MovesImage.fillAmount = normalizedValue;
        //MovesText.text = remainingValue.ToString();
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

    public void UI_OpenMenu()
    {
        MenuButton.onClick.RemoveAllListeners();
        MenuButton.onClick.AddListener(UI_CloseMenu);

        Menu.anchoredPosition = new Vector2(0, 0);
        MenuButtonImage.sprite = CloseSprite;
    }

    public void UI_CloseMenu()
    {
        MenuButton.onClick.RemoveAllListeners();
        MenuButton.onClick.AddListener(UI_OpenMenu);

        Menu.anchoredPosition = new Vector2(0, 100);
        MenuButtonImage.sprite = OpenSprite;
    }
    
    public void UI_Back()
    {
        TraversalManager.Instance.CancelTraversal();

        Destroy(MainManager.Instance.ActiveLevel.gameObject);
        LevelSelectionPanel.instance.Show();
    }

    public void UI_Reset()
    {
        TraversalManager.Instance.CancelTraversal();
        MainManager.Instance.ReloadLevel();
    }

    public void UI_Go()
    {
        TraversalManager.Instance.ConfirmTraversal();
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
    }
}
