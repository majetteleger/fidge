using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectable : Content
{
    public override void Contact()
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Good);

        transform.SetParent(MainManager.Instance.Player.transform, false);
        GetComponent<SpriteRenderer>().enabled = false;

        InGamePanel.instance.UpdateCollectables(MainManager.Instance.Player.GetCollectables());
    }
}
