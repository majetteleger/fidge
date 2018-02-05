using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Obstacle
{
    public int Health { get; set; }

    private int _currentHealth;

    private void Start()
    {
        Health = 1; // CHANGE IN EDITOR?
        _currentHealth = Health;
    }

    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        if(_currentHealth <= 0)
        {
            return nextTraversalNode;
        }

        _currentHealth--;
        return currentTraversalNode;
    }

    public override void HandleResolution()
    {
        base.HandleResolution();

        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
