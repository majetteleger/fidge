using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject GameManager;

	void Start ()
	{
	    if (MainManager.Instance == null)
	    {
	        Instantiate(GameManager);
        }
	}
}
