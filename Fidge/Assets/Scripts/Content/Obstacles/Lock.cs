using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : Obstacle
{
    public Sprite RedSprite;
    public Sprite BlueSprite;
    public Sprite GreenSprite;
    public Sprite CyanSprite;
    public Sprite MagentaSprite;
    public Sprite YellowSprite;

    public Level.KeyLockColor Color { get; set; }

    private int _keysToUnlock;

    private void Start()
    {
        _keysToUnlock = FindKeys();
    }

    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        var playerCollectables = MainManager.Instance.Player.GetCollectables();
        var collectedKeys = 0;

        for (var i = 0; i < playerCollectables.Length; i++)
        {
            var key = playerCollectables[i].GetComponent<Key>();

            if (key != null && key.Color == Color)
            {
                collectedKeys++;
            }
        }

        return collectedKeys >= _keysToUnlock ? nextTraversalNode: null;
    }

    public override void HandleResolution()
    {
        var playerCollectables = MainManager.Instance.Player.GetCollectables();

        for (var i = 0; i < playerCollectables.Length; i++)
        {
            var key = playerCollectables[i].GetComponent<Key>();

            if (key != null && key.Color == Color)
            {
                DestroyImmediate(key.gameObject);
            }
        }

        DestroyImmediate(gameObject);
        InGamePanel.instance.UpdateCollectables(MainManager.Instance.Player.GetCollectables());
    }

    public int FindKeys()
    {
        var keys = FindObjectsOfType<Key>();
        var keysToUnlock = 0;

        for (var i = 0; i < keys.Length; i++)
        {
            if(keys[i].Color == Color)
            {
                keysToUnlock++;
            }
        }

        return keysToUnlock;
    }
}
