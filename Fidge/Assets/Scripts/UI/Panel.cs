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
        
        foreach (var panel in UIManager.Instance.Panels)
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
            if (UIManager.Instance.InGamePanel != null && button.transform.parent == UIManager.Instance.InGamePanel.TraversalInput.transform)
            {
                if(button == UIManager.Instance.InGamePanel.GoButton)
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
