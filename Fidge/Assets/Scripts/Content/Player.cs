using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Collectable[] GetCollectables()
    {
        var collectables = GetComponentsInChildren<Collectable>();

        return collectables;
    }
}
