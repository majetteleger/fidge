using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEditPanel : Panel
{
    public const int KWidth = 7;
    public const int KHeight = 11;

    public enum UserClickType
    {
        Base,
        Extra,
        TraversalModifier,
        NONE
    }

    public static LevelEditPanel Instance;

    public GameObject LevelCellPrefab;
    public Transform LevelCellContainer;
    public Button ToggleElementButton;
    public Image ToggleElementButtonImage;
    public Button ToggleExtraButton;
    public Image ToggleExtraButtonImage;
    public Button ToggleTraversalModifierButton;
    public Image ToggleTraversalModifierButtonImage;
    public LevelEditContextMenu ContextMenu;
    public GameObject MessageBubble;
    public GameObject MessageBubbleBackground;
    public Text BackMessageText;
    public float LongPressTime;
    [TextArea] public string BackMessage;
    
    [Header("Sprites")]
    public Sprite NodeSprite;
    public Sprite StartNodeSprite;
    public Sprite EndNodeSprite;
    public Sprite VerticalPathSprite;
    public Sprite HorizontalPathSprite;
    public Sprite KeySprite;
    public Sprite LinkSprite;
    public Sprite LockSprite;
    public Sprite SlideSprite;
    public Sprite CrackSprite;
    public Sprite WallSprite;
    public Sprite FlagSprite;
    public Sprite CoveredSprite;
    public Sprite RevealedSprite;

    private UserClickType _clickType;
    private LevelCell[] _levelCells;
    private bool _cellPressed;
    private float _longPressTimer;
    private LevelCell _cellClicked;
    private Vector2Int? _startNodePosition;
    private Vector2Int? _endNodePosition;
    private string _toggledExtra;
    private string _toggledTraversalModifier;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupSounds();

        _longPressTimer = LongPressTime;
        _clickType = UserClickType.NONE;
        _toggledExtra = EditableLevel.KKey;
        _toggledTraversalModifier = EditableLevel.KTraversalStateCovered;

        var levelCellList = new List<LevelCell>();

        for (var i = 0; i < KWidth * KHeight; i++)
        {
            var newLevelCell = Instantiate(LevelCellPrefab, LevelCellContainer).GetComponent<LevelCell>();
            var newPosition = new Vector2Int(i % KWidth, i / KWidth);
            newLevelCell.Position = newPosition;

            var trigger = newLevelCell.gameObject.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((e) =>
            {
                _cellClicked = EventSystem.current.currentSelectedGameObject.GetComponent<LevelCell>();

                _cellPressed = true;
            });

            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry{ eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((e) =>
            {
                _cellClicked = EventSystem.current.currentSelectedGameObject.GetComponent<LevelCell>();

                ClickOnCell();
                _longPressTimer = LongPressTime;
                _cellPressed = false;
            });

            trigger.triggers.Add(pointerUp);
            
            levelCellList.Add(newLevelCell);
        }

        _levelCells = levelCellList.ToArray();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenuPanel.instance.Show();
        }

        if (_cellPressed && _longPressTimer > 0)
        {
            _longPressTimer -= Time.deltaTime;

            if (_longPressTimer <= 0)
            {
                OpenContextMenu(_clickType);
            }
        }
    }

    public override void Show(Panel originPanel = null)
    {
        ToggleElement();
        base.Show(originPanel);
    }

    public LevelCell GetCellByPosition(int x, int y)
    {
        var position = new Vector2Int(x, y);

        foreach (var levelCell in _levelCells)
        {
            if (levelCell.Position == position)
            {
                return levelCell;
            }
        }

        return null;
    }

    private void SaveLevel()
    {
        // save to a json
    }

    private bool Validate()
    {
        return true;
    }
    
    private void ClickOnCell()
    {
        if (_longPressTimer <= 0)
        {
            return;
        }
        
        switch (_clickType)
        {
            case UserClickType.Base:

                _cellClicked.ChangeBase();

                break;

            case UserClickType.Extra:

                _cellClicked.ChangeExtra(_toggledExtra);

                break;

            case UserClickType.TraversalModifier:

                _cellClicked.ChangeTraversalModifier(_toggledTraversalModifier);

                break;
        }
    }

    private void OpenContextMenu(UserClickType clickType)
    {
        if (_cellClicked == null || string.IsNullOrEmpty(_cellClicked.Content))
        {
            return;
        }

        ContextMenu.Activate(_cellClicked);
    }

    private void CycleThroughExtra()
    {
        var newIndex = 0;

        var extraToCycleThrough = new string[EditableLevel.Collectables.Length + EditableLevel.Obstacles.Length];
        EditableLevel.Collectables.CopyTo(extraToCycleThrough, 0);
        EditableLevel.Obstacles.CopyTo(extraToCycleThrough, EditableLevel.Collectables.Length);

        var extraSprites = new[]
        {
            KeySprite,
            FlagSprite,
            LinkSprite,
            LockSprite,
            WallSprite,
            CrackSprite,
            SlideSprite
        };

        if (!string.IsNullOrEmpty(_toggledExtra))
        {
            for (var i = 0; i < extraToCycleThrough.Length; i++)
            {
                if (extraToCycleThrough[i] == _toggledExtra)
                {
                    newIndex = i < extraToCycleThrough.Length - 1 ? i + 1 : 0;
                }
            }
        }

        _toggledExtra = extraToCycleThrough[newIndex];
        ToggleExtraButtonImage.sprite = extraSprites[newIndex];
    }

    private void CycleThroughTraversalModifier()
    {
        var newIndex = 0;

        var traversalModifierToCycleThrough = new string[EditableLevel.TraversalStates.Length];
        EditableLevel.TraversalStates.CopyTo(traversalModifierToCycleThrough, 0);

        var traversalModifierSprites = new[]
        {
            CoveredSprite,
            RevealedSprite
        };

        if (!string.IsNullOrEmpty(_toggledTraversalModifier))
        {
            for (var i = 0; i < traversalModifierToCycleThrough.Length; i++)
            {
                if (traversalModifierToCycleThrough[i] == _toggledTraversalModifier)
                {
                    newIndex = i < traversalModifierToCycleThrough.Length - 1 ? i + 1 : 0;
                }
            }
        }

        _toggledTraversalModifier = traversalModifierToCycleThrough[newIndex];
        ToggleTraversalModifierButtonImage.sprite = traversalModifierSprites[newIndex];
    }

    public void UI_ChangePathOrientation()
    {
        _cellClicked.ChangePathOrientation();
    }

    public void UI_MakeStartNode()
    {
        if (_startNodePosition.HasValue)
        {
            var previousStartNode = GetCellByPosition(_startNodePosition.Value.x, _startNodePosition.Value.y);
            if (previousStartNode != null && !string.IsNullOrEmpty(previousStartNode.Content) && previousStartNode.Content.Contains(EditableLevel.KNode))
            {
                previousStartNode.ChangeSprite(NodeSprite, UserClickType.Base);
            }

            if (_endNodePosition == _cellClicked.Position)
            {
                _endNodePosition = null;
            }
        }
        
        _startNodePosition = _cellClicked.Position;
        _cellClicked.ChangeSprite(StartNodeSprite, UserClickType.Base);
        
    }

    public void UI_MakeEndNode()
    {
        if (_endNodePosition.HasValue)
        {
            var previousEndNode = GetCellByPosition(_endNodePosition.Value.x, _endNodePosition.Value.y);
            if (previousEndNode != null && !string.IsNullOrEmpty(previousEndNode.Content) && previousEndNode.Content.Contains(EditableLevel.KNode))
            {
                previousEndNode.ChangeSprite(NodeSprite, UserClickType.Base);
            }

            if (_startNodePosition == _cellClicked.Position)
            {
                _startNodePosition = null;
            }
        }
            
        _endNodePosition = _cellClicked.Position;
        _cellClicked.ChangeSprite(EndNodeSprite, UserClickType.Base);
    }

    public void UI_Validate()
    {
        var valid = Validate();

        // open a popup containing validation info:
        // if valid, tell the player of the possible solutions
        // if invalid, tell the player why
    }

    public void UI_Back()
    {
        MessageBubble.SetActive(true);
        MessageBubbleBackground.SetActive(true);
        BackMessageText.text = BackMessage;
        ForceLayoutRebuilding(MessageBubble.GetComponent<RectTransform>());
        
        // tell the player whether the level is valid or not and ask for confirmation before leaving
    }

    public void UI_BackConfirmed()
    {
        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
        SaveLevel();
        LevelEditorMenuPanel.instance.Show();
    }

    public void UI_BackCanceled()
    {
        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
    }
    
    public void UI_ToggleElement()
    {
        ToggleElement();
    }

    private void ToggleElement()
    {
        if (_clickType == UserClickType.Base)
        {
            return;
        }

        _clickType = UserClickType.Base;
        ToggleElementButton.GetComponent<Image>().color = Color.gray;
        ToggleExtraButton.GetComponent<Image>().color = Color.white;
        ToggleTraversalModifierButton.GetComponent<Image>().color = Color.white;
    }

    public void UI_ToggleExtra()
    {
        if (_clickType == UserClickType.Extra)
        {
            CycleThroughExtra();
            return;
        }

        _clickType = UserClickType.Extra;
        ToggleElementButton.GetComponent<Image>().color = Color.white;
        ToggleExtraButton.GetComponent<Image>().color = Color.gray;
        ToggleTraversalModifierButton.GetComponent<Image>().color = Color.white;
    }

    public void UI_ToggleTraversalModifier()
    {
        if (_clickType == UserClickType.TraversalModifier)
        {
            CycleThroughTraversalModifier();
            return;
        }

        _clickType = UserClickType.TraversalModifier;
        ToggleElementButton.GetComponent<Image>().color = Color.white;
        ToggleExtraButton.GetComponent<Image>().color = Color.white;
        ToggleTraversalModifierButton.GetComponent<Image>().color = Color.gray;
    }

    public void UI_OtherOptions()
    {
        // shift up/right/down/left
        // ...
    }
}
