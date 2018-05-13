using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public const string KSavedTutorials = "SavedTutorials";

    public enum TutorialTag
    {
        Player,
        EndNode,
        TraversalInput,
        Time,
        Moves,
        Star,
        Slide,
        Crack,
        Wall,
        Lock,
        Key,
        Link,
        Covered,
        Revealed
    }

    [Serializable]
    public class Tutorial
    {
        [TextArea]
        public string Text;
        public TutorialTag TargetTag;
        public bool NoArrow;

        public TutorialTagger Tagger { get; set; }
    }

    public static TutorialManager Instance;
    
    public Tutorial[] Tutorials;
    
    public Queue<Tutorial> TutorialQueue { get; set; }

    public bool ListeningForTutorialChecks
    {
        get
        {
            return _listeningForTutorialChecks;
        }
        set
        {
            _listeningForTutorialChecks = value;

            if (_listeningForTutorialChecks)
            {
                return;
            }

            ReorderTutorials();
            TutorialPanel.Instance.AdvanceTutorial();
        }
    }

    public string SavedTutorials
    {
        get
        {
            var savedTutorials = PlayerPrefs.GetString(KSavedTutorials);

            if (string.IsNullOrEmpty(savedTutorials) || savedTutorials.Length != Tutorials.Length)
            {
                savedTutorials = "";
                for (var j = 0; j < Tutorials.Length; j++)
                {
                    savedTutorials += '0';
                }
            }

            return savedTutorials;
        }
    }

    private bool _listeningForTutorialChecks;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        TutorialQueue = new Queue<Tutorial>();
    }

    public void QueueTutorial(Tutorial tutorial)
    {
        if (!ListeningForTutorialChecks)
        {
            return;
        }

        TutorialQueue.Enqueue(tutorial);
    }

    public void SaveTutorial(TutorialTag savedTag, bool saveValue)
    {
        var tagIndex = (int)savedTag;

        if (SavedTutorials[tagIndex] == (saveValue ? '0' : '1'))
        {
            var beforeString = SavedTutorials.Substring(0, tagIndex);
            var afterString = SavedTutorials.Substring(tagIndex + 1, SavedTutorials.Length - (tagIndex + 1));

            PlayerPrefs.SetString(KSavedTutorials, beforeString + (saveValue ? '1' : '0') + afterString);
        }
    }

    private void ReorderTutorials()
    {
        TutorialQueue = new Queue<Tutorial>(TutorialQueue.ToArray().OrderBy(d => d.TargetTag));
    }
}
