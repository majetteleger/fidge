using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : Obstacle
{
    public DirectedSpriteCollection Sprites;
    public DirectedSpriteCollection EditorSprites;

    public TraversalManager.TraversalMove Direction { get; set; }

    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        if (direction == Direction)
        {
            return nextTraversalNode;
        }

        return null;
    }

    public override IEnumerator HandleResolution()
    {
        yield return new WaitForSeconds(TraversalManager.Instance.TraversalSpeed / 2);

        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Slide);
    }
}
