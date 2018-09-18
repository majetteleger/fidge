using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link : Content
{
    public ColoredSpriteCollection Sprites;
    public ColoredSpriteCollection EditorSprites;

    public Level.KeyLockColor Color { get; set; }
}
