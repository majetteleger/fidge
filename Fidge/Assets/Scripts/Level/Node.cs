using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : Element
{
    public Sprite Sprite;
    public Sprite StartSprite;
    public Sprite EndSprite;

    public VerticalPath UpPath { get; set; }
    public HorizontalPath RightPath { get; set; }
    public VerticalPath DownPath { get; set; }
    public HorizontalPath LeftPath { get; set; }
    
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

    public override void Cover()
    {
        base.Cover();

        if (UpPath != null)
        {
            UpPath.ChangeSprite(UpPath.Sprite == UpPath.UpStubSprite ? UpPath.BothStubSprite : UpPath.DownStubSprite);
        }
        if (RightPath != null)
        {
            RightPath.ChangeSprite(RightPath.Sprite == RightPath.RightStubSprite ? RightPath.BothStubSprite : RightPath.LeftStubSprite);
        }
        if (DownPath != null)
        {
            DownPath.ChangeSprite(DownPath.Sprite == DownPath.DownStubSprite ? DownPath.BothStubSprite : DownPath.UpStubSprite);
        }
        if (LeftPath != null)
        {
            LeftPath.ChangeSprite(LeftPath.Sprite == LeftPath.LeftStubSprite ? LeftPath.BothStubSprite : LeftPath.RightStubSprite);
        }
    }

    public override void Reveal()
    {
        base.Reveal();

        if (UpPath != null)
        {
            UpPath.ChangeSprite(UpPath.Sprite == UpPath.BothStubSprite ? UpPath.UpStubSprite : UpPath.Sprite);
        }
        if (RightPath != null)
        {
            RightPath.ChangeSprite(RightPath.Sprite == RightPath.BothStubSprite ? RightPath.RightStubSprite : RightPath.Sprite);
        }
        if (DownPath != null)
        {
            DownPath.ChangeSprite(DownPath.Sprite == DownPath.BothStubSprite ? DownPath.DownStubSprite : DownPath.Sprite);
        }
        if (LeftPath != null)
        {
            LeftPath.ChangeSprite(LeftPath.Sprite == LeftPath.BothStubSprite ? LeftPath.LeftStubSprite : LeftPath.Sprite);
        }
    }
}
