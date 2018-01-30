﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : Element
{
    public Sprite Sprite;

    public Node UpNode { get; set; }
    public Node RightNode { get; set; }
    public Node DownNode { get; set; }
    public Node LeftNode { get; set; }
    
    public Node Traverse(TraversalManager.TraversalMove traversalMove, Node currentTraversalNode)
    {
        var obstacle = getObstacle();
        var nextTraversalNode = (Node)null;
        
        switch (traversalMove)
        {
            case TraversalManager.TraversalMove.Up:

                nextTraversalNode = UpNode;
                break;
                
            case TraversalManager.TraversalMove.Right:

                nextTraversalNode = RightNode;
                break;

            case TraversalManager.TraversalMove.Down:

                nextTraversalNode = DownNode;
                break;

            case TraversalManager.TraversalMove.Left:

                nextTraversalNode = LeftNode;
                break;
        }

        if (obstacle != null)
        {
            var obstacleResolution = obstacle.Resolve(currentTraversalNode, nextTraversalNode, traversalMove);
            nextTraversalNode = obstacleResolution;

            if(obstacleResolution != null)
            {
                obstacle.HandleResolution();
            }
        }

        return nextTraversalNode;
    }

    private Obstacle getObstacle()
    {
        var obstacle = GetComponentInChildren<Obstacle>();

        return obstacle;
    }
}

