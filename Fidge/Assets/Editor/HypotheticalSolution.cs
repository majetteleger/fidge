using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HypotheticalSolution
{
    public int Id;
    public string[] Elements;
    public Vector2 StartNode;
    public Vector2 EndNode;
	public List<TraversalManager.TraversalMove> Movements;
    public List<Vector2> VisitedElements;
    public int EndNodeVisited;
    public int EndNodeBypasses;

    private List<string> collectedCollectables;
    public List<string> CollectedCollectables
    {
        get
        {
            if (collectedCollectables == null)
            {
                collectedCollectables = new List<string>();
            }

            return collectedCollectables;
        }
        set
        {
            collectedCollectables = value;
        }
    }

    public HypotheticalSolution(EditableLevel editableLevel)
    {
        Id = LevelEditor.Solutions.Count;
        LevelEditor.Solutions.Add(this);

        Elements = new string[editableLevel.Elements.Length];

        for (var i = 0; i < Elements.Length; i++)
        {
            Elements[i] = editableLevel.Elements[i];
        }
        
        StartNode = new Vector2(editableLevel.StartNode.x, editableLevel.StartNode.y);
        EndNode = new Vector2(editableLevel.EndNode.x, editableLevel.EndNode.y);
        Movements = new List<TraversalManager.TraversalMove>();
        VisitedElements = new List<Vector2>();
        CollectedCollectables = new List<string>();
        EndNodeBypasses = editableLevel.AllowedEndNodeBypasses;
    }

    public HypotheticalSolution(HypotheticalSolution otherSolution)
    {
        Id = LevelEditor.Solutions.Count;
        LevelEditor.Solutions.Add(this);
        
        Elements = new string[otherSolution.Elements.Length];

        for (var i = 0; i < Elements.Length; i++)
        {
            Elements[i] = otherSolution.Elements[i];
        }

        StartNode = new Vector2(otherSolution.StartNode.x, otherSolution.StartNode.y);
        EndNode = new Vector2(otherSolution.EndNode.x, otherSolution.EndNode.y);
        
        Movements = new List<TraversalManager.TraversalMove>();

        for (var i = 0; i < otherSolution.Movements.Count; i++)
        {
            Movements.Add(otherSolution.Movements[i]);
        }
            
        VisitedElements = new List<Vector2>();

        for (var i = 0; i < otherSolution.VisitedElements.Count; i++)
        {
            VisitedElements.Add(otherSolution.VisitedElements[i]);
        }

        CollectedCollectables = new List<string>();

        for (var i = 0; i < otherSolution.CollectedCollectables.Count; i++)
        {
            CollectedCollectables.Add(otherSolution.CollectedCollectables[i]);
        }

        EndNodeVisited = otherSolution.EndNodeVisited;
        EndNodeBypasses = otherSolution.EndNodeBypasses;
    }

    public void Solve()
    {
        Solve(StartNode);
    }

    public void Solve(Vector2 position)
    {
        if (Movements.Count > 100)
        {
            Debug.Log("Solution aborted");
            return;
        }

        var currentPosition = position;

        GrabCollectables(currentPosition);
        VisitedElements.Add(currentPosition);

        if (GetElement(currentPosition).Contains(EditableLevel.KLink))
        {
            var currentLinkColor = string.Empty;
            
            for (var i = 0; i < EditableLevel.Colors.Length; i++)
            {
                if (GetElement(currentPosition).Contains(EditableLevel.Colors[i]))
                {
                    currentLinkColor = EditableLevel.Colors[i];
                    break;
                }
            }

            for (var x = 0; x < EditableLevel.KWidth; x++)
            {
                for (var y = 0; y < EditableLevel.KHeight; y++)
                {
                    var newPosition = new Vector2(x, y);
                    var newElement = GetElement(x, y);

                    if (newElement != null && newElement.Contains(EditableLevel.KLink) && currentPosition != newPosition)
                    {
                        var otherLinkColor = string.Empty;

                        for (var i = 0; i < EditableLevel.Colors.Length; i++)
                        {
                            if (newElement.Contains(EditableLevel.Colors[i]))
                            {
                                otherLinkColor = EditableLevel.Colors[i];
                                break;
                            }
                        }

                        if (currentLinkColor == otherLinkColor)
                        {
                            currentPosition = newPosition;
                            goto linked;
                        }
                    }
                }
            }
        }

        linked:

        if (EndNode == currentPosition)
        {
            if (EndNodeVisited >= EndNodeBypasses)
            {
                return;
            }

            EndNodeVisited++;
            
            var newSolution = new HypotheticalSolution(this);
            newSolution.Solve(currentPosition);
        }
        
        var wall = GetElement(currentPosition).Contains(EditableLevel.KWall);
        
        TraversalManager.TraversalMove[] moves =
        {
            TraversalManager.TraversalMove.Up,
            TraversalManager.TraversalMove.Right,
            TraversalManager.TraversalMove.Down,
            TraversalManager.TraversalMove.Left
        };

        var possibleMoves = new List<TraversalManager.TraversalMove>();

        for (var i = 0; i < moves.Length; i++)
        {
            if (TryTraverse(currentPosition, moves[i]))
            {
                possibleMoves.Add(moves[i]);
            }
        }

        if (possibleMoves.Count == 0)
        {
            LevelEditor.Solutions.Remove(this);
            return;
        }

        var solutions = new HypotheticalSolution[possibleMoves.Count];

        for (var i = 0; i < solutions.Length; i++)
        {
            if (i > 0)
            {
                var newSolution = new HypotheticalSolution(this);
                solutions[i] = newSolution;
            }
            else
            {
                solutions[i] = this;
            }
        }

        for (var i = 0; i < possibleMoves.Count; i++)
        {
            var nextPosition = GetNextPosition(currentPosition, possibleMoves[i]);

            if (wall)
            {
                solutions[i].Movements.Add(possibleMoves[i]);
                solutions[i].Movements.Add(possibleMoves[i]);   // ewww
            }
            
            solutions[i].Movements.Add(possibleMoves[i]);
            solutions[i].Solve(nextPosition);
        }
    }

    private bool TryTraverse(Vector2 currentPosition, TraversalManager.TraversalMove move)
    {
        var currentElement = GetElement(currentPosition);
        var nextElement = GetElement(currentPosition, move);
        var nextPosition = GetNextPosition(currentPosition, move);

        if (VisitedElements.Contains(nextPosition) || string.IsNullOrEmpty(nextElement))
        {
            return false;
        }
        
        switch (move)
        {
            case TraversalManager.TraversalMove.Up:
            case TraversalManager.TraversalMove.Down:

                if (currentElement.Contains(EditableLevel.KNode) && (!nextElement.Contains(EditableLevel.KPath) || !nextElement.Contains(EditableLevel.KVertical)) ||
                    currentElement.Contains(EditableLevel.KHorizontal))
                {
                    return false;
                }

                break;

            case TraversalManager.TraversalMove.Left:
            case TraversalManager.TraversalMove.Right:

                if (currentElement.Contains(EditableLevel.KNode) && (!nextElement.Contains(EditableLevel.KPath) || !nextElement.Contains(EditableLevel.KHorizontal)) ||
                    currentElement.Contains(EditableLevel.KVertical))
                {
                    return false;
                }

                break;
        }

        // Obstacles
        if (currentElement.Contains(EditableLevel.KPath))
        {
            if (currentElement.Contains(EditableLevel.KSlide))
            {
                if (currentElement.Contains(EditableLevel.KDirectionUp) && move != TraversalManager.TraversalMove.Up ||
                    currentElement.Contains(EditableLevel.KDirectionRight) && move != TraversalManager.TraversalMove.Right ||
                    currentElement.Contains(EditableLevel.KDirectionDown) && move != TraversalManager.TraversalMove.Down ||
                    currentElement.Contains(EditableLevel.KDirectionLeft) && move != TraversalManager.TraversalMove.Left)
                {
                    return false;
                }
            }
            if (currentElement.Contains(EditableLevel.KCrack))
            {
                SetElement((int)currentPosition.x, (int)currentPosition.y, string.Empty);
            }
            if (currentElement.Contains(EditableLevel.KWall))
            {
                RemoveFromElement((int)currentPosition.x, (int)currentPosition.y, EditableLevel.KWall);
            }
            if (currentElement.Contains(EditableLevel.KLock))
            {
                var unlocked = false;
                var lockColor = string.Empty;

                for (var i = 0; i < EditableLevel.Colors.Length; i++)
                {
                    if (currentElement.Contains(EditableLevel.Colors[i]))
                    {
                        lockColor = EditableLevel.Colors[i];
                        break;
                    }
                }

                for (var i = 0; i < Elements.Length; i++)
                {
                    if (Elements[i] != null && Elements[i].Contains(EditableLevel.KKey))
                    {
                        var keyColor = string.Empty;

                        for (var j = 0; j < EditableLevel.Colors.Length; j++)
                        {
                            if (Elements[i].Contains(EditableLevel.Colors[j]))
                            {
                                keyColor = EditableLevel.Colors[j];

                                if (lockColor == keyColor)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                for (var i = 0; i < CollectedCollectables.Count; i++)
                {
                    if (CollectedCollectables[i].Contains(EditableLevel.KKey))
                    {
                        var keyColor = string.Empty;

                        for (var j = 0; j < EditableLevel.Colors.Length; j++)
                        {
                            if (CollectedCollectables[i].Contains(EditableLevel.Colors[j]))
                            {
                                keyColor = EditableLevel.Colors[j];

                                if (lockColor == keyColor)
                                {
                                    unlocked = true;
                                }
                            }
                        }
                    }
                }

                if (unlocked)
                {
                    RemoveFromElement((int)currentPosition.x, (int)currentPosition.y, EditableLevel.KLock);
                }
                else
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    private void GrabCollectables(Vector2 position)
    {
        var element = GetElement(position);

        for (var i = 0; i < EditableLevel.Collectables.Length; i++)
        {
            if(EditableLevel.Collectables[i] == EditableLevel.KLink || !element.Contains(EditableLevel.Collectables[i]))
            {
                continue;
            }

            var color = string.Empty;

            for (var j = 0; j < EditableLevel.Colors.Length; j++)
            {
                if (element.Contains(EditableLevel.Colors[j]))
                {
                    color = EditableLevel.Colors[j];
                    break;
                }
            }
            
            CollectedCollectables.Add(EditableLevel.Collectables[i] + color);
            RemoveFromElement((int)position.x, (int)position.y, EditableLevel.Collectables[i]);
            RemoveFromElement((int)position.x, (int)position.y, EditableLevel.Colors[i]);
            VisitedElements.Clear();
        }
    }

    private Vector2 GetNextPosition(Vector2 currentPosition, TraversalManager.TraversalMove move = TraversalManager.TraversalMove.NONE)
    {
        var nextPosition = currentPosition;

        switch (move)
        {
            case TraversalManager.TraversalMove.Up:
                nextPosition = new Vector2(currentPosition.x, currentPosition.y - 1);
                break;
            case TraversalManager.TraversalMove.Right:
                nextPosition = new Vector2(currentPosition.x + 1, currentPosition.y);
                break;
            case TraversalManager.TraversalMove.Down:
                nextPosition = new Vector2(currentPosition.x, currentPosition.y + 1);
                break;
            case TraversalManager.TraversalMove.Left:
                nextPosition = new Vector2(currentPosition.x - 1, currentPosition.y);
                break;
        }

        return nextPosition;
    }
    
    private string GetElement(int x, int y, TraversalManager.TraversalMove move = TraversalManager.TraversalMove.NONE)
    {
        var position = GetNextPosition(new Vector2(x, y), move);

        if((int)position.x + (int)position.y * EditableLevel.KWidth >= Elements.Length ||
           (int)position.x + (int)position.y * EditableLevel.KWidth < 0)
        {
            return string.Empty;
        }

        return Elements[(int)position.x + (int)position.y * EditableLevel.KWidth];
    }

    private string GetElement(float x, float y, TraversalManager.TraversalMove move = TraversalManager.TraversalMove.NONE)
    {
        return GetElement((int)x, (int)y, move);
    }

    private string GetElement(Vector2 vector, TraversalManager.TraversalMove move = TraversalManager.TraversalMove.NONE)
    {
        return GetElement(vector.x, vector.y, move);
    }

    private void SetElement(int x, int y, string newElement)
    {
        Elements[x + y * EditableLevel.KWidth] = newElement;
    }

    private void RemoveFromElement(int x, int y, string contentToRemove)
    {
        var oldElement = Elements[x + y * EditableLevel.KWidth];

        if (oldElement != null)
        {
            Elements[x + y * EditableLevel.KWidth] = oldElement.Replace(contentToRemove, string.Empty);
        }
    }

    private void RemoveFromElement(int x, int y, string[] contentToRemove)
    {
        for (var i = 0; i < contentToRemove.Length; i++)
        {
            RemoveFromElement(x, y, contentToRemove[i]);
        }
    }

    private bool ElementContains(int x, int y, string character)
    {
        var source = GetElement(x, y);
        return source != null && source.IndexOf(character, StringComparison.Ordinal) != -1;
    }
}
