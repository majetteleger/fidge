using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Collectable
{
    public override void Contact()
    {
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.GetStar);

        base.Contact();
    }
}
