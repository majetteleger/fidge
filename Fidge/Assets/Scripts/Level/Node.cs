using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : Element
{
    public Sprite Sprite;
    public Sprite StartSprite;
    public Sprite EndSprite;

    public Path UpPath { get; set; }
    public Path RightPath { get; set; }
    public Path DownPath { get; set; }
    public Path LeftPath { get; set; }
    
    public Collectable GetCollectable()
    {
        var collectables = GetComponentsInChildren<Collectable>();

        for (var i = 0; i < collectables.Length; i++)
        {
            if (collectables[i].transform.parent != MainManager.Instance.Player.transform)
            {
                return collectables[i];
            }
        }

        return null;
    }
}
