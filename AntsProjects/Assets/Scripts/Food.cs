using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public bool IsInNest;
    public bool IsBeingCarried;


    private void OnTransformParentChanged()
    {
        Nest.singleton.NestCheck();
    }
}
