using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTagger : MonoBehaviour
{
    public TutorialManager.TutorialTag Tag;
    
    public void CheckForTutorial()
    {
        foreach (var queuedTutorial in TutorialManager.Instance.TutorialQueue)
        {
            if (queuedTutorial.TargetTag == Tag)
            {
                return;
            }
        }

        for (var i = 0; i < TutorialManager.Instance.Tutorials.Length; i++)
        {
            var tutorial = TutorialManager.Instance.Tutorials[i];
            var savedTutorials = TutorialManager.Instance.SavedTutorials;

            if (tutorial.TargetTag == Tag && savedTutorials[i] == '0')
            {
                tutorial.Tagger = this;
                TutorialManager.Instance.QueueTutorial(tutorial);
            }
        }
    }
}
