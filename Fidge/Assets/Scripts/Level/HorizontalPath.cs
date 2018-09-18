using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalPath : Path
{
    public Sprite LeftStubSprite;
    public Sprite RightStubSprite;

    public override void TryTurnToStubs()
    {
        if (LeftNode == null && RightNode == null)
        {
            ChangeSprite(BothStubSprite);
        }
        else if (LeftNode == null)
        {
            ChangeSprite(LeftStubSprite);
        }
        else if (RightNode == null)
        {
            ChangeSprite(RightStubSprite);
        }
    }
}

