using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacle : Content
{
    public abstract Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction);

    public abstract void HandleResolution();
}
