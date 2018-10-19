using System.Collections;
using System.Collections.Generic;
using geniikw.DataRenderer2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraversalManager : MonoBehaviour
{
    public static TraversalManager Instance;
    
    public enum TraversalMove
    {
        Up,
        Right,
        Down,
        Left,
        NONE
    }

    public float MinTraversalSpeed;
    public float MaxTraversalSpeed;
    public float TraversalSpeedStep;
    public float ScriptedTraversalPlanningDelay;

    public bool IsPlanningTraversal { get; set; }
    public Coroutine Traversal { get; set; }
    public List<TraversalMove> TraversalMoves { get; set; }
    public float TraversalSpeed { get; set; }

    private int _currentTraversalMoves;
    private float _currentTraversalTime;
    private Node _currentTraversalNode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (MainManager.Instance.ActiveLevel != null)
        {
            if (Traversal == null)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    SimulateButtonPress(UIManager.Instance.InGamePanel.UpButton);
                }
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    SimulateButtonPress(UIManager.Instance.InGamePanel.RightButton);
                }
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    SimulateButtonPress(UIManager.Instance.InGamePanel.DownButton);
                }
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    SimulateButtonPress(UIManager.Instance.InGamePanel.LeftButton);
                }
                if (IsPlanningTraversal && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                {
                    SimulateButtonPress(UIManager.Instance.InGamePanel.GoButton);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTraversal();
                MainManager.Instance.ReloadLevel();
            }
        }
        
        if(IsPlanningTraversal)
        {
            _currentTraversalTime += Time.deltaTime;
            UIManager.Instance.InGamePanel.UpdateTimer(_currentTraversalTime);
        }
    }

    public void StartPlanningTraversal()
    {
        _currentTraversalMoves = 0;
        _currentTraversalTime = 0f;
        IsPlanningTraversal = true;
        TraversalMoves = new List<TraversalMove>();

        var elements = MainManager.Instance.ActiveLevel.GetComponentsInChildren<Element>();

        for (var i = 0; i < elements.Length; i++)
        {
            if (elements[i].State == Element.TraversalState.Covered)
            {
                elements[i].Cover();
            }
            else if (elements[i].State == Element.TraversalState.Revealed)
            {
                elements[i].Reveal();
            }
        }
    }

    public void ConfirmTraversal()
    {
        IsPlanningTraversal = false;
        Traversal = StartCoroutine(ExecuteTraversal());

        UIManager.Instance.InGamePanel.ToggleTraversalInputs(false);
    }

    public void CancelTraversal()
    {
        if (Traversal != null)
        {
            StopAllCoroutines();
            Traversal = null;
        }

        IsPlanningTraversal = false;

        if (TraversalMoves != null)
        {
            TraversalMoves.Clear();
        }
        
        _currentTraversalMoves = 0;
        _currentTraversalTime = 0;

        if(MainManager.Instance.ActiveLevel != null)
        {
            UIManager.Instance.InGamePanel.UpdateMoves(_currentTraversalMoves);
            UIManager.Instance.InGamePanel.UpdateTimer(_currentTraversalTime);
        }
    }

    public void RegisterTraversalMove(TraversalMove traversalMove)
    {
        if (!IsPlanningTraversal)
        {
            StartPlanningTraversal();
        }

        TraversalMoves.Add(traversalMove);
        _currentTraversalMoves++;

        UIManager.Instance.InGamePanel.UpdateMoves(_currentTraversalMoves);
    }

    /*public void SimulateTraversalPlanning(TraversalMove[] traversalScript)
    {
        StartCoroutine(DoSimulateTraversalPlanning(traversalScript));
    }*/
    
    private IEnumerator ExecuteTraversal()
    {
        TraversalSpeed = MaxTraversalSpeed;

        if (MainManager.Instance.ActiveLevel.Scripted)
        {
            yield return new WaitForSeconds(TraversalSpeed);
        }

        _currentTraversalNode = MainManager.Instance.ActiveLevel.StartNode;
        var traversal = TraversalMoves.ToArray();

        for(var i = 0; i < traversal.Length; i++)
        {
            var nextPath = (Path)null;
            var direction = Vector3.zero;

            switch (traversal[i])
            {
                case TraversalMove.Up:
                    nextPath = _currentTraversalNode.UpPath;
                    direction = Vector3.up;
                    break;

                case TraversalMove.Right:
                    nextPath = _currentTraversalNode.RightPath;
                    direction = Vector3.right;
                    break;

                case TraversalMove.Down:
                    nextPath = _currentTraversalNode.DownPath;
                    direction = Vector3.down;
                    break;

                case TraversalMove.Left:
                    nextPath = _currentTraversalNode.LeftPath;
                    direction = Vector3.left;
                    break;
            }

            var nextNode = (Node) null;

            if (nextPath != null)
            {
                var wall = nextPath.GetComponentInChildren<Wall>() != null;
                
                nextNode = nextPath.Traverse(traversal[i], _currentTraversalNode);

                if (wall)
                {
                    yield return new WaitForSeconds(TraversalSpeed);
                }
            }

            if (nextPath == null || nextNode == null)
            {
                StartCoroutine(MovePlayer(_currentTraversalNode.transform.position, _currentTraversalNode.transform.position + direction / 2, MaxTraversalSpeed, true));
                yield return new WaitForSeconds(MaxTraversalSpeed * 2);

                Fail();
                yield break;
            }

            if (_currentTraversalNode == nextNode)
            {
                continue;
            }

            StartCoroutine(MovePlayer(_currentTraversalNode.transform.position, nextNode.transform.position, TraversalSpeed, nextPath.GetObstacle() == null));
            yield return new WaitForSeconds(TraversalSpeed);

            MainManager.Instance.Player.transform.SetParent(nextNode.transform, true);

            var content = nextNode.GetComponentInChildren<Content>();

            if (content != null && content.transform.parent == nextNode.transform)
            {
                content.Contact();
            }
            
            _currentTraversalNode = nextNode;
            
            var link = _currentTraversalNode.GetComponentInChildren<Link>();

            if (link != null)
            {
                var links = FindObjectsOfType<Link>();

                for (var j = 0; j < links.Length; j++)
                {
                    if (links[j] != link && links[j].Color == link.Color)
                    {
                        nextNode = links[j].transform.parent.GetComponent<Node>();

                        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.TeleportLink);
                        StartCoroutine(MovePlayer(_currentTraversalNode.transform.position, nextNode.transform.position, TraversalSpeed, false));
                        yield return new WaitForSeconds(TraversalSpeed);

                        MainManager.Instance.Player.transform.SetParent(nextNode.transform, true);

                        _currentTraversalNode = nextNode;

                        break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(TraversalSpeed);

        CheckForCompletion();
    }
    
    private IEnumerator MovePlayer(Vector3 sourcePosition, Vector3 targetPosition, float time, bool freeMove)
    {
        if (Vector3.Distance(sourcePosition, targetPosition) < 0.1f)
        {
            Debug.Log("wat");
        }

        if (freeMove)
        {
            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.PlayerMove);
        }
        
        var startTime = Time.time;

        while (Time.time < startTime + time)
        {
            var step = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - startTime) / time);
            MainManager.Instance.Player.transform.position = Vector3.Lerp(sourcePosition, targetPosition, step);

            yield return null;
        }

        MainManager.Instance.Player.transform.position = targetPosition;

        if (TraversalSpeed > MinTraversalSpeed)
        {
            TraversalSpeed -= TraversalSpeedStep;
        }
    }

    /*private IEnumerator DoSimulateTraversalPlanning(TraversalMove[] traversalScript)
    {
        yield return new WaitForSeconds(ScriptedTraversalPlanningDelay);

        for (var i = 0; i < traversalScript.Length; i++)
        {
            switch (traversalScript[i])
            {
                case TraversalMove.Up:
                    SimulateButtonPress(InGamePanel.Instance.UpButton);
                    break;
                case TraversalMove.Right:
                    SimulateButtonPress(InGamePanel.Instance.RightButton);
                    break;
                case TraversalMove.Down:
                    SimulateButtonPress(InGamePanel.Instance.DownButton);
                    break;
                case TraversalMove.Left:
                    SimulateButtonPress(InGamePanel.Instance.LeftButton);
                    break;
            }

            yield return new WaitForSeconds(TraversalSpeed);
        }
        
        SimulateButtonPress(InGamePanel.Instance.GoButton);
    }*/

    private void SimulateButtonPress(Button button)
    {
        var pointer = new PointerEventData(EventSystem.current);
        
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.submitHandler);
    }

    private void CheckForCompletion()
    {
        var timeMedal = MainManager.Instance.ActiveLevel.Scripted || _currentTraversalTime <= MainManager.Instance.ActiveLevel.ExpectedTime;
        var movesMedal = MainManager.Instance.ActiveLevel.Scripted || _currentTraversalMoves <= MainManager.Instance.ActiveLevel.ExpectedMoves;
        var flagMedal = MainManager.Instance.ActiveLevel.Scripted;

        if (_currentTraversalNode == MainManager.Instance.ActiveLevel.EndNode)
        {
            Clear(timeMedal, movesMedal, flagMedal);
        }
        else
        {
            Fail();
        }
    }

    private void Fail()
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.LevelFailed);

        CancelTraversal();

        MainManager.Instance.ReloadLevel();

        UIManager.Instance.InGamePanel.ToggleTraversalInputs(true);
    }

    private void Clear(bool timeMedal, bool movesMedal, bool flagMedal)
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.LevelCleared);

        var flags = FindObjectsOfType<Flag>();

        for (var i = 0; i < flags.Length; i++)
        {
            if (flags[i].transform.parent != MainManager.Instance.Player.transform)
            {
                flagMedal = false;
                break;
            }

            flagMedal = true;
        }
        
        MainManager.Instance.ActiveLevel.Save(timeMedal, movesMedal, flagMedal);

        if (MainManager.Instance.ActiveLevel.UserMade)
        {
            UIManager.Instance.PopupPanel.ShowWon(MainManager.Instance.ActiveLevel.Guid);
        }
        else
        {
            UIManager.Instance.PopupPanel.ShowWon(MainManager.Instance.ActiveLevel.Index >= 0 ? MainManager.Instance.Levels[MainManager.Instance.ActiveLevel.Index].Level : null);
        }
        
        Destroy(MainManager.Instance.ActiveLevel.gameObject);
    }
}
