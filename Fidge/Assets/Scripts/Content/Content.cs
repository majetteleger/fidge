using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Content : MonoBehaviour
{
    [Serializable]
    public struct ColoredSpriteCollection
    {
        public Sprite RedSprite;
        public Sprite BlueSprite;
        public Sprite GreenSprite;
        public Sprite OrangeSprite;
        public Sprite PurpleSprite;
    }

    [Serializable]
    public struct DirectedSpriteCollection
    {
        public Sprite UpSprite;
        public Sprite RightSprite;
        public Sprite DownSprite;
        public Sprite LeftSprite;
    }

    [Serializable]
    public struct OrientedSpriteCollection
    {
        public Sprite VerticalSprite;
        public Sprite HorizontalSprite;
    }

    public virtual void Contact() { }
}
