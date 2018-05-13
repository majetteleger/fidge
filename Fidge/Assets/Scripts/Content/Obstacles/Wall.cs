using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Obstacle
{
    public OrientedSpriteCollection Sprites;
    public OrientedSpriteCollection EditorSprites;

    public int Health { get; set; }

    private int _currentHealth;
    private Vector3 _playerOriginalPosition;

    private void Start()
    {
        Health = 1; // CHANGE IN EDITOR?
        _currentHealth = Health;
    }

    public override Node Resolve(Node currentTraversalNode, Node nextTraversalNode, TraversalManager.TraversalMove direction)
    {
        StartCoroutine(DoResolving());

        return currentTraversalNode;
    }

    public override IEnumerator HandleResolution()
    {
        yield return new WaitForSeconds(TraversalManager.Instance.TraversalSpeed / 2);
        
        if (_currentHealth <= 0)
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    private IEnumerator DoResolving()
    {
        _playerOriginalPosition = MainManager.Instance.Player.transform.position;

        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Wall);

        StartCoroutine(MovePlayer(_playerOriginalPosition, transform.position, TraversalManager.Instance.TraversalSpeed / 4));
        yield return new WaitForSeconds(TraversalManager.Instance.TraversalSpeed / 4);

        _currentHealth--;

        if (_currentHealth <= 0)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(MovePlayer(transform.position, _playerOriginalPosition, TraversalManager.Instance.TraversalSpeed / 4));
        yield return new WaitForSeconds(TraversalManager.Instance.TraversalSpeed / 4);
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
}
