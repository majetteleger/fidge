using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

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
    public const string KColorOrange = "(CO)";
    public const string KColorPurple = "(CP)";
    public const string KTraversalStateCovered = "(TSC)";
    public const string KTraversalStateRevealed = "(TSR)";

    public static readonly string[] Collectables = {KKey, KFlag, KLink};
    public static readonly string[] Obstacles = {KLock, KWall, KCrack , KSlide};
    public static readonly string[] CollectablesAndObstacles = { KFlag, KSlide, KCrack, KWall, KKey, KLock, KLink };
    public static readonly string[] LevelElements = {KNode, KPath, KVertical, KHorizontal};
    public static readonly string[] Colors = {KColorRed, KColorGreen, KColorBlue, KColorOrange, KColorPurple};
    public static readonly string[] Directions = {KDirectionUp, KDirectionRight, KDirectionDown, KDirectionLeft};
    public static readonly string[] TraversalStates = { KTraversalStateCovered, KTraversalStateRevealed };
    
    public bool Scripted;
    public int AllowedEndNodeBypasses;
    public string Desription;
    public string Author;
    public int ExpectedTime;
    public int ExpectedMoves;
    public TutorialManager.TutorialTag[] ResetTutorials;

    [HideInInspector] public bool Shifted;
    [HideInInspector] public int Index;
    [HideInInspector] public int NumberOfSolutions;
    [HideInInspector] public int MinimumMoves;
    [HideInInspector] public int MinimumMovesWithFlag;
    [HideInInspector] public int Difficulty;
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

    public int MedalsNeededToUnlock => Mathf.CeilToInt(Index * MainManager.Instance.LevelUnlockMultiplier);

    public bool Unlocked => MainManager.Instance.Medals >= MedalsNeededToUnlock;

    public EditableLevel()
    {
        Elements = new string[KWidth * KHeight];
        NumberOfSolutions = -1;
        MinimumMoves = -1;
        MinimumMovesWithFlag = -1;
    }
    
    public string GetElement(int x, int y)
    {
        if (x + y * KWidth > Elements.Length - 1)
        {
            return null;
        }

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

    public bool ElementContains(int x, int y, string[] characters)
    {
        foreach (var character in characters)
        {
            if (ElementContains(x, y, character))
            {
                return true;
            }
        }

        return false;
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
                            Instantiate(level.PlayerPrefab, level.StartNode.transform);
                        }
                        // End node
                        else if (x == (int)EndNode.x && y == (int)EndNode.y)
                        {
                            level.EndNode = node;
                            level.EndNode.gameObject.AddComponent<TutorialTagger>().Tag = TutorialManager.TutorialTag.EndNode;
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
                                    newKeyRenderer.sprite = newKey.Sprites.RedSprite;
                                }
                                else if (ElementContains(x, y, KColorGreen))
                                {
                                    newKey.Color = Level.KeyLockColor.Green;
                                    newKeyRenderer.sprite = newKey.Sprites.GreenSprite;
                                }
                                else if (ElementContains(x, y, KColorBlue))
                                {
                                    newKey.Color = Level.KeyLockColor.Blue;
                                    newKeyRenderer.sprite = newKey.Sprites.BlueSprite;
                                }
                                else if (ElementContains(x, y, KColorOrange))
                                {
                                    newKey.Color = Level.KeyLockColor.Orange;
                                    newKeyRenderer.sprite = newKey.Sprites.OrangeSprite;
                                }
                                else if (ElementContains(x, y, KColorPurple))
                                {
                                    newKey.Color = Level.KeyLockColor.Purple;
                                    newKeyRenderer.sprite = newKey.Sprites.PurpleSprite;
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
                                    newLinkRenderer.sprite = newLink.Sprites.RedSprite;
                                }
                                else if (ElementContains(x, y, KColorGreen))
                                {
                                    newLink.Color = Level.KeyLockColor.Green;
                                    newLinkRenderer.sprite = newLink.Sprites.GreenSprite;
                                }
                                else if (ElementContains(x, y, KColorBlue))
                                {
                                    newLink.Color = Level.KeyLockColor.Blue;
                                    newLinkRenderer.sprite = newLink.Sprites.BlueSprite;
                                }
                                else if (ElementContains(x, y, KColorOrange))
                                {
                                    newLink.Color = Level.KeyLockColor.Orange;
                                    newLinkRenderer.sprite = newLink.Sprites.OrangeSprite;
                                }
                                else if (ElementContains(x, y, KColorPurple))
                                {
                                    newLink.Color = Level.KeyLockColor.Purple;
                                    newLinkRenderer.sprite = newLink.Sprites.PurpleSprite;
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
                                    newLockRenderer.sprite = newLock.Sprites.RedSprite;
                                }
                                else if (ElementContains(x, y, KColorGreen))
                                {
                                    newLock.Color = Level.KeyLockColor.Green;
                                    newLockRenderer.sprite = newLock.Sprites.GreenSprite;
                                }
                                else if (ElementContains(x, y, KColorBlue))
                                {
                                    newLock.Color = Level.KeyLockColor.Blue;
                                    newLockRenderer.sprite = newLock.Sprites.BlueSprite;
                                }
                                else if (ElementContains(x, y, KColorOrange))
                                {
                                    newLock.Color = Level.KeyLockColor.Orange;
                                    newLockRenderer.sprite = newLock.Sprites.OrangeSprite;
                                }
                                else if (ElementContains(x, y, KColorPurple))
                                {
                                    newLock.Color = Level.KeyLockColor.Purple;
                                    newLockRenderer.sprite = newLock.Sprites.PurpleSprite;
                                }
                            }
                            // Wall
                            else if (ElementContains(x, y, KWall))
                            {
                                Instantiate(level.WallPrefab, newElementGameObject.transform);
                                var newWall = newElementGameObject.GetComponentInChildren<Wall>();
                                var newWallRenderer = newWall.GetComponent<SpriteRenderer>();
                                
                                if (ElementContains(x, y, KHorizontal))
                                {
                                    newWallRenderer.sprite = newWall.Sprites.VerticalSprite;
                                }
                                else if(ElementContains(x, y, KVertical))
                                {
                                    newWallRenderer.sprite = newWall.Sprites.HorizontalSprite;
                                }
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
                                    newSlideRenderer.sprite = newSlide.Sprites.UpSprite;
                                }
                                else if (ElementContains(x, y, KDirectionRight))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Right;
                                    newSlideRenderer.sprite = newSlide.Sprites.RightSprite;
                                }
                                else if (ElementContains(x, y, KDirectionDown))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Down;
                                    newSlideRenderer.sprite = newSlide.Sprites.DownSprite;
                                }
                                else if (ElementContains(x, y, KDirectionLeft))
                                {
                                    newSlide.Direction = TraversalManager.TraversalMove.Left;
                                    newSlideRenderer.sprite = newSlide.Sprites.LeftSprite;
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

                        var newMask = Instantiate(level.ModifierMaskPrefab, level.NodesContainer);
                        newMask.transform.SetParent(newElementGameObject.transform, false);
                    }
                    else if (ElementContains(x, y, KTraversalStateRevealed))
                    {
                        newElement.State = Element.TraversalState.Revealed;
                        newTraversalStateGameObject = Instantiate(level.RevealedPrefab, level.NodesContainer);
                    }

                    if (newTraversalStateGameObject != null)
                    {
                        newTraversalStateGameObject.transform.SetParent(newElementGameObject.transform, false);
                        var traversalStateObject = newTraversalStateGameObject.GetComponent<TraversalStateModifier>();

                        newTraversalStateGameObject.GetComponent<SpriteRenderer>().color = new Color(
                            Random.Range(traversalStateObject.RandomColorFrom.r, traversalStateObject.RandomColorTo.r),
                            Random.Range(traversalStateObject.RandomColorFrom.g, traversalStateObject.RandomColorTo.g),
                            Random.Range(traversalStateObject.RandomColorFrom.b, traversalStateObject.RandomColorTo.b)
                        );
                    }
                }
            }
        }

        level.Initiliaze(this);

        return level;
    }
}