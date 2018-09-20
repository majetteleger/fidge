using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCell : MonoBehaviour
{
    public Image BaseImage;
    public Image ContentImage;
    public Image TraversalModifierImage;

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
            case LevelEditPanel.UserClickType.Content:
                imageToChange = ContentImage;
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
}
