using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEditPanel : Panel
{
    public class UserLevel
    {
        public bool Valid;
        public bool Uploaded;
        public string Guid;
        public string UserId;
        public int ExpectedTime;
        public int ExpectedMoves;
        public int MinimumMovesWithFlag;
        public int Difficulty;
        public Vector2 StartNode;
        public Vector2 EndNode;
        public string[] Elements;

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

        public Level InstantiateLevel()
        {
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
                        if (element.Contains(EditableLevel.KNode))
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
                                if (element.Contains(EditableLevel.KKey))
                                {
                                    Instantiate(level.KeyPrefab, newElementGameObject.transform);
                                    var newKey = newElementGameObject.GetComponentInChildren<Key>();
                                    var newKeyRenderer = newKey.GetComponent<SpriteRenderer>();

                                    if (element.Contains(EditableLevel.KColorRed))
                                    {
                                        newKey.Color = Level.KeyLockColor.Red;
                                        newKeyRenderer.sprite = newKey.Sprites.RedSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorGreen))
                                    {
                                        newKey.Color = Level.KeyLockColor.Green;
                                        newKeyRenderer.sprite = newKey.Sprites.GreenSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorBlue))
                                    {
                                        newKey.Color = Level.KeyLockColor.Blue;
                                        newKeyRenderer.sprite = newKey.Sprites.BlueSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorOrange))
                                    {
                                        newKey.Color = Level.KeyLockColor.Orange;
                                        newKeyRenderer.sprite = newKey.Sprites.OrangeSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorPurple))
                                    {
                                        newKey.Color = Level.KeyLockColor.Purple;
                                        newKeyRenderer.sprite = newKey.Sprites.PurpleSprite;
                                    }
                                }
                                // Link
                                if (element.Contains(EditableLevel.KLink))
                                {
                                    Instantiate(level.LinkPrefab, newElementGameObject.transform);
                                    var newLink = newElementGameObject.GetComponentInChildren<Link>();
                                    var newLinkRenderer = newLink.GetComponent<SpriteRenderer>();

                                    if (element.Contains(EditableLevel.KColorRed))
                                    {
                                        newLink.Color = Level.KeyLockColor.Red;
                                        newLinkRenderer.sprite = newLink.Sprites.RedSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorGreen))
                                    {
                                        newLink.Color = Level.KeyLockColor.Green;
                                        newLinkRenderer.sprite = newLink.Sprites.GreenSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorBlue))
                                    {
                                        newLink.Color = Level.KeyLockColor.Blue;
                                        newLinkRenderer.sprite = newLink.Sprites.BlueSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorOrange))
                                    {
                                        newLink.Color = Level.KeyLockColor.Orange;
                                        newLinkRenderer.sprite = newLink.Sprites.OrangeSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorPurple))
                                    {
                                        newLink.Color = Level.KeyLockColor.Purple;
                                        newLinkRenderer.sprite = newLink.Sprites.PurpleSprite;
                                    }
                                }
                                // Flag
                                else if (element.Contains(EditableLevel.KFlag))
                                {
                                    Instantiate(level.FlagPrefab, newElementGameObject.transform);
                                }
                            }
                        }
                        // Path
                        else if (element.Contains(EditableLevel.KPath))
                        {
                            // Vertical path
                            if (element.Contains(EditableLevel.KVertical))
                            {
                                newElementGameObject = Instantiate(level.VerticalPathPrefab, level.PathsContainer);
                            }
                            // Horizontal path
                            else if (element.Contains(EditableLevel.KHorizontal))
                            {
                                newElementGameObject = Instantiate(level.HorizontalPathPrefab, level.PathsContainer);
                            }

                            // Obstacles
                            if (newElementGameObject != null)
                            {
                                // Lock
                                if (element.Contains(EditableLevel.KLock))
                                {
                                    Instantiate(level.LockPrefab, newElementGameObject.transform);
                                    var newLock = newElementGameObject.GetComponentInChildren<Lock>();
                                    var newLockRenderer = newLock.GetComponent<SpriteRenderer>();

                                    if (element.Contains(EditableLevel.KColorRed))
                                    {
                                        newLock.Color = Level.KeyLockColor.Red;
                                        newLockRenderer.sprite = newLock.Sprites.RedSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorGreen))
                                    {
                                        newLock.Color = Level.KeyLockColor.Green;
                                        newLockRenderer.sprite = newLock.Sprites.GreenSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorBlue))
                                    {
                                        newLock.Color = Level.KeyLockColor.Blue;
                                        newLockRenderer.sprite = newLock.Sprites.BlueSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorOrange))
                                    {
                                        newLock.Color = Level.KeyLockColor.Orange;
                                        newLockRenderer.sprite = newLock.Sprites.OrangeSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KColorPurple))
                                    {
                                        newLock.Color = Level.KeyLockColor.Purple;
                                        newLockRenderer.sprite = newLock.Sprites.PurpleSprite;
                                    }
                                }
                                // Wall
                                else if (element.Contains(EditableLevel.KWall))
                                {
                                    Instantiate(level.WallPrefab, newElementGameObject.transform);
                                    var newWall = newElementGameObject.GetComponentInChildren<Wall>();
                                    var newWallRenderer = newWall.GetComponent<SpriteRenderer>();

                                    if (element.Contains(EditableLevel.KHorizontal))
                                    {
                                        newWallRenderer.sprite = newWall.Sprites.VerticalSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KVertical))
                                    {
                                        newWallRenderer.sprite = newWall.Sprites.HorizontalSprite;
                                    }
                                }
                                // Crack
                                else if (element.Contains(EditableLevel.KCrack))
                                {
                                    Instantiate(level.CrackPrefab, newElementGameObject.transform);
                                }
                                // Slide
                                else if (element.Contains(EditableLevel.KSlide))
                                {
                                    Instantiate(level.SlidePrefab, newElementGameObject.transform);
                                    var newSlide = newElementGameObject.GetComponentInChildren<Slide>();
                                    var newSlideRenderer = newSlide.GetComponent<SpriteRenderer>();

                                    if (element.Contains(EditableLevel.KDirectionUp))
                                    {
                                        newSlide.Direction = TraversalManager.TraversalMove.Up;
                                        newSlideRenderer.sprite = newSlide.Sprites.UpSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KDirectionRight))
                                    {
                                        newSlide.Direction = TraversalManager.TraversalMove.Right;
                                        newSlideRenderer.sprite = newSlide.Sprites.RightSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KDirectionDown))
                                    {
                                        newSlide.Direction = TraversalManager.TraversalMove.Down;
                                        newSlideRenderer.sprite = newSlide.Sprites.DownSprite;
                                    }
                                    else if (element.Contains(EditableLevel.KDirectionLeft))
                                    {
                                        newSlide.Direction = TraversalManager.TraversalMove.Left;
                                        newSlideRenderer.sprite = newSlide.Sprites.LeftSprite;
                                    }
                                }
                            }
                        }

                        var halfPathLength = EditableLevel.KPathLength / 2;

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

                        if (element.Contains(EditableLevel.KTraversalStateCovered))
                        {
                            newElement.State = Element.TraversalState.Covered;
                            newTraversalStateGameObject = Instantiate(level.CoveredPrefab, level.NodesContainer);

                            var newMask = Instantiate(level.ModifierMaskPrefab, level.NodesContainer);
                            newMask.transform.SetParent(newElementGameObject.transform, false);
                        }
                        else if (element.Contains(EditableLevel.KTraversalStateRevealed))
                        {
                            newElement.State = Element.TraversalState.Revealed;
                            newTraversalStateGameObject = Instantiate(level.RevealedPrefab, level.NodesContainer);
                        }

                        if (newTraversalStateGameObject != null)
                        {
                            newTraversalStateGameObject.transform.SetParent(newElementGameObject.transform, false);
                            var traversalStateObject = newTraversalStateGameObject.GetComponent<TraversalStateModifier>();

                            newTraversalStateGameObject.GetComponent<SpriteRenderer>().color = new Color(
                                UnityEngine.Random.Range(traversalStateObject.RandomColorFrom.r, traversalStateObject.RandomColorTo.r),
                                UnityEngine.Random.Range(traversalStateObject.RandomColorFrom.g, traversalStateObject.RandomColorTo.g),
                                UnityEngine.Random.Range(traversalStateObject.RandomColorFrom.b, traversalStateObject.RandomColorTo.b)
                            );
                        }
                    }
                }
            }

            level.Initiliaze(this);

            return level;
        }


    }

    public const int KWidth = 7;
    public const int KHeight = 11;
    public const string KBase = "BASE";
    public const string KDelete = "DELETE";

    public enum UserClickType
    {
        Base,
        Extra,
        TraversalModifier,
        NONE
    }
    
    public GameObject LevelCellPrefab;
    public Transform LevelCellContainer;
    public Button ToggleElementButton;
    public Image ToggleElementButtonImage;
    public Button ToggleExtraButton;
    public Image ToggleExtraButtonImage;
    public Button ToggleTraversalModifierButton;
    public Image ToggleTraversalModifierButtonImage;
    public Button OtherOptionsButton;
    public LevelEditContextMenu ContextMenu;
    public GameObject MessageBubble;
    public GameObject MessageBubbleBackground;
    public GameObject MessageBubbleConfirmButton;
    public GameObject MessageBubbleCancelButton;
    public GameObject MessageBubbleContinueButton;
    public GameObject MessageBubbleSaveAndQuitButton;
    public Text BackMessageText;
    public float LongPressTime;
    [TextArea] public string BackValidMessage;
    [TextArea] public string BackInvalidMessage;
    [TextArea] public string SaveValidMessage;
    [TextArea] public string SaveInvalidMessage;

    [Header("Sprites")]
    public Sprite NodeSprite;
    public Sprite StartNodeSprite;
    public Sprite EndNodeSprite;
    public Sprite VerticalPathSprite;
    public Sprite HorizontalPathSprite;
    public Sprite KeySprite;
    public Sprite LinkSprite;
    public Sprite LockSprite;
    public Sprite SlideSprite;
    public Sprite CrackSprite;
    public Sprite WallSprite;
    public Sprite FlagSprite;
    public Sprite CoveredSprite;
    public Sprite RevealedSprite;
    public Sprite DeleteSprite;
    
    public List<UserHypotheticalSolution> Solutions { get; set; }
    public UserLevel CurrentUserLevel { get; set; }
    
    private UserClickType _clickType;
    private LevelCell[] _levelCells;
    private bool _cellPressed;
    private float _longPressTimer;
    private LevelCell _cellClicked;
    private string _toggledBase;
    private string _toggledExtra;
    private string _toggledTraversalModifier;
    private bool _validationDirty;
    
    void Awake()
    {
        _longPressTimer = LongPressTime;
        _clickType = UserClickType.NONE;
        _toggledBase = KBase;
        _toggledExtra = EditableLevel.KKey;
        _toggledTraversalModifier = EditableLevel.KTraversalStateCovered;

        var levelCellList = new List<LevelCell>();

        for (var i = 0; i < KWidth * KHeight; i++)
        {
            var newLevelCell = Instantiate(LevelCellPrefab, LevelCellContainer).GetComponent<LevelCell>();
            var newPosition = new Vector2Int(i % KWidth, i / KWidth);
            newLevelCell.Position = newPosition;

            var trigger = newLevelCell.gameObject.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((e) =>
            {
                _cellClicked = EventSystem.current.currentSelectedGameObject.GetComponent<LevelCell>();

                _cellPressed = true;
            });

            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((e) =>
            {
                _cellClicked = EventSystem.current.currentSelectedGameObject.GetComponent<LevelCell>();

                ClickOnCell();
                _longPressTimer = LongPressTime;
                _cellPressed = false;
            });

            trigger.triggers.Add(pointerUp);

            levelCellList.Add(newLevelCell);
        }

        _levelCells = levelCellList.ToArray();
    }

    void Start()
    {
        SetupSounds();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.MainMenuPanel.Show();
        }

        if (_cellPressed && _longPressTimer > 0)
        {
            _longPressTimer -= Time.deltaTime;

            if (_longPressTimer <= 0)
            {
                OpenContextMenu(_clickType);
            }
        }
    }

    public void ShowAndLoadLevel(UserLevel userLevel)
    {
        CurrentUserLevel = userLevel;
        
        for (var i = 0; i < userLevel.Elements.Length; i++)
        {
            var levelCell = _levelCells[i];

            levelCell.Content = userLevel.Elements[i];

            // BASE
            if (levelCell.Content.Contains(EditableLevel.KNode))
            {
                if (levelCell.Position == CurrentUserLevel.StartNode)
                {
                    levelCell.ChangeSprite(StartNodeSprite, UserClickType.Base);
                }
                else if (levelCell.Position == CurrentUserLevel.EndNode)
                {
                    levelCell.ChangeSprite(EndNodeSprite, UserClickType.Base);
                }
                else
                {
                    levelCell.ChangeSprite(NodeSprite, UserClickType.Base);
                }
            }
            else if(levelCell.Content.Contains(EditableLevel.KPath))
            {
                if (levelCell.Content.Contains(EditableLevel.KHorizontal))
                {
                    levelCell.ChangeSprite(HorizontalPathSprite, UserClickType.Base);
                }
                else if (levelCell.Content.Contains(EditableLevel.KVertical))
                {
                    levelCell.ChangeSprite(VerticalPathSprite, UserClickType.Base);
                }
            }

            // EXTRA
            if (levelCell.Content.Contains(EditableLevel.KKey))
            {
                for (var j = 0; j < EditableLevel.Colors.Length; j++)
                {
                    if (levelCell.Content.Contains(EditableLevel.Colors[j]))
                    {
                        levelCell.ChangeSprite(levelCell.KeySprites[j], UserClickType.Extra);
                    }
                }
            }
            else if(levelCell.Content.Contains(EditableLevel.KFlag))
            {
                levelCell.ChangeSprite(levelCell.FlagSprite, UserClickType.Extra);
            }
            else if (levelCell.Content.Contains(EditableLevel.KLink))
            {
                for (var j = 0; j < EditableLevel.Colors.Length; j++)
                {
                    if (levelCell.Content.Contains(EditableLevel.Colors[j]))
                    {
                        levelCell.ChangeSprite(levelCell.LinkSprites[j], UserClickType.Extra);
                    }
                }
            }
            else if (levelCell.Content.Contains(EditableLevel.KLock))
            {
                for (var j = 0; j < EditableLevel.Colors.Length; j++)
                {
                    if (levelCell.Content.Contains(EditableLevel.Colors[j]))
                    {
                        levelCell.ChangeSprite(levelCell.LockSprites[j], UserClickType.Extra);
                    }
                }
            }
            else if (levelCell.Content.Contains(EditableLevel.KWall))
            {
                levelCell.ChangeSprite(levelCell.WallSprite, UserClickType.Extra);
            }
            else if (levelCell.Content.Contains(EditableLevel.KCrack))
            {
                levelCell.ChangeSprite(levelCell.CrackSprite, UserClickType.Extra);
            }
            else if (levelCell.Content.Contains(EditableLevel.KSlide))
            {
                for (var j = 0; j < EditableLevel.Colors.Length; j++)
                {
                    if (levelCell.Content.Contains(EditableLevel.Colors[j]))
                    {
                        levelCell.ChangeSprite(levelCell.SlideSprites[j], UserClickType.Extra);
                    }
                }
            }

            // TRAVERSAL MODIFIER
            if (levelCell.Content.Contains(EditableLevel.KTraversalStateCovered))
            {
                levelCell.ChangeSprite(levelCell.CoveredSprite, UserClickType.TraversalModifier);
            }
            else if (levelCell.Content.Contains(EditableLevel.KTraversalStateRevealed))
            {
                levelCell.ChangeSprite(levelCell.RevealedSprite, UserClickType.TraversalModifier);
            }
        }

        ToggleElement(true);

        _validationDirty = true;

        base.Show();
    }

    public override void Show(Panel originPanel = null)
    {
        ToggleElement(true);

        _validationDirty = true;

        for (var i = 0; i < _levelCells.Length; i++)
        {
            _levelCells[i].ResetCell();
        }

        CurrentUserLevel = new UserLevel
        {
            Guid = Guid.NewGuid().ToString(),
            UserId = MainManager.Instance.UserId,
            StartNode = -Vector2.one,
            EndNode = -Vector2.one,
            Elements = _levelCells.Select(x => x.Content).ToArray()
        };
        
        base.Show(originPanel);
    }

    public LevelCell GetCellByPosition(int x, int y)
    {
        var position = new Vector2Int(x, y);

        foreach (var levelCell in _levelCells)
        {
            if (levelCell.Position == position)
            {
                return levelCell;
            }
        }

        return null;
    }

    private void SaveLevel()
    {
        var output = MainManager.Instance.SaveLevelToDevice(CurrentUserLevel);
        
        // Mark it as uploaded only if successful, since we'll need to prune it out if it is removed from the database offline OR keep it if not in the database but on device

        MainManager.Instance.DatabaseReference.Child(CurrentUserLevel.Guid).SetRawJsonValueAsync(output).ContinueWith(x => {
            if (x.IsCompleted)
            {
                Debug.Log("level " + CurrentUserLevel.Guid + " uploaded");
                CurrentUserLevel.Uploaded = true;
                MainManager.Instance.DatabaseReference.Child(CurrentUserLevel.Guid).Child("Uploaded").SetValueAsync(true);
                MainManager.Instance.SaveLevelToDevice(CurrentUserLevel);
            }
        });
    }

    private string[] Validate()
    {
        if (!_validationDirty)
        {
            return null;
        }

        var errorList = new List<string>();

        var startCell = GetCellByPosition((int) CurrentUserLevel.StartNode.x, (int) CurrentUserLevel.StartNode.y);
        var endCell = GetCellByPosition((int) CurrentUserLevel.EndNode.x, (int) CurrentUserLevel.EndNode.y);

        if (startCell == null)
        {
            errorList.Add("No start node was found");
        }

        if (endCell == null)
        {
            errorList.Add("No end node was found");
        }
        
        CurrentUserLevel.Elements = _levelCells.Select(x => x.Content).ToArray();

        Solutions = new List<UserHypotheticalSolution>();

        var firstSolution = new UserHypotheticalSolution(CurrentUserLevel);
        firstSolution.Solve();

        _validationDirty = false;

        var minimumMoves = int.MaxValue;
        var minimumMovesWithFlag = int.MaxValue;

        if (Solutions.Count > 0)
        {
            for (var i = 0; i < Solutions.Count; i++)
            {
                if (Solutions[i].CollectedCollectables.Contains(EditableLevel.KFlag) &&
                    Solutions[i].Movements.Count < minimumMovesWithFlag)
                {
                    var lingeringFlag = false;

                    for (var j = 0; j < Solutions[i].Elements.Length; j++)
                    {
                        var element = Solutions[i].Elements[j];

                        if (element != null && element.Contains(EditableLevel.KFlag))
                        {
                            lingeringFlag = true;
                        }
                    }

                    if (!lingeringFlag)
                    {
                        minimumMovesWithFlag = Solutions[i].Movements.Count;
                    }
                }
                if (Solutions[i].Movements.Count < minimumMoves)
                {
                    minimumMoves = Solutions[i].Movements.Count;
                }
            }

            if(minimumMoves < int.MaxValue && minimumMovesWithFlag < int.MaxValue)
            {
                CurrentUserLevel.ExpectedMoves = minimumMoves / 2;
                CurrentUserLevel.MinimumMovesWithFlag = minimumMovesWithFlag / 2;
                CurrentUserLevel.ExpectedTime = Mathf.CeilToInt(minimumMoves / 2f);
                CurrentUserLevel.Valid = true;
                
                Debug.Log(string.Format("{0} solutions found, minimum: {1} moves, {2} moves with flag, {3} seconds",
                    Solutions.Count,
                    CurrentUserLevel.ExpectedMoves,
                    CurrentUserLevel.MinimumMovesWithFlag,
                    CurrentUserLevel.ExpectedTime)
                );

                var numberOfElements = 0;
                var numberOfFlags = 0;
                string[] mechanics = {
                    EditableLevel.KSlide,
                    EditableLevel.KWall,
                    EditableLevel.KCrack,
                    EditableLevel.KLock,
                    EditableLevel.KLink,
                    EditableLevel.KTraversalStateCovered,
                    EditableLevel.KTraversalStateRevealed
                };

                bool[] differentMechanics = new bool[mechanics.Length];
                var numberOfDifferentMechanics = 0;

                for (var i = 0; i < CurrentUserLevel.Elements.Length; i++)
                {
                    if (!string.IsNullOrEmpty(CurrentUserLevel.Elements[i]))
                    {
                        numberOfElements++;

                        if (CurrentUserLevel.Elements[i].Contains(EditableLevel.KFlag))
                        {
                            numberOfFlags++;
                        }

                        for (var j = 0; j < mechanics.Length; j++)
                        {
                            if (!differentMechanics[j] && CurrentUserLevel.Elements[i].Contains(mechanics[j]))
                            {
                                differentMechanics[j] = true;
                                numberOfDifferentMechanics++;
                            }
                        }
                    }
                }

                CurrentUserLevel.Difficulty =
                    numberOfDifferentMechanics * 10 +
                    minimumMoves * 5 +
                    minimumMovesWithFlag * 5 +
                    numberOfFlags * 10 +
                    numberOfElements;

                return null;
            }
        }
        
        CurrentUserLevel.ExpectedMoves = 0;
        CurrentUserLevel.MinimumMovesWithFlag = 0;
        CurrentUserLevel.Valid = false;
        CurrentUserLevel.Difficulty = 0;

        errorList.Add("No valid solution found");

        return errorList.Count == 0 ? null : errorList.ToArray();
    }
    
    private void ClickOnCell()
    {
        if (_longPressTimer <= 0)
        {
            return;
        }
        
        switch (_clickType)
        {
            case UserClickType.Base:

                _cellClicked.ChangeBase(_toggledBase);

                break;

            case UserClickType.Extra:

                _cellClicked.ChangeExtra(_toggledExtra);

                break;

            case UserClickType.TraversalModifier:

                _cellClicked.ChangeTraversalModifier(_toggledTraversalModifier);

                break;
        }

        _validationDirty = true;
    }

    private void OpenContextMenu(UserClickType clickType)
    {
        if (_cellClicked == null || string.IsNullOrEmpty(_cellClicked.Content))
        {
            return;
        }

        ContextMenu.Activate(_cellClicked);
    }

    private void CycleThroughBase(bool start = false)
    {
        var newIndex = 0;
        
        var baseToCycleThrough = new string[2];
        baseToCycleThrough[0] = KBase;
        baseToCycleThrough[1] = KDelete;

        var baseSprites = new[]
        {
            StartNodeSprite,
            DeleteSprite
        };
        
        if (!start)
        {
            if (!string.IsNullOrEmpty(_toggledBase))
            {
                for (var i = 0; i < baseToCycleThrough.Length; i++)
                {
                    if (baseToCycleThrough[i] == _toggledBase)
                    {
                        newIndex = i < baseToCycleThrough.Length - 1 ? i + 1 : 0;
                    }
                }
            }
        }
        
        _toggledBase = baseToCycleThrough[newIndex];
        ToggleElementButtonImage.sprite = baseSprites[newIndex];
    }

    private void CycleThroughExtra()
    {
        var newIndex = 0;

        var extraToCycleThrough = new string[EditableLevel.Collectables.Length + EditableLevel.Obstacles.Length + 1];
        EditableLevel.Collectables.CopyTo(extraToCycleThrough, 0);
        EditableLevel.Obstacles.CopyTo(extraToCycleThrough, EditableLevel.Collectables.Length);
        extraToCycleThrough[EditableLevel.Collectables.Length + EditableLevel.Obstacles.Length] = KDelete;

        var extraSprites = new[]
        {
            KeySprite,
            FlagSprite,
            LinkSprite,
            LockSprite,
            WallSprite,
            CrackSprite,
            SlideSprite,
            DeleteSprite
        };

        if (!string.IsNullOrEmpty(_toggledExtra))
        {
            for (var i = 0; i < extraToCycleThrough.Length; i++)
            {
                if (extraToCycleThrough[i] == _toggledExtra)
                {
                    newIndex = i < extraToCycleThrough.Length - 1 ? i + 1 : 0;
                }
            }
        }

        _toggledExtra = extraToCycleThrough[newIndex];
        ToggleExtraButtonImage.sprite = extraSprites[newIndex];
    }

    private void CycleThroughTraversalModifier()
    {
        var newIndex = 0;

        var traversalModifierToCycleThrough = new string[EditableLevel.TraversalStates.Length + 1];
        EditableLevel.TraversalStates.CopyTo(traversalModifierToCycleThrough, 0);
        traversalModifierToCycleThrough[EditableLevel.TraversalStates.Length] = KDelete;

        var traversalModifierSprites = new[]
        {
            CoveredSprite,
            RevealedSprite,
            DeleteSprite
        };

        if (!string.IsNullOrEmpty(_toggledTraversalModifier))
        {
            for (var i = 0; i < traversalModifierToCycleThrough.Length; i++)
            {
                if (traversalModifierToCycleThrough[i] == _toggledTraversalModifier)
                {
                    newIndex = i < traversalModifierToCycleThrough.Length - 1 ? i + 1 : 0;
                }
            }
        }

        _toggledTraversalModifier = traversalModifierToCycleThrough[newIndex];
        ToggleTraversalModifierButtonImage.sprite = traversalModifierSprites[newIndex];
    }
    
    public void UI_Reset()
    {
        foreach (var levelCell in _levelCells)
        {
            levelCell.Clear();
        }

        _validationDirty = true;
    }

    public void UI_Center()
    {
        // center the level according to the area occupied by its current elements

        _validationDirty = true;
    }

    public void UI_Delete()
    {
        if (Directory.Exists(MainManager.Instance.UserLevelPath) && File.Exists(MainManager.Instance.UserLevelPath + "/" + CurrentUserLevel.Guid + ".json"))
        {
            File.Delete(MainManager.Instance.UserLevelPath + "/" + CurrentUserLevel.Guid + ".json");
        }

        MainManager.Instance.DatabaseReference.Child(CurrentUserLevel.Guid).RemoveValueAsync();

        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
        UIManager.Instance.LevelEditorMenuPanel.Show();
    }

    public void UI_Clear()
    {
        _cellClicked.Clear();

        _validationDirty = true;
    }

    public void UI_ChangePathOrientation()
    {
        _cellClicked.ChangePathOrientation();

        _validationDirty = true;
    }

    public void UI_MakeStartNode()
    {
        if (CurrentUserLevel.StartNode != -Vector2.one)
        {
            var previousStartNode = GetCellByPosition((int)CurrentUserLevel.StartNode.x, (int)CurrentUserLevel.StartNode.y);
            if (previousStartNode != null && !string.IsNullOrEmpty(previousStartNode.Content) && previousStartNode.Content.Contains(EditableLevel.KNode))
            {
                previousStartNode.ChangeSprite(NodeSprite, UserClickType.Base);
            }

            if (CurrentUserLevel.EndNode == _cellClicked.Position)
            {
                CurrentUserLevel.EndNode = -Vector2.one;
            }
        }

        CurrentUserLevel.StartNode = _cellClicked.Position;
        _cellClicked.ChangeSprite(StartNodeSprite, UserClickType.Base);
        
        _validationDirty = true;
    }

    public void UI_MakeEndNode()
    {
        if (CurrentUserLevel.EndNode != -Vector2.one)
        {
            var previousEndNode = GetCellByPosition((int)CurrentUserLevel.EndNode.x, (int)CurrentUserLevel.EndNode.y);
            if (previousEndNode != null && !string.IsNullOrEmpty(previousEndNode.Content) && previousEndNode.Content.Contains(EditableLevel.KNode))
            {
                previousEndNode.ChangeSprite(NodeSprite, UserClickType.Base);
            }

            if (CurrentUserLevel.StartNode == _cellClicked.Position)
            {
                CurrentUserLevel.StartNode = -Vector2.one;
            }
        }

        CurrentUserLevel.EndNode = _cellClicked.Position;
        _cellClicked.ChangeSprite(EndNodeSprite, UserClickType.Base);

        _validationDirty = true;
    }

    public void UI_Info()
    {
        Validate();

        MessageBubble.SetActive(true);
        MessageBubbleBackground.SetActive(true);

        if (CurrentUserLevel.Valid)
        {
            BackMessageText.text = string.Format(
                "Your level is valid!\n\n" +
                "Minimum moves : {0}\n" +
                "Minimum time : {1}",
                CurrentUserLevel.ExpectedMoves,
                CurrentUserLevel.ExpectedTime
            );
        }
        else
        {
            BackMessageText.text = "Your level is invalid.";
        }

        MessageBubbleConfirmButton.SetActive(false);
        MessageBubbleCancelButton.SetActive(false);
        MessageBubbleSaveAndQuitButton.SetActive(false);
        MessageBubbleContinueButton.SetActive(true);

        ForceLayoutRebuilding(MessageBubble.GetComponent<RectTransform>());
    }

    public void UI_Save()
    {
        Validate();

        SaveLevel();

        MessageBubble.SetActive(true);
        MessageBubbleBackground.SetActive(true);
        BackMessageText.text = CurrentUserLevel.Valid ? SaveValidMessage : SaveInvalidMessage;

        MessageBubbleConfirmButton.SetActive(false);
        MessageBubbleCancelButton.SetActive(false);
        MessageBubbleSaveAndQuitButton.SetActive(false);
        MessageBubbleContinueButton.SetActive(true);

        ForceLayoutRebuilding(MessageBubble.GetComponent<RectTransform>());
    }

    public void UI_Back()
    {
        Validate();

        MessageBubble.SetActive(true);
        MessageBubbleBackground.SetActive(true);
        BackMessageText.text = CurrentUserLevel.Valid ? BackValidMessage : BackInvalidMessage;

        MessageBubbleConfirmButton.SetActive(true);
        MessageBubbleCancelButton.SetActive(true);
        MessageBubbleSaveAndQuitButton.SetActive(true);
        MessageBubbleContinueButton.SetActive(false);

        ForceLayoutRebuilding(MessageBubble.GetComponent<RectTransform>());
    }

    public void UI_BackAndSaveConfirmed()
    {
        Validate();
        SaveLevel();

        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
        UIManager.Instance.LevelEditorMenuPanel.Show();
    }

    public void UI_BackConfirmed()
    {
        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
        UIManager.Instance.LevelEditorMenuPanel.Show();
    }

    public void UI_BackCanceled()
    {
        MessageBubble.SetActive(false);
        MessageBubbleBackground.SetActive(false);
    }
    
    public void UI_ToggleElement()
    {
        ToggleElement();
    }

    private void ToggleElement(bool start = false)
    {
        if (_clickType == UserClickType.Base)
        {
            CycleThroughBase(start);
            return;
        }

        _clickType = UserClickType.Base;
        ToggleElementButton.GetComponent<Image>().color = Color.gray;
        ToggleExtraButton.GetComponent<Image>().color = Color.white;
        ToggleTraversalModifierButton.GetComponent<Image>().color = Color.white;
    }

    public void UI_ToggleExtra()
    {
        if (_clickType == UserClickType.Extra)
        {
            CycleThroughExtra();
            return;
        }

        _clickType = UserClickType.Extra;
        ToggleElementButton.GetComponent<Image>().color = Color.white;
        ToggleExtraButton.GetComponent<Image>().color = Color.gray;
        ToggleTraversalModifierButton.GetComponent<Image>().color = Color.white;
    }

    public void UI_ToggleTraversalModifier()
    {
        if (_clickType == UserClickType.TraversalModifier)
        {
            CycleThroughTraversalModifier();
            return;
        }

        _clickType = UserClickType.TraversalModifier;
        ToggleElementButton.GetComponent<Image>().color = Color.white;
        ToggleExtraButton.GetComponent<Image>().color = Color.white;
        ToggleTraversalModifierButton.GetComponent<Image>().color = Color.gray;
    }

    public void UI_OtherOptions()
    {
        ContextMenu.Activate(OtherOptionsButton);
    }
}
