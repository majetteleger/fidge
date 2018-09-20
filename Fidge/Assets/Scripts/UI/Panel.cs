using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    public List<Panel> CoexistsWith;
    
	public virtual void Show(Panel originPanel = null)
	{
		gameObject.SetActive(true);
        
        foreach (var panel in UIManager.instance.Panels)
        {
            if(panel == this || CoexistsWith.Contains(panel))
            {
                continue;
            }

            panel.Hide();
        }
	}

	public virtual void Hide()
	{
	    gameObject.SetActive(false);
    }

    public virtual void SetupSounds()
    {
        var buttons = GetComponentsInChildren<Button>(true);

        foreach (var button in buttons)
        {
            if (InGamePanel.instance != null && button.transform.parent == InGamePanel.instance.TraversalInput.transform)
            {
                if(button == InGamePanel.instance.GoButton)
                {
                    button.onClick.AddListener(() => { AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.InputGo); });
                }
                else
                {
                    button.onClick.AddListener(() => { AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.InputDirection); });
                }
            }
            else
            {
                button.onClick.AddListener(() => { AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.MenuClick); });
            }
            
        }
    }

    public void UpdateButtonSprite(Image image, Sprite onSprite, Sprite offSprite, bool value)
    {
        image.sprite = value ? onSprite : offSprite;
    }

    public void ForceLayoutRebuilding(RectTransform transformToRebuild)
    {
        var layoutGroup = transformToRebuild.GetComponent<VerticalLayoutGroup>();

        LayoutRebuilder.ForceRebuildLayoutImmediate(transformToRebuild);
        LayoutRebuilder.MarkLayoutForRebuild(transformToRebuild);
        Canvas.ForceUpdateCanvases();

        layoutGroup.CalculateLayoutInputVertical();
        layoutGroup.SetLayoutVertical();
    }
}
