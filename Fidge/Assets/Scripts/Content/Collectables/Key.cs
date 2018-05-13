using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Collectable
{
    public ColoredSpriteCollection Sprites;
    public ColoredSpriteCollection EditorSprites;

    public Level.KeyLockColor Color { get; set; }

    public override void Contact()
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.GetKey);

        base.Contact();
    }
}
