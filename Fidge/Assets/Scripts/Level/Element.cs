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
    
    private SpriteRenderer _spriteRenderer
    {
        get
        {
            if (_bSpriteRenderer == null)
            {
                _bSpriteRenderer = GetComponent<SpriteRenderer>();
            }

            return _bSpriteRenderer;
        }
    }

    private SpriteRenderer _bSpriteRenderer;

    public virtual void Cover()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].GetComponent<Content>() || renderers[i].GetComponent<Element>() || renderers[i].GetComponent<Covered>())
            {
                renderers[i].enabled = false;
            }
        }
    }

    public virtual void Reveal()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].GetComponent<Content>() || renderers[i].GetComponent<Element>())
            {
                renderers[i].enabled = true;
            }

            if (renderers[i].GetComponent<Revealed>())
            {
                renderers[i].enabled = false;
            }
        }
    }

    public void ChangeSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }
}
