using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSection : MonoBehaviour
{
    public RectTransform Gap;
    public RectTransform Blocker;
    
    public void UI_Pay()
    {
        UIManager.Instance.PurchasePanel.Show();
    }
}
