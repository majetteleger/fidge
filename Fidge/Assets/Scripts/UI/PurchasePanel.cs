using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePanel : Panel
{
    public Text MessageText;
    public Button CancelButton;
    public Button ConfirmButton;
    public Button CloseButton;
    [TextArea] public string PurchaseMessage;
    [TextArea] public string ThankYouMessage;

    private bool _forceRebuild;
    
    public override void Show(Panel originPanel = null)
    {
        MessageText.text = PurchaseMessage;
        CancelButton.gameObject.SetActive(true);
        ConfirmButton.gameObject.SetActive(true);
        CloseButton.gameObject.SetActive(false);

        ForceLayoutRebuilding(GetComponent<RectTransform>());
        
        base.Show(originPanel);
    }

    public void UI_Confirm()
    {
        MessageText.text = ThankYouMessage;
        CancelButton.gameObject.SetActive(false);
        ConfirmButton.gameObject.SetActive(false);
        CloseButton.gameObject.SetActive(true);
        
        ForceLayoutRebuilding(GetComponent<RectTransform>());
    }

    public void UI_Close()
    {
        Hide();
    }
}
