﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectable : Content
{
    public override void Contact()
    {
        transform.SetParent(MainManager.Instance.Player.transform, false);
        GetComponent<SpriteRenderer>().enabled = false;

        InGamePanel.instance.UpdateCollectables(MainManager.Instance.Player.GetCollectables());
    }
}
