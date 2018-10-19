using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance;

    public MainMenuPanel MainMenuPanel;
    public OptionsPanel OptionsPanel;
    public InGamePanel InGamePanel;
    public LevelSelectionPanel LevelSelectionPanel;
    public LevelEditorMenuPanel LevelEditorMenuPanel;
    public LevelEditPanel LevelEditPanel;
    public UserLevelsPanel UserLevelsPanel;
    public PopupPanel PopupPanel;
    public CreditsPanel CreditsPanel;
    public TutorialPanel TutorialPanel;
    public PurchasePanel PurchasePanel;

    private Panel[] _panels;

    public Panel[] Panels
	{
		get
		{
			if(_panels == null)
			{
				_panels = GetComponentsInChildren<Panel>();
			}

			return _panels;
		}
	}

	void Awake()
	{
	    Instance = this;
    }
}
