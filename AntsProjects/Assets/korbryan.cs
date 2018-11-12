using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class korbryan : MonoBehaviour
{

    private void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Fart();
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            Shit();
        }
    }

    void Fart()
    {
        Debug.Log(transform.name + " is farting...");
    }

    void Shit()
    {
        Debug.Log(transform.name + " is shitting...");
    }
}
