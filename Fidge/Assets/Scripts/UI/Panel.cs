using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
	private CanvasGroup canvasGroup;
	private CanvasGroup _canvasGroup
	{
		get
		{
			if(canvasGroup == null)
			{
				canvasGroup = GetComponent<CanvasGroup>();
			}

			return canvasGroup;
		}
	}

	public virtual void Show()
	{
		_canvasGroup.alpha = 1;
		_canvasGroup.blocksRaycasts = true;

		for (int i = 0; i < UIManager.instance.Panels.Length; i++)
		{
			if(UIManager.instance.Panels[i] == this)
			{
				continue;
			}

			UIManager.instance.Panels[i].Hide();
		}
	}

	public void Hide()
	{
		_canvasGroup.alpha = 0;
		_canvasGroup.blocksRaycasts = false;
	}
}
