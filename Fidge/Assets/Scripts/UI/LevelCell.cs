using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelCell : MonoBehaviour
{
    public Image BaseImage;
    public Image ExtraImage;
    public Image TraversalModifierImage;

    [Header("Sprites")]
    public Sprite NodeSprite;
    public Sprite VerticalPathSprite;
    public Sprite HorizontalPathSprite;
    public Sprite[] KeySprites;
    public Sprite[] LinkSprites;
    public Sprite[] LockSprites;
    public Sprite[] SlideSprites;
    public Sprite CrackSprite;
    public Sprite WallSprite;
    public Sprite FlagSprite;
    public Sprite CoveredSprite;
    public Sprite RevealedSprite;

    public Vector2Int Position { get; set; }
    public string Content { get; set; }

    public bool PositionIsOdd { get { return (Position.x + Position.y) % 2 == 0; } }
    
    public void ChangePathOrientation()
    {
        var oldContent = Content;
        var newContent = new char[oldContent.Length];

        for (var i = 0; i < oldContent.Length; i++)
        {
            if (oldContent[i] == 'V')
            {
                newContent[i] = 'H';
            }
            else if (oldContent[i] == 'H')
            {
                newContent[i] = 'V';
            }
            else
            {
                newContent[i] = oldContent[i];
            }
        }

        Content = new string(newContent);
    }

    public void ChangeSprite(Sprite sprite, LevelEditPanel.UserClickType clickType)
    {
        var imageToChange = (Image) null;

        switch (clickType)
        {
            case LevelEditPanel.UserClickType.Base:
                imageToChange = BaseImage;
                break;
            case LevelEditPanel.UserClickType.Extra:
                imageToChange = ExtraImage;
                break;
            case LevelEditPanel.UserClickType.TraversalModifier:
                imageToChange = TraversalModifierImage;
                break;
        }

        if (imageToChange == null)
        {
            return;
        }

        imageToChange.color = Color.white;
        imageToChange.sprite = sprite;
    }

    public void ChangeBase()
    {
        if (PositionIsOdd)
        {
            if (!string.IsNullOrEmpty(Content) && Content.Contains(EditableLevel.KNode))
            {
                return;
            }

            ChangeSprite(NodeSprite, LevelEditPanel.UserClickType.Base);
            Content = EditableLevel.KNode;
        }
        else
        {
            if (!string.IsNullOrEmpty(Content) && Content.Contains(EditableLevel.KPath))
            {
                ChangePathOrientation();
                ChangeSprite(
                    Content.Contains(EditableLevel.KVertical)
                        ? VerticalPathSprite
                        : HorizontalPathSprite, LevelEditPanel.UserClickType.Base);

                return;
            }

            Content = EditableLevel.KPath;

            var upCell = LevelEditPanel.Instance.GetCellByPosition(Position.x, Position.y + 1);
            var downCell = LevelEditPanel.Instance.GetCellByPosition(Position.x, Position.y - 1);
            var upIsNode = upCell != null && !string.IsNullOrEmpty(upCell.Content) &&
                           upCell.Content.Contains(EditableLevel.KNode);
            var downIsNode = downCell != null && !string.IsNullOrEmpty(downCell.Content) &&
                             downCell.Content.Contains(EditableLevel.KNode);

            if (upIsNode || downIsNode)
            {
                ChangeSprite(VerticalPathSprite, LevelEditPanel.UserClickType.Base);
                Content += EditableLevel.KVertical;
            }
            else
            {
                ChangeSprite(HorizontalPathSprite, LevelEditPanel.UserClickType.Base);
                Content += EditableLevel.KHorizontal;
            }

        }
    }

    public void ChangeExtra(string extra)
    {
        if (EditableLevel.Collectables.Contains(extra) && !Content.Contains(EditableLevel.KNode))
        {
            Debug.Log("Can only place this extra on a node");
            return;   
        }

        if (EditableLevel.Obstacles.Contains(extra) && !Content.Contains(EditableLevel.KPath))
        {
            Debug.Log("Can only place this extra on a path");
            return;
        }

        switch (extra)
        {
            case EditableLevel.KKey:

                if (Content.Contains(EditableLevel.KKey))
                {
                    CycleThrough(EditableLevel.Colors, KeySprites);
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KKey;
                Content += EditableLevel.KColorRed;

                ChangeSprite(KeySprites[0], LevelEditPanel.UserClickType.Extra);

                break;

            case EditableLevel.KFlag:

                if (Content.Contains(EditableLevel.KFlag))
                {
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KFlag;

                ChangeSprite(FlagSprite, LevelEditPanel.UserClickType.Extra);

                break;

            case EditableLevel.KLink:

                if (Content.Contains(EditableLevel.KLink))
                {
                    CycleThrough(EditableLevel.Colors, LinkSprites);
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KLink;
                Content += EditableLevel.KColorRed;

                ChangeSprite(LinkSprites[0], LevelEditPanel.UserClickType.Extra);

                break;

            case EditableLevel.KLock:

                if (Content.Contains(EditableLevel.KLock))
                {
                    CycleThrough(EditableLevel.Colors, LockSprites);
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KLock;
                Content += EditableLevel.KColorRed;

                ChangeSprite(LockSprites[0], LevelEditPanel.UserClickType.Extra);

                break;

            case EditableLevel.KWall:

                if (Content.Contains(EditableLevel.KWall))
                {
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KWall;

                ChangeSprite(WallSprite, LevelEditPanel.UserClickType.Extra);

                break;

            case EditableLevel.KCrack:

                if (Content.Contains(EditableLevel.KCrack))
                {
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KCrack;

                ChangeSprite(CrackSprite, LevelEditPanel.UserClickType.Extra);

                break;

            case EditableLevel.KSlide:

                if (Content.Contains(EditableLevel.KSlide))
                {
                    CycleThrough(EditableLevel.Directions, SlideSprites);
                    break;
                }

                RemoveAllExtra();
                Content += EditableLevel.KSlide;
                Content += EditableLevel.KDirectionRight;

                ChangeSprite(SlideSprites[0], LevelEditPanel.UserClickType.Extra);

                break;
        }
    }

    public void ChangeTraversalModifier(string traversalModifier)
    {
        switch (traversalModifier)
        {
            case EditableLevel.KTraversalStateCovered:

                if (!string.IsNullOrEmpty(Content) && Content.Contains(EditableLevel.KTraversalStateCovered))
                {
                    break;
                }

                RemoveAllTraversalModifier();
                Content += EditableLevel.KTraversalStateCovered;

                ChangeSprite(CoveredSprite, LevelEditPanel.UserClickType.TraversalModifier);

                break;

            case EditableLevel.KTraversalStateRevealed:

                if (!string.IsNullOrEmpty(Content) && Content.Contains(EditableLevel.KTraversalStateRevealed))
                {
                    break;
                }

                RemoveAllTraversalModifier();
                Content += EditableLevel.KTraversalStateRevealed;

                ChangeSprite(RevealedSprite, LevelEditPanel.UserClickType.TraversalModifier);

                break;
        }
    }

    private void CycleThrough(string[] thingsToCycleThrough, Sprite[] spritesToCycleThrough)
    {
        var newIndex = 0;
        var oldThing = string.Empty;

        for (var i = 0; i < thingsToCycleThrough.Length; i++)
        {
            if (Content.Contains(thingsToCycleThrough[i]))
            {
                newIndex = i < thingsToCycleThrough.Length - 1 ? i + 1 : 0;
                oldThing = thingsToCycleThrough[i];
            }
        }

        Content = Content.Replace(oldThing, string.Empty);
        Content += thingsToCycleThrough[newIndex];

        ChangeSprite(spritesToCycleThrough[newIndex], LevelEditPanel.UserClickType.Extra);
    }

    private void RemoveAllExtra()
    {
        RemoveFromElement(EditableLevel.Collectables);
        RemoveFromElement(EditableLevel.Obstacles);
        RemoveFromElement(EditableLevel.Colors);
        RemoveFromElement(EditableLevel.Directions);
    }

    private void RemoveAllTraversalModifier()
    {
        RemoveFromElement(EditableLevel.TraversalStates);
    }

    private void RemoveFromElement(string contentToRemove)
    {
        if (string.IsNullOrEmpty(Content))
        {
            return;
        }

        Content = Content.Replace(contentToRemove, string.Empty);
    }

    private void RemoveFromElement(string[] contentToRemove)
    {
        for (var i = 0; i < contentToRemove.Length; i++)
        {
            RemoveFromElement(contentToRemove[i]);
        }
    }
}
