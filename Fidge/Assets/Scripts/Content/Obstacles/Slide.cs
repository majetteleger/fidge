using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : Obstacle
{
    public Sprite UpSprite;
    public Sprite RightSprite;
    public Sprite DownSprite;
    public Sprite LeftSprite;

    public TraversalManager.TraversalMove Direction { get; set; }

    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        if (direction == Direction)
        {
            return nextTraversalNode;
        }

        return null;
    }

    public override void HandleResolution()
    {
        //base.HandleResolution();
    }
}
