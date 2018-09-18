using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalPath : Path
{
    public Sprite DownStubSprite;
    public Sprite UpStubSprite;

    public override void TryTurnToStubs()
    {
        if (UpNode == null && DownNode == null)
        {
            ChangeSprite(BothStubSprite);
        }
        else if (UpNode == null)
        {
            ChangeSprite(UpStubSprite);
        }
        else if (DownNode == null)
        {
            ChangeSprite(DownStubSprite);
        }
    }
}
