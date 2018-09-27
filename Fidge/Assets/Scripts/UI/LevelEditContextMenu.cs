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
    public GameObject ClearButton;
    public GameObject ResetButton;
    public GameObject CenterButton;
    public GameObject DeleteButton;

    private GameObject[] _buttons;

    public void Activate(LevelCell cellClicked)
    {
        _buttons = GetComponentsInChildren<Button>(true).Where(x => x.gameObject != gameObject).Select(x => x.gameObject).ToArray();
        
        if (cellClicked.Content.Contains(EditableLevel.KNode))
        {
            foreach (var button in _buttons)
            {
                var buttonActive = 
                    button == MakeStartNodeButton ||
                    button == MakeEndNodeButton ||
                    button == CancelButton ||
                    button == ClearButton;

                button.SetActive(buttonActive);
            }
        }
        else if (cellClicked.Content.Contains(EditableLevel.KPath))
        {
            foreach (var button in _buttons)
            {
                var buttonActive =
                    button == ChangePathOrientationButton ||
                    button == CancelButton ||
                    button == ClearButton;

                button.SetActive(buttonActive);
            }
        }

        var top = Camera.main.ScreenToViewportPoint(cellClicked.transform.position).y < 0.5f;
        var right = Camera.main.ScreenToViewportPoint(cellClicked.transform.position).x < 0.5f;

        gameObject.SetActive(true);
        ButtonGroup.anchoredPosition = cellClicked.transform.localPosition;
        ButtonGroup.pivot = new Vector2(right ? 0f : 1f, top ? 0.5f : 1.5f);

        // POSITION IS WEIRD
    }

    public void Activate(Button buttonClicked)
    {
        _buttons = GetComponentsInChildren<Button>(true).Where(x => x.gameObject != gameObject).Select(x => x.gameObject).ToArray();

        if (buttonClicked == LevelEditPanel.Instance.OtherOptionsButton)
        {
            foreach (var button in _buttons)
            {
                var buttonActive =
                    button == ResetButton ||
                    button == CenterButton ||
                    button == DeleteButton;

                button.SetActive(buttonActive);
            }
        }

        var top = Camera.main.ScreenToViewportPoint(buttonClicked.transform.position).y < 0.5f;
        var right = Camera.main.ScreenToViewportPoint(buttonClicked.transform.position).x < 0.5f;

        gameObject.SetActive(true);
        ButtonGroup.anchoredPosition = buttonClicked.transform.localPosition;
        ButtonGroup.pivot = new Vector2(right ? 0f : 1f, top ? 0.5f : 1.5f);
    }

    public void UI_Close()
    {
        gameObject.SetActive(false);
    }
}
