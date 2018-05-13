using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadow : MonoBehaviour
{
    public int ShadowLayer;

    private Level _level;
    private Transform _shadowsContainer;
    private SpriteRenderer _sourceSpriteRenderer;
    private SpriteRenderer _shadowSpriteRenderer;
    private GameObject _shadow;
    
    void Start()
    {
        if (MainManager.Instance == null || MainManager.Instance.ActiveLevel == null)
        {
            Destroy(this);
            return;
        }

        _level = MainManager.Instance.ActiveLevel;
        _shadowsContainer = _level.ShadowsContainer;
        _sourceSpriteRenderer = GetComponent<SpriteRenderer>();
        
        _shadow = Instantiate(gameObject, _shadowsContainer);
        _shadowSpriteRenderer = _shadow.GetComponent<SpriteRenderer>();
        _shadowSpriteRenderer.color = _level.ShadowsColor;
        _shadowSpriteRenderer.sortingOrder = ShadowLayer;

        _shadow.transform.position = transform.position + new Vector3(_level.ShadowOffset.x, _level.ShadowOffset.y, 0);

        var shadowChilren = _shadow.GetComponentsInChildren<Transform>();

        foreach (var child in shadowChilren)
        {
            if (child.transform == _shadow.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        foreach (var component in _shadow.GetComponents<Component>())
        {
            if (component == _shadowSpriteRenderer || component == _shadow.transform)
            {
                continue;
            }

            Destroy(component);
        }
    }
    
    void Update()
    {
        if (transform.hasChanged)
        {
            _shadow.transform.position = transform.position + new Vector3(_level.ShadowOffset.x, _level.ShadowOffset.y, 0);
            _shadow.transform.rotation = transform.rotation;
            _shadow.transform.localScale = transform.localScale;

            transform.hasChanged = false;
        }

        if (_shadowSpriteRenderer.sprite != _sourceSpriteRenderer.sprite)
        {
            _shadowSpriteRenderer.sprite = _sourceSpriteRenderer.sprite;
        }

        if (!_sourceSpriteRenderer.enabled && _shadowSpriteRenderer.enabled)
        {
            _shadowSpriteRenderer.enabled = false;
        }
        else if (_sourceSpriteRenderer.enabled && !_shadowSpriteRenderer.enabled)
        {
            _shadowSpriteRenderer.enabled = true;
        }
    }

    void OnDestroy()
    {
        if (_shadow == null)
        {
            return;
        }

        Destroy(_shadow.gameObject);
    }
}
