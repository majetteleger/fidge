using System;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class EditableLevel : ScriptableObject
{
    public const int KWidth = 9;
    public const int KHeight = 13;
    public const float KNodeSize = 24f;
    public const float KPathLength = 0.96f;

    public const string KNode = "(N)";
    public const string KPath = "(P)";
    public const string KHorizontal = "(H)";
    public const string KVertical = "(V)";
    public const string KKey = "(K)";
    public const string KFlag = "(F)";
    public const string KLink = "(LK)";
    public const string KLock = "(L)";
    public const string KWall = "(W)";
    public const string KCrack = "(C)";
    public const string KSlide = "(S)";
    public const string KDirectionUp = "(DU)";
    public const string KDirectionRight = "(DR)";
    public const string KDirectionDown = "(DD)";
    public const string KDirectionLeft = "(DL)";
    public const string KColorRed = "(CR)";
    public const string KColorGreen = "(CG)";
    public const string KColorBlue = "(CB)";
    public const string KColorCyan = "(CC)";
    public const string KColorMagenta = "(CM)";
    public const string KColorYellow = "(CY)";
    public const string KTraversalStateCovered = "(TSC)";
    public const string KTraversalStateRevealed = "(TSR)";

    public static readonly string[] Collectables = {KKey, KFlag, KLink};
    public static readonly string[] Obstacles = {KLock, KWall, KCrack , KSlide};
    public static readonly string[] LevelElements = {KNode, KPath, KVertical, KHorizontal};
    public static readonly string[] Colors = {KColorRed, KColorGreen, KColorBlue, KColorCyan, KColorMagenta, KColorYellow};
    public static readonly string[] Directions = {KDirectionUp, KDirectionRight, KDirectionDown, KDirectionLeft};
    public static readonly string[] TraversalStates = { KTraversalStateCovered, KTraversalStateRevealed };

    public bool Scripted;
    public int AllowedEndNodeBypasses;
    public string Desription;
    public string Author;
    public int ExpectedTime;
    public int ExpectedMoves;
    public TraversalManager.TraversalMove[] TraversalScript;

    [HideInInspector] public bool Shifted;
    [HideInInspector] public int Index;
    [HideInInspector] public int NumberOfSolutions;
    [HideInInspector] public int MinimumMoves;
    [HideInInspector] public int MinimumMovesWithFlag;
    [HideInInspector] public Vector2 StartNode;
    [HideInInspector] public Vector2 EndNode;
    [HideInInspector] public string[] Elements;

    private GameObject levelPrefab;
    public GameObject LevelPrefab
    {
        get
        {
            if (levelPrefab == null)
            {
                levelPrefab = Resources.Load("Level", typeof(GameObject)) as GameObject;
            }

            return levelPrefab;
        }
    }

    public EditableLevel()
    {
        Elements = new string[KWidth * KHeight];
        NumberOfSolutions = -1;
        MinimumMoves = -1;
        MinimumMovesWithFlag = -1;
    }
    
    public string GetElement(int x, int y)
    {
        return Elements[x + y * KWidth];
    }

    public string GetElement(float x, float y)
    {
        return GetElement((int)x, (int)y);
    }

    public string GetElement(Vector2 vector)
    {
        return GetElement(vector.x, vector.y);
    }

    public void SetElement(int x, int y, string newElement)
    {
        Elements[x + y * KWidth] = newElement;
    }

    public void AddToElement(int x, int y, string contentToAdd)
    {
        Elements[x + y * KWidth] += contentToAdd;
    }

    public void RemoveFromElement(int x, int y, string contentToRemove)
    {
        var oldElement = Elements[x + y * KWidth];

        if (oldElement != null)
        {
            Elements[x + y * KWidth] = oldElement.Replace(contentToRemove, string.Empty);
        }
    }

    public void RemoveFromElement(int x, int y, string[] contentToRemove)
    {
        for (var i = 0; i < contentToRemove.Length; i++)
        {
            RemoveFromElement(x, y, contentToRemove[i]);
        }
    }

    public void RemoveAllContent(int x, int y)
    {
        var contentToRemove = new string[Collectables.Length + Obstacles.Length + Colors.Length + Directions.Length + TraversalStates.Length];
        Collectables.CopyTo(contentToRemove, 0);
        Obstacles.CopyTo(contentToRemove, Collectables.Length);
        Colors.CopyTo(contentToRemove, Collectables.Length + Obstacles.Length);
        Directions.CopyTo(contentToRemove, Collectables.Length + Obstacles.Length + Colors.Length);
        TraversalStates.CopyTo(contentToRemove, Collectables.Length + Obstacles.Length + Colors.Length + Directions.Length);

        RemoveFromElement(x, y, contentToRemove);
    }
    
    public bool ElementContains(int x, int y, string character)
    {
        var source = GetElement(x, y);
        return source != null && source.IndexOf(character, StringComparison.Ordinal) != -1;
    }

    public Level InstantiateLevel()
    {
        // TODO: Could potentially benefit from an Initialize() function in each element/collectables/obstacles class to thin this down

        var level = Instantiate(LevelPrefab).GetComponent<Level>();
        
        for (var x = 0; x < KWidth; x++)
        {
            for (var y = 0; y < KHeight; y++)
            {
                var element = Elements[x + y * KWidth];

                if (!string.IsNullOrEmpty(element))
                {
                    var newElementGameObject = (GameObject)null;

                    // Node
                    if (ElementContains(x, y, KNode))
                    {
                        newElementGameObject = Instantiate(level.NodePrefab, level.NodesContainer);
                        var node = newElementGameObject.GetComponent<Node>();

                        // Start node
                        if (x == (int)StartNode.x && y == (int)StartNode.y)
                        {
                            level.StartNode = node;
                        }
                        // End node
                        else if (x == (int)EndNode.x && y == (int)EndNode.y)
                        {
                            level.EndNode = node;
                        }

                        // Collectables
                        if (newElementGameObject != null)
                        {
                            // Key
                            if (ElementContains(x, y, KKey))
                            {
                                Instantiate(level.KeyPrefab, newElementGameObject.transform);
                                var newKey = newElementGameObject.GetComponentInChildren<Key>();
                                var newKeyRenderer = newKey.GetComponent<SpriteRenderer>();

                                if (ElementContains(x, y, KColorRed))
                                {
                                    newKey.Color = Level.KeyLockColor.Red;
                                    newKeyRenderer.sprite = newKey.RedSprite;
                                }
                                else if (ElementContains(x, y, KColorGreen))
                                {
                                    newKey.Color = Level.KeyLockColor.Green;
                                    newKeyRenderer.sprite = newKey.GreenSprite;
                                }
                                else if (ElementContains(x, y, KColorBlue))
                                {
                                    newKey.Color = Level.KeyLockColor.Blue;
                                    newKeyRenderer.sprite = newKey.BlueSprite;
                                }
                                else if (ElementContains(x, y, KColorCyan))
                                {
                                    newKey.Color = Level.KeyLockColor.Cyan;
                                    newKeyRenderer.sprite = newKey.CyanSprite;
                                }
                                else if (ElementContains(x, y, KColorMagenta))
                                {
                                    newKey.Color = Level.KeyLockColor.Magenta;
                                    newKeyRenderer.sprite = newKey.MagentaSprite;
                                }
                                else if (ElementContains(x, y, KColorYellow))
                                {
                                    newKey.Color = Level.KeyLockColor.Yellow;
                                    newKeyRenderer.sprite = newKey.YellowSprite;
                                }
                            }
                            // Link
                            if (ElementContains(x, y, KLink))
                            {
                                Instantiate(level.LinkPrefab, newElementGameObject.transform);
                                var newLink = newElementGameObject.GetComponentInChildren<Link>();
                                var newLinkRenderer = newLink.GetComponent<SpriteRenderer>();

                                if (ElementContains(x, y, KColorRed))
                                {
                                    newLink.Color = Level.KeyLockColor.Red;
                                    newLinkRenderer.sprite = newLink.RedSprite;
                                }
                                else if (ElementContains(x, y, KColorGreen))
                                {
                                    newLink.Color = Level.KeyLockColor.Green;
                                    newLinkRenderer.sprite = newLink.GreenSprite;
                                }
                                else if (ElementContains(x, y, KColorBlue))
                                {
                                    newLink.Color = Level.KeyLockColor.Blue;
                                    newLinkRenderer.sprite = newLink.BlueSprite;
                                }
                                else if (ElementContains(x, y, KColorCyan))
                                {
                                    newLink.Color = Level.KeyLockColor.Cyan;
                                    newLinkRenderer.sprite = newLink.CyanSprite;
                                }
                                else if (ElementContains(x, y, KColorMagenta))
                                {
                                    newLink.Color = Level.KeyLockColor.Magenta;
                                    newLinkRenderer.sprite = newLink.MagentaSprite;
                                }
                                else if (ElementContains(x, y, KColorYellow))
                                {
                                    newLink.Color = Level.KeyLockColor.Yellow;
                                    newLinkRenderer.sprite = newLink.YellowSprite;
                                }
                            }
                            // Flag
                            else if (ElementContains(x, y, KFlag))
                            {
                                Instantiate(level.FlagPrefab, newElementGameObject.transform);
                            }
                        }
                    }
                    // Path
                    else if (ElementContains(x, y, KPath))
                    {
                        // Vertical path
                        if (ElementContains(x, y, KVertical))
                        {
                            newElementGameObject = Instantiate(level.VerticalPathPrefab, level.PathsContainer);
                        }
                        // Horizontal path
                        else if (ElementContains(x, y, KHorizontal))
                        {
                            newElementGameObject = Instantiate(level.HorizontalPathPrefab, level.PathsContainer);
                        }

                        // Obstacles
                        if (newElementGameObject != null)
                        {
                            // Lock
                            if (ElementContains(x, y, KLock))
                            {
                                Instantiate(level.LockPrefab, newElementGameObject.transform);
                                var newLock = newElementGameObject.GetComponentInChildren<Lock>();
                                var newLockRenderer = newLock.GetComponent<SpriteRenderer>();

                                if (ElementContains(x, y, KColorRed))
                                {
                                    newLock.Color = Level.KeyLockColor.Red;
                                    newLockRenderer.sprite = newLock.RedSprite;
                                }
                                else if (ElementContains(x, y, KColorGreen))
                                {
                                    newLock.Color = Level.KeyLockColor.Green;
                                    newLockRenderer.sprite = newLock.GreenSprite;
                                }
                                else if (ElementContains(x, y, KColorBlue))
                                {
                                    newLock.Color = Level.KeyLockColor.Blue;
                                    newLockRenderer.sprite = newLock.BlueSprite;
                                }
                                else if (ElementContains(x, y, KColorCyan))
                                {
                                    newLock.Color = Level.KeyLockColor.Cyan;
                                    newLockRenderer.sprite = newLock.CyanSprite;
                                }
                                else if (ElementContains(x, y, KColorMagenta))
                                {
                                    newLock.Color = Level.KeyLockColor.Magenta;
                                    newLockRenderer.sprite = newLock.MagentaSprite;
                                }
                                else if (ElementContains(x, y, KColorYellow))
                                {
                                    newLock.Color = Level.KeyLockColor.Yellow;
                                    newLockRenderer.sprite = newLock.YellowSprite;
                                }
                            }
                            // Wall
                            else if (ElementContains(x, y, KWall))
                            {
                                Instantiate(level.WallPrefab, newElementGameObject.transform);
                            }
                            // Crack
                            else if (ElementContains(x, y, KCrack))
                            {
                                Instantiate(level.CrackPrefab, newElementGameObject.transform);
                            }
                            // Slide
                            else if (ElementContains(x, y, KSlide))
                            {
                                Instantiate(level.SlidePrefab, newElementGameObject.transform);
                                var newSlide = newElementGameObject.GetComponentInChildren<Slide>();
                                var newSlideRenderer = newSlide.GetComponent<SpriteRenderer>();

                                if (ElementContains(x, y, KDirectionUp))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Up;
                                    newSlideRenderer.sprite = newSlide.UpSprite;
                                }
                                else if (ElementContains(x, y, KDirectionRight))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Right;
                                    newSlideRenderer.sprite = newSlide.RightSprite;
                                }
                                else if (ElementContains(x, y, KDirectionDown))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Down;
                                    newSlideRenderer.sprite = newSlide.DownSprite;
                                }
                                else if (ElementContains(x, y, KDirectionLeft))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Left;
                                    newSlideRenderer.sprite = newSlide.LeftSprite;
                                }
                            }
                        }
                    }
                    
                    var halfPathLength = KPathLength / 2;

                    if (newElementGameObject == null)
                    {
                        newElementGameObject = Instantiate(level.EmptyElementPrefab, level.EmptyElementsContainer);
                    }

                    newElementGameObject.transform.localPosition = new Vector3(
                        x * halfPathLength - ((KWidth - 1) * halfPathLength / 2),
                        -y * halfPathLength + ((KHeight - 1) * halfPathLength / 2),
                        0
                    );

                    // Traversal state
                    var newTraversalStateGameObject = (GameObject)null;
                    var newElement = newElementGameObject.GetComponent<Element>();

                    if (ElementContains(x, y, KTraversalStateCovered))
                    {
                        newElement.State = Element.TraversalState.Covered;
                        newTraversalStateGameObject = Instantiate(level.CoveredPrefab, level.NodesContainer);
                    }
                    else if (ElementContains(x, y, KTraversalStateRevealed))
                    {
                        newElement.State = Element.TraversalState.Revealed;
                        newTraversalStateGameObject = Instantiate(level.RevealedPrefab, level.NodesContainer);

                        newElement.Cover();
                    }

                    if (newTraversalStateGameObject != null)
                    {
                        newTraversalStateGameObject.transform.SetParent(newElementGameObject.transform, false);
                    }
                }
            }
        }

        level.Initiliaze(this);

        return level;
    }
}