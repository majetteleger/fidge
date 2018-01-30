using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element : MonoBehaviour
{
    public enum TraversalState
    {
        Normal,
        Covered,
        Revealed
    }
    
    public TraversalState State { get; set; }

    public void Cover()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].GetComponent<Content>() || renderers[i].GetComponent<Element>() || renderers[i].GetComponent<TraversalStateModifier>())
            {
                renderers[i].enabled = false;
            }
        }
    }

    public void Reveal()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].GetComponent<Content>() || renderers[i].GetComponent<Element>())
            {
                renderers[i].enabled = true;
            }

            if (renderers[i].GetComponent<TraversalStateModifier>())
            {
                renderers[i].enabled = false;
            }
        }
    }
}
