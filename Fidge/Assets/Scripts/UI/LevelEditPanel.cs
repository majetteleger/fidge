using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEditPanel : Panel
{
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

    private UserClickType _clickType;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupSounds();
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

    public void UI_ClickOnCell()
    {
        // open the context menu OR add the element OR add the traversal modifier
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
        // close confrimation popup
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
