using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LevelEditContextMenu : MonoBehaviour
{
    public RectTransform ButtonGroup;

    public Transform CanvasTransform;
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

        var traversalModified = cellClicked.Content.Contains(EditableLevel.KTraversalStateCovered) || cellClicked.Content.Contains(EditableLevel.KTraversalStateRevealed);
        var containsCollectable = cellClicked.Content.Contains(EditableLevel.KKey) || cellClicked.Content.Contains(EditableLevel.KLink) || cellClicked.Content.Contains(EditableLevel.KFlag);

        var activeButtons = 0;

        if (cellClicked.Content.Contains(EditableLevel.KNode))
        {
            foreach (var button in _buttons)
            {
                var buttonActive = 
                    (button == MakeStartNodeButton && !traversalModified && !containsCollectable) ||
                    (button == MakeEndNodeButton && !traversalModified && !containsCollectable) ||
                    button == CancelButton ||
                    button == ClearButton;

                button.SetActive(buttonActive);

                if (buttonActive)
                {
                    activeButtons++;
                }
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

                if (buttonActive)
                {
                    activeButtons++;
                }
            }
        }

        var top = Camera.main.ScreenToViewportPoint(cellClicked.transform.position).y < 0.5f;
        var right = Camera.main.ScreenToViewportPoint(cellClicked.transform.position).x < 0.5f;

        gameObject.SetActive(true);
        
        var position = CanvasTransform.InverseTransformPoint(cellClicked.transform.position);
        var buttonHeight = _buttons[0].GetComponent<RectTransform>().sizeDelta.y;
        var halfHeight = ((buttonHeight + 8) * activeButtons) / 2f;
        var halfWidth = _buttons[0].GetComponent<RectTransform>().sizeDelta.x / 2f;
        
        position.y += top ? halfHeight : -halfHeight;
        position.x += right ? halfWidth : -halfWidth;

        ButtonGroup.localPosition = position;
    }

    public void Activate(Button buttonClicked)
    {
        _buttons = GetComponentsInChildren<Button>(true).Where(x => x.gameObject != gameObject).Select(x => x.gameObject).ToArray();

        var activeButtons = 0;

        if (buttonClicked == UIManager.Instance.LevelEditPanel.OtherOptionsButton)
        {
            foreach (var button in _buttons)
            {
                var buttonActive =
                    button == ResetButton ||
                    button == CenterButton ||
                    button == DeleteButton;

                button.SetActive(buttonActive);

                if (buttonActive)
                {
                    activeButtons++;
                }
            }
        }

        var top = Camera.main.ScreenToViewportPoint(buttonClicked.transform.position).y < 0.5f;
        var right = Camera.main.ScreenToViewportPoint(buttonClicked.transform.position).x < 0.5f;

        gameObject.SetActive(true);

        var position = CanvasTransform.InverseTransformPoint(buttonClicked.transform.position);
        var buttonHeight = _buttons[0].GetComponent<RectTransform>().sizeDelta.y;
        var halfHeight = ((buttonHeight + 8) * activeButtons) / 2f;
        var halfWidth = _buttons[0].GetComponent<RectTransform>().sizeDelta.x / 2f;
        
        position.y += top ? halfHeight : -halfHeight;
        position.x += right ? halfWidth : -halfWidth;

        ButtonGroup.localPosition = position;
    }

    public void UI_Close()
    {
        gameObject.SetActive(false);
    }
}
