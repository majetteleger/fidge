using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crack : Obstacle
{
    public Path Path { get; set; }
    public int Health { get; set; }

    private int _currentHealth;

    void Start()
    {
        Health = 1; // CHANGE IN EDITOR?
        _currentHealth = Health;

        Path = transform.parent.GetComponent<Path>();
    }

    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        _currentHealth--;
        return nextTraversalNode;
    }

    public override void HandleResolution()
    {
        if (_currentHealth <= 0)
        {
            Destroy(Path.gameObject);
        }
    }
}
