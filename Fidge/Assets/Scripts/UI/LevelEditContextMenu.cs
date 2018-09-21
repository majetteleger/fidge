using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LevelEditContextMenu : MonoBehaviour
{
    public RectTransform ButtonGroup;

    public GameObject CancelButton;
    public GameObject MakeStartNodeButton;
    public GameObject MakeEndNodeButton;
    public GameObject ChangePathOrientationButton;

    private GameObject[] _buttons;

    public void Activate(LevelCell cellClicked)
    {
        _buttons = GetComponentsInChildren<Button>(true).Where(x => x.gameObject != gameObject).Select(x => x.gameObject).ToArray();

        var top = Camera.main.ScreenToViewportPoint(cellClicked.transform.position).y < 0.5f;
        var right = Camera.main.ScreenToViewportPoint(cellClicked.transform.position).x < 0.5f;

        gameObject.SetActive(true);
        ButtonGroup.anchoredPosition = cellClicked.transform.localPosition;
        ButtonGroup.pivot = new Vector2(right ? 0f : 1f, top ? 0.5f : 1.5f);
        
        if (cellClicked.Content.Contains(EditableLevel.KNode))
        {
            foreach (var button in _buttons)
            {
                var buttonActive = 
                    button == MakeStartNodeButton ||
                    button == MakeEndNodeButton ||
                    button == CancelButton;

                button.SetActive(buttonActive);
            }
        }
        else if (cellClicked.Content.Contains(EditableLevel.KPath))
        {
            foreach (var button in _buttons)
            {
                var buttonActive =
                    button == ChangePathOrientationButton ||
                    button == CancelButton;

                button.SetActive(buttonActive);
            }
        }
    }

    public void UI_Close()
    {
        gameObject.SetActive(false);
    }
}
