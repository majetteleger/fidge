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
        Element,
        Content,
        TraversalModifier
    }

    public static LevelEditPanel Instance;

    public Button ToggleElementButton;
    public Button ToggleContentButton;
    public Button ToggleTraversalModifierButton;
    public GameObject MessageBubble;
    public GameObject MessageBubbleBackground;
    public Text BackMessageText;
    [TextArea] public string BackMessage;
    public Sprite NodeSprite;
    public Sprite VerticalPathSprite;
    public Sprite HorizontalPathSprite;

    private UserClickType _clickType;
    private LevelCell[] _levelCells;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupSounds();
        _levelCells = GetComponentsInChildren<LevelCell>();

        for (var i = 0; i < _levelCells.Length; i++)
        {
            var newPosition = new Vector2Int(i % KWidth, i / KWidth);
            _levelCells[i].Position = newPosition;
        }
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
        ToggleElement();
        base.Show(originPanel);
    }

    private void ToggleElement()
    {
        _clickType = UserClickType.Element;
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

    public void UI_ClickOnCell()
    {
        var cellClicked = EventSystem.current.currentSelectedGameObject.GetComponent<LevelCell>();

        switch (_clickType)
        {
            case UserClickType.Element:

                if (!string.IsNullOrEmpty(cellClicked.Content))
                {
                    if (!cellClicked.PositionIsOdd)
                    {
                        cellClicked.ChangePathOrientation();
                        cellClicked.ChangeSprite(cellClicked.Content.Contains(EditableLevel.KVertical) ? VerticalPathSprite : HorizontalPathSprite);
                    }

                    break;
                }
                
                if (cellClicked.PositionIsOdd)
                {
                    cellClicked.ChangeSprite(NodeSprite);
                    cellClicked.Content = EditableLevel.KNode;
                }
                else
                {
                    cellClicked.Content = EditableLevel.KPath;

                    var upCell = GetCellByPosition(cellClicked.Position.x, cellClicked.Position.y + 1);
                    var downCell = GetCellByPosition(cellClicked.Position.x, cellClicked.Position.y - 1);
                    var upIsNode = upCell != null && !string.IsNullOrEmpty(upCell.Content) && upCell.Content.Contains(EditableLevel.KNode);
                    var downIsNode = downCell != null && !string.IsNullOrEmpty(downCell.Content) && downCell.Content.Contains(EditableLevel.KNode);

                    if (upIsNode || downIsNode)
                    {
                        cellClicked.ChangeSprite(VerticalPathSprite);
                        cellClicked.Content += EditableLevel.KVertical;
                    }
                    else
                    {
                        cellClicked.ChangeSprite(HorizontalPathSprite);
                        cellClicked.Content += EditableLevel.KHorizontal;
                    }
                    
                }
                
                break;
            case UserClickType.Content:
                break;
            case UserClickType.TraversalModifier:
                break;
        }
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
