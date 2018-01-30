using System.Collections;
using System.Collections.Generic;
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

    public float TraversalExecutionDelay;
    public float ScriptedTraversalPlanningDelay;

    public bool IsPlanningTraversal { get; set; }

    private int _currentTraversalMoves;
    private float _currentTraversalTime;
    private List<TraversalMove> _traversalMoves;
    private Node _currentTraversalNode;
    private Coroutine _traversal;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (MainManager.Instance.ActiveLevel != null && !MainManager.Instance.ActiveLevel.Scripted)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                SimulateButtonPress(InGamePanel.instance.UpButton);
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SimulateButtonPress(InGamePanel.instance.RightButton);
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                SimulateButtonPress(InGamePanel.instance.DownButton);
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SimulateButtonPress(InGamePanel.instance.LeftButton);
            }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                SimulateButtonPress(InGamePanel.instance.GoButton);
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
            InGamePanel.instance.UpdateTimer(_currentTraversalTime);
        }
    }

    public void StartPlanningTraversal()
    {
        _currentTraversalMoves = 0;
        _currentTraversalTime = 0f;
        IsPlanningTraversal = true;
        _traversalMoves = new List<TraversalMove>();

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
        _traversal = StartCoroutine(ExecuteTraversal());
    }

    public void CancelTraversal()
    {
        if (_traversal != null)
        {
            StopAllCoroutines();
        }

        IsPlanningTraversal = false;

        if (_traversalMoves != null)
        {
            _traversalMoves.Clear();
        }
        
        _currentTraversalMoves = 0;
        _currentTraversalTime = 0;

        InGamePanel.instance.UpdateMoves(_currentTraversalMoves);
        InGamePanel.instance.UpdateTimer(_currentTraversalTime);
    }

    public void RegisterTraversalMove(TraversalMove traversalMove)
    {
        if (!IsPlanningTraversal)
        {
            StartPlanningTraversal();
        }

        _traversalMoves.Add(traversalMove);
        _currentTraversalMoves++;

        InGamePanel.instance.UpdateMoves(_currentTraversalMoves);
    }

    public void SimulateTraversalPlanning(TraversalMove[] traversalScript)
    {
        StartCoroutine(DoSimulateTraversalPlanning(traversalScript));
    }

    private IEnumerator ExecuteTraversal()
    {
        if (MainManager.Instance.ActiveLevel.Scripted)
        {
            yield return new WaitForSeconds(TraversalExecutionDelay);
        }

        _currentTraversalNode = MainManager.Instance.ActiveLevel.StartNode;
        var traversal = _traversalMoves.ToArray();

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
                nextNode = nextPath.Traverse(traversal[i], _currentTraversalNode);
            }

            if (nextPath == null || nextNode == null)
            {
                StartCoroutine(MovePlayer(_currentTraversalNode.transform.position, _currentTraversalNode.transform.position + direction / 2, TraversalExecutionDelay));
                yield return new WaitForSeconds(TraversalExecutionDelay * 2);

                Fail();
                yield break;
            }
            
            StartCoroutine(MovePlayer(_currentTraversalNode.transform.position, nextNode.transform.position, TraversalExecutionDelay));
            yield return new WaitForSeconds(TraversalExecutionDelay);

            MainManager.Instance.Player.transform.SetParent(nextNode.transform, true);

            var content = nextNode.GetComponentInChildren<Content>();

            if (content != null)
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

                        StartCoroutine(MovePlayer(_currentTraversalNode.transform.position, nextNode.transform.position, TraversalExecutionDelay));
                        yield return new WaitForSeconds(TraversalExecutionDelay);

                        MainManager.Instance.Player.transform.SetParent(nextNode.transform, true);

                        _currentTraversalNode = nextNode;

                        break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(TraversalExecutionDelay);

        CheckForCompletion();
    }
    
    private IEnumerator MovePlayer(Vector3 sourcePosition, Vector3 targetPosition, float time)
    {
        var startTime = Time.time;

        while (Time.time < startTime + time)
        {
            var step = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - startTime) / time);
            MainManager.Instance.Player.transform.position = Vector3.Lerp(sourcePosition, targetPosition, step);

            yield return null;
        }

        MainManager.Instance.Player.transform.position = targetPosition;
    }

    private IEnumerator DoSimulateTraversalPlanning(TraversalMove[] traversalScript)
    {
        yield return new WaitForSeconds(ScriptedTraversalPlanningDelay);

        for (var i = 0; i < traversalScript.Length; i++)
        {
            switch (traversalScript[i])
            {
                case TraversalMove.Up:
                    SimulateButtonPress(InGamePanel.instance.UpButton);
                    break;
                case TraversalMove.Right:
                    SimulateButtonPress(InGamePanel.instance.RightButton);
                    break;
                case TraversalMove.Down:
                    SimulateButtonPress(InGamePanel.instance.DownButton);
                    break;
                case TraversalMove.Left:
                    SimulateButtonPress(InGamePanel.instance.LeftButton);
                    break;
            }

            yield return new WaitForSeconds(TraversalExecutionDelay);
        }
        
        SimulateButtonPress(InGamePanel.instance.GoButton);
    }

    private void SimulateButtonPress(Button button)
    {
        var pointer = new PointerEventData(EventSystem.current);
        
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.submitHandler);
    }

    private void CheckForCompletion()
    {
        if (_currentTraversalNode == MainManager.Instance.ActiveLevel.EndNode)
        {
            Clear();
        }
        else
        {
            Fail();
        }
    }

    private void Fail()
    {
        PopupPanel.instance.ShowLost(MainManager.Instance.ActiveLevel.Index);

        Destroy(MainManager.Instance.ActiveLevel.gameObject);
    }

    private void Clear()
    {
        var timeMedal = MainManager.Instance.ActiveLevel.Scripted || _currentTraversalTime <= MainManager.Instance.ActiveLevel.ExpectedTime;
        var movesMedal = MainManager.Instance.ActiveLevel.Scripted || _currentTraversalMoves <= MainManager.Instance.ActiveLevel.ExpectedMoves;
        var flagMedal = MainManager.Instance.ActiveLevel.Scripted;
        
        var flags = FindObjectsOfType<Flag>();

        for (var i = 0; i < flags.Length; i++)
        {
            if (flags[i].transform.parent != MainManager.Instance.Player.transform)
            {
                break;
            }

            flagMedal = true;
        }
        
        MainManager.Instance.ActiveLevel.Save(timeMedal, movesMedal, flagMedal);

        PopupPanel.instance.ShowWon(MainManager.Instance.LastLoadedLevelIndex);
        
        Destroy(MainManager.Instance.ActiveLevel.gameObject);
    }
}
