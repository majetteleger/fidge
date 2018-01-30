using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public static UIManager instance = null;
    
	private Panel[] _panels;
	public Panel[] Panels
	{
		get
		{
			if(_panels == null)
			{
				_panels = FindObjectsOfType<Panel>();
			}

			return _panels;
		}
	}

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
	}
}
