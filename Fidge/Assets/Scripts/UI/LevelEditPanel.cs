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
        Content,
        TraversalModifier
    }

    public static LevelEditPanel Instance;

    public GameObject LevelCellPrefab;
    public Transform LevelCellContainer;
    public Button ToggleElementButton;
    public Button ToggleContentButton;
    public Button ToggleTraversalModifierButton;
    public RectTransform NodeContextMenu;
    public RectTransform NodeContextMenuButtonGroup;
    public GameObject MessageBubble;
    public GameObject MessageBubbleBackground;
    public Text BackMessageText;
    public float LongPressTime;
    [TextArea] public string BackMessage;
    public Sprite NodeSprite;
    public Sprite StartNodeSprite;
    public Sprite EndNodeSprite;
    public Sprite VerticalPathSprite;
    public Sprite HorizontalPathSprite;

    private UserClickType _clickType;
    private LevelCell[] _levelCells;
    private bool _cellPressed;
    private float _longPressTimer;
    private LevelCell _cellClicked;
    private Vector2Int? _startNodePosition;
    private Vector2Int? _endNodePosition;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupSounds();

        _longPressTimer = LongPressTime;

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

    private void ToggleElement()
    {
        _clickType = UserClickType.Base;
        ToggleElementButton.interactable = false;
        ToggleContentButton.interactable = true;
        ToggleTraversalModifierButton.interactable = true;
    }

    private void SaveLevel()
    {
        // save to a json
    }

    private bool Validate()
    {
        return true;
    }

    private LevelCell GetCellByPosition(int x, int y)
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

    private void ClickOnCell()
    {
        if (_longPressTimer <= 0)
        {
            return;
        }
        
        switch (_clickType)
        {
            case UserClickType.Base:

                if (!string.IsNullOrEmpty(_cellClicked.Content))
                {
                    if (!_cellClicked.PositionIsOdd)
                    {
                        _cellClicked.ChangePathOrientation();
                        _cellClicked.ChangeSprite(
                            _cellClicked.Content.Contains(EditableLevel.KVertical)
                                ? VerticalPathSprite
                                : HorizontalPathSprite, UserClickType.Base);
                    }

                    break;
                }

                if (_cellClicked.PositionIsOdd)
                {
                    _cellClicked.ChangeSprite(NodeSprite, UserClickType.Base);
                    _cellClicked.Content = EditableLevel.KNode;
                }
                else
                {
                    _cellClicked.Content = EditableLevel.KPath;

                    var upCell = GetCellByPosition(_cellClicked.Position.x, _cellClicked.Position.y + 1);
                    var downCell = GetCellByPosition(_cellClicked.Position.x, _cellClicked.Position.y - 1);
                    var upIsNode = upCell != null && !string.IsNullOrEmpty(upCell.Content) &&
                                   upCell.Content.Contains(EditableLevel.KNode);
                    var downIsNode = downCell != null && !string.IsNullOrEmpty(downCell.Content) &&
                                     downCell.Content.Contains(EditableLevel.KNode);

                    if (upIsNode || downIsNode)
                    {
                        _cellClicked.ChangeSprite(VerticalPathSprite, UserClickType.Base);
                        _cellClicked.Content += EditableLevel.KVertical;
                    }
                    else
                    {
                        _cellClicked.ChangeSprite(HorizontalPathSprite, UserClickType.Base);
                        _cellClicked.Content += EditableLevel.KHorizontal;
                    }

                }

                break;
            case UserClickType.Content:
                break;
            case UserClickType.TraversalModifier:
                break;
        }
    }

    private void OpenContextMenu(UserClickType clickType)
    {
        if (_cellClicked == null || string.IsNullOrEmpty(_cellClicked.Content))
        {
            return;
        }

        var top = Camera.main.ScreenToViewportPoint(_cellClicked.transform.position).y < 0.5f;
        var right = Camera.main.ScreenToViewportPoint(_cellClicked.transform.position).x < 0.5f;

        if (_cellClicked.Content.Contains(EditableLevel.KNode))
        {
            NodeContextMenu.gameObject.SetActive(true);
            NodeContextMenuButtonGroup.anchoredPosition = _cellClicked.transform.localPosition;
            NodeContextMenuButtonGroup.pivot = new Vector2(right ? 0f : 1f, top ? 0.5f : 1.5f);
        }
    }
    
    public void UI_CloseContextMenu()
    {
        NodeContextMenu.gameObject.SetActive(false);
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

    public void UI_ToggleContent()
    {
        _clickType = UserClickType.Content;
        ToggleElementButton.interactable = true;
        ToggleContentButton.interactable = false;
        ToggleTraversalModifierButton.interactable = true;
    }

    public void UI_ToggleTraversalModifier()
    {
        _clickType = UserClickType.TraversalModifier;
        ToggleElementButton.interactable = true;
        ToggleContentButton.interactable = true;
        ToggleTraversalModifierButton.interactable = false;
    }

    public void UI_OtherOptions()
    {
        // shift up/right/down/left
        // ...
    }
}
