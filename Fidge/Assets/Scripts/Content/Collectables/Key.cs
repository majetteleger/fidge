using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Collectable
{
    public Sprite RedSprite;
    public Sprite BlueSprite;
    public Sprite GreenSprite;
    public Sprite CyanSprite;
    public Sprite MagentaSprite;
    public Sprite YellowSprite;

    public Level.KeyLockColor Color { get; set; }
}
