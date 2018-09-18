using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : Obstacle
{
    public ColoredSpriteCollection Sprites;
    public ColoredSpriteCollection EditorSprites;

    public Level.KeyLockColor Color { get; set; }
    
    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        var playerCollectables = MainManager.Instance.Player.GetCollectables();

        for (var i = 0; i < playerCollectables.Length; i++)
        {
            var key = playerCollectables[i].GetComponent<Key>();

            if (key != null && key.Color == Color)
            {
                return nextTraversalNode;
            }
        }

        return null;
    }

    public override IEnumerator HandleResolution()
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Lock);

        yield return new WaitForSeconds(TraversalManager.Instance.TraversalSpeed / 2);
        
        var playerCollectables = MainManager.Instance.Player.GetCollectables();

        for (var i = 0; i < playerCollectables.Length; i++)
        {
            var key = playerCollectables[i].GetComponent<Key>();

            if (key != null && key.Color == Color)
            {
                DestroyImmediate(key.gameObject);
                break;
            }
        }

        DestroyImmediate(gameObject);
        InGamePanel.instance.UpdateCollectables(MainManager.Instance.Player.GetCollectables());
    }
}
