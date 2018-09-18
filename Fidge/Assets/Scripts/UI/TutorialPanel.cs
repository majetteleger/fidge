using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using geniikw.DataRenderer2D;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : Panel
{
    public static TutorialPanel Instance = null;
    
    public GameObject AdvanceButton;
    public GameObject TutorialBubble;
    public UILine TutorialLine;
    public Image TutorialArrowHead;
    public Sprite ArrowUp;
    public Sprite ArrowDown;
    public Transform LineOriginUp;
    public Transform LineOriginDown;
    public Image TargetMask;
    public Sprite RoundMask;
    public Sprite LongMask;
    public Text TutorialText;
    public float TutorialLineWeight;
    public float TutorialLineOffset;
    public float TutorialBubbleDistance;

    private List<GameObject> _tempMasks;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        SetupSounds();
    }
    
    public void AdvanceTutorial()
    {
        if (_tempMasks != null)
        {
            foreach (var maskObject in _tempMasks)
            {
                Destroy(maskObject);
            }
        }
        
        var nextTutorial = TutorialManager.Instance.TutorialQueue.Count > 0 ? TutorialManager.Instance.TutorialQueue.Dequeue() : null;

        if (nextTutorial != null)
        {
            ShowTutorial(nextTutorial);
            return;
        }

        Hide();
    }

    public void UI_AdvanceTutorial()
    {
        AdvanceTutorial();
    }

    private void ShowTutorial(TutorialManager.Tutorial tutorial)
    {
        TutorialManager.Instance.SaveTutorial(tutorial.TargetTag, true);
        
        // Place the bubble

        var tagetIsUi = tutorial.Tagger.GetComponentInParent<Panel>();

        var targetPosition = !tagetIsUi
            ? Camera.main.WorldToScreenPoint(tutorial.Tagger.transform.position)
            : tutorial.Tagger.transform.position;

        var targetInUpperHalf = targetPosition.y > Screen.height / 2f;

        var endPosition = targetPosition;
        endPosition.y += targetInUpperHalf ? -TutorialLineOffset : TutorialLineOffset;
        endPosition.z = 0f;

        var tempBubblePosition = TutorialBubble.transform.position;
        tempBubblePosition.y = targetPosition.y + (targetInUpperHalf ? -TutorialBubbleDistance : TutorialBubbleDistance);
        TutorialBubble.transform.position = tempBubblePosition;
        TutorialBubble.GetComponent<RectTransform>().pivot = new Vector2(0.5f, targetInUpperHalf ? 1f : 0f);

        // Change the text

        TutorialText.text = tutorial.Text;

        // Place the arrow

        if (tutorial.NoArrow)
        {
            TutorialLine.gameObject.SetActive(false);
            TutorialArrowHead.gameObject.SetActive(false);
        }
        else
        {
            TutorialLine.gameObject.SetActive(true);
            TutorialArrowHead.gameObject.SetActive(true);

            var startPosition = targetInUpperHalf ? LineOriginUp.position : LineOriginDown.position;
            startPosition.z = 0f;

            var secondPosition = startPosition;
            secondPosition.y = (endPosition.y + startPosition.y) / 2f;

            var thirdPosition = endPosition;
            thirdPosition.y = (endPosition.y + startPosition.y) / 2f;

            TutorialLine.line.EditPoint(0, startPosition, TutorialLineWeight);
            TutorialLine.line.EditPoint(1, secondPosition, TutorialLineWeight);
            TutorialLine.line.EditPoint(2, thirdPosition, TutorialLineWeight);
            TutorialLine.line.EditPoint(3, endPosition, TutorialLineWeight);

            TutorialArrowHead.sprite = targetInUpperHalf ? ArrowUp : ArrowDown;
            TutorialArrowHead.transform.position = endPosition + new Vector3(0f, targetInUpperHalf ? 8f : -8f, 0f);
            TutorialArrowHead.GetComponent<Shadow>().effectDistance = new Vector2(4, targetInUpperHalf ? 2f : -2f);
        }
        
        // Highlight the target

        TargetMask.transform.position = targetPosition;

        switch (tutorial.TargetTag)
        {
            case TutorialManager.TutorialTag.Player:
            case TutorialManager.TutorialTag.EndNode:
            case TutorialManager.TutorialTag.Star:
            case TutorialManager.TutorialTag.Slide:
            case TutorialManager.TutorialTag.Crack:
            case TutorialManager.TutorialTag.Wall:
            case TutorialManager.TutorialTag.Lock:
            case TutorialManager.TutorialTag.Key:
            case TutorialManager.TutorialTag.Link:

                TargetMask.type = Image.Type.Simple;
                TargetMask.sprite = RoundMask;
                TargetMask.GetComponent<RectTransform>().sizeDelta = new Vector2(88f, 88f);

                break;

            case TutorialManager.TutorialTag.TraversalInput:
            case TutorialManager.TutorialTag.Time:
            case TutorialManager.TutorialTag.Moves:

                TargetMask.type = Image.Type.Sliced;
                TargetMask.sprite = LongMask;
                TargetMask.GetComponent<RectTransform>().sizeDelta = tutorial.Tagger.GetComponent<RectTransform>().sizeDelta + new Vector2(24f, 24f);

                break;

            case TutorialManager.TutorialTag.Covered:
            case TutorialManager.TutorialTag.Revealed:

                _tempMasks = new List<GameObject>();

                TargetMask.type = Image.Type.Sliced;
                TargetMask.sprite = LongMask;
                TargetMask.GetComponent<RectTransform>().sizeDelta = new Vector2(124f, 124f);

                foreach (var tagger in FindObjectsOfType<TutorialTagger>().Where(x => x.Tag == tutorial.TargetTag))
                {
                    if (tagger == tutorial.Tagger)
                    {
                        continue;
                    }

                    var newMask = Instantiate(TargetMask.gameObject).transform;
                    newMask.SetParent(TargetMask.transform.parent);
                    newMask.SetAsFirstSibling();
                    newMask.position = Camera.main.WorldToScreenPoint(tagger.transform.position);

                    _tempMasks.Add(newMask.gameObject);
                }

                break;
        }
        
        Show();
    }
}
