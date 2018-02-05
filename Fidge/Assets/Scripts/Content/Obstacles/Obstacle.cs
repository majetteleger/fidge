using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Content
{
    public virtual Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        return null;
    }

    public virtual void HandleResolution()
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Good);
    }
}
