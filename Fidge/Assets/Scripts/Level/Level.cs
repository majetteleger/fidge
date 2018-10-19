using System.Collections;
using System.Collections.Generic;
using geniikw.DataRenderer2D;
using UnityEngine;

public class Level : MonoBehaviour
{
    public enum KeyLockColor
    {
        Red,
        Green,
        Blue,
        Orange,
        Purple,
    }
    
    [Header("Prefabs")]
    public GameObject PlayerPrefab;
    public GameObject NodePrefab;
    public GameObject VerticalPathPrefab;
    public GameObject HorizontalPathPrefab;
    public GameObject EmptyElementPrefab;
    public GameObject KeyPrefab;
    public GameObject FlagPrefab;
    public GameObject LinkPrefab;
    public GameObject LockPrefab;
    public GameObject WallPrefab;
    public GameObject CrackPrefab;
    public GameObject SlidePrefab;
    public GameObject CoveredPrefab;
    public GameObject RevealedPrefab;
    public GameObject ModifierMaskPrefab;
    [Header("Containers")]
    public Transform NodesContainer;
    public Transform PathsContainer;
    public Transform EmptyElementsContainer;
    public Transform ShadowsContainer;
    public Color ShadowsColor;
    public Vector2 ShadowOffset;
    [Header("Editor")]
    public Sprite UpArrow;
    public Sprite RightArrow;
    public Sprite DownArrow;
    public Sprite LeftArrow;

    public bool UserMade { get; set; }
    public string Guid { get; set; }

    public bool Scripted { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public Node StartNode { get; set; }
    public Node EndNode { get; set; }
    public float ExpectedTime { get; set; }
    public int ExpectedMoves { get; set; }

    private string savedKey;
    private string _savedKey
    {
        get
        {
            if (string.IsNullOrEmpty(savedKey))
            {
                savedKey = UserMade ? Guid : "Level" + Index;
            }

            return savedKey;
        }
    }

    private string savedValue;
    private string _savedValue
    {
        get
        {
            var tempValue = PlayerPrefs.GetString(_savedKey);

            if (string.IsNullOrEmpty(tempValue))
            {
                PlayerPrefs.SetString(_savedKey, "000");
            }

            savedValue = PlayerPrefs.GetString(_savedKey);
            
            return savedValue;
        }
        set
        {
            PlayerPrefs.SetString(_savedKey, value);
        }
    }

    public static string GetSavedValue(int levelIndex)
    {
        return PlayerPrefs.GetString("Level" + levelIndex);
    }

    public static string GetSavedValue(string guid)
    {
        return PlayerPrefs.GetString(guid);
    }
    
    public bool TimeMedal
    {
        get { return _savedValue[0] == '1'; }
    }

    public bool MovesMedal
    {
        get { return _savedValue[1] == '1'; }
    }

    public bool FlagMedal
    {
        get { return _savedValue[2] == '1'; }
    }
    
    void Start()
    {
        UIManager.Instance.InGamePanel.UpdateTimer();
        UIManager.Instance.InGamePanel.UpdateMoves();
    }

    public void Save(bool timeMedal, bool movesMedal, bool flagMedal)
    {
        var pastValue = _savedValue;
        var currentValue = new char[pastValue.Length];

        bool[] boolValues = {timeMedal, movesMedal, flagMedal};

        for (var i = 0; i < currentValue.Length; i++)
        {
            currentValue[i] = pastValue[i] == '0' && boolValues[i] ? '1' : pastValue[i];
        }

        var newValue = new string(currentValue);

        _savedValue = newValue;

        MainManager.Instance.DirtyMedals = true;
    }

    public void Initiliaze(LevelEditPanel.UserLevel userLevel)
    {
        UserMade = true;
        Guid = userLevel.Guid;
        Name = userLevel.Guid;
        ExpectedTime = userLevel.ExpectedTime;
        ExpectedMoves = userLevel.ExpectedMoves;

        gameObject.name = Name;

        LinkElements<Node, Path>();
        LinkElements<Path, Node>();

        EndNode.GetComponent<SpriteRenderer>().sprite = NodePrefab.GetComponent<Node>().EndSprite;

        foreach (var element in GetComponentsInChildren<Element>())
        {
            if (element.State == Element.TraversalState.Revealed)
            {
                element.Cover();
            }
        }
    }

    public void Initiliaze(EditableLevel editableLevel)
    {
        Scripted = editableLevel.Scripted;
        Name = editableLevel.name;
        ExpectedTime = editableLevel.ExpectedTime;
        ExpectedMoves = editableLevel.ExpectedMoves;

        gameObject.name = Name;
        
        LinkElements<Node, Path>();
        LinkElements<Path, Node>();
        
        EndNode.GetComponent<SpriteRenderer>().sprite = NodePrefab.GetComponent<Node>().EndSprite;

        foreach (var element in GetComponentsInChildren<Element>())
        {
            if (element.State == Element.TraversalState.Revealed)
            {
                element.Cover();
            }
        }
        
        if (Application.isPlaying && Scripted)
        {
            foreach (var tutorialTag in editableLevel.ResetTutorials)
            {
                TutorialManager.Instance.SaveTutorial(tutorialTag, false);
            }
        }
    }
    
    private void LinkElements<E, L>() where E : MonoBehaviour where L : MonoBehaviour
    {
        var elements = GetComponentsInChildren<E>();

        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i].gameObject;

            var elementCollider = element.GetComponent<BoxCollider2D>();
            var otherColliders = Physics2D.OverlapBoxAll(element.transform.position, elementCollider.size, 0f);

            var leftLink = (L)null;
            var rightLink = (L)null;
            var upLink = (L)null;
            var downLink = (L)null;

            for (var j = 0; j < otherColliders.Length; j++)
            {
                var link = otherColliders[j].GetComponent<L>();

                if (element.GetComponent<E>() != null && link == null)
                {
                    continue;
                }

                var xDistance = link.transform.position.x - element.transform.position.x;
                var yDistance = link.transform.position.y - element.transform.position.y;

                if (yDistance == 0)
                {
                    if (xDistance < 0)
                    {
                        leftLink = link;
                    }
                    else if (xDistance > 0)
                    {
                        rightLink = link;
                    }
                }
                else if (xDistance == 0)
                {
                    if (yDistance < 0)
                    {
                        downLink = link;
                    }
                    else if (yDistance > 0)
                    {
                        upLink = link;
                    }
                }
            }

            var node = element.GetComponent<Node>();
            var path = element.GetComponent<Path>();

            if (node != null)
            {
                node.UpPath = upLink as VerticalPath;
                node.RightPath = rightLink as HorizontalPath;
                node.DownPath = downLink as VerticalPath;
                node.LeftPath = leftLink as HorizontalPath;
            }

            if (path != null)
            {
                path.UpNode = upLink as Node;
                path.RightNode = rightLink as Node;
                path.DownNode = downLink as Node;
                path.LeftNode = leftLink as Node;

                path.TryTurnToStubs();
            }
        }
    }
}
