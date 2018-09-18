using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCell : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public Image Image { get; set; }
    public string Content { get; set; }

    public bool PositionIsOdd { get { return (Position.x + Position.y) % 2 == 0; } }
    
    private void Start()
    {
        Image = GetComponent<Image>();
    }

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

    public void ChangeSprite(Sprite sprite)
    {
        Image.sprite = sprite;
    }
}
