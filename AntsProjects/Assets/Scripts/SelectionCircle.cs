using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCircle : MonoBehaviour
{
    public Transform ImBelongTo;

    private void FixedUpdate()
    {
        Debug.DrawLine(transform.position, ImBelongTo.position, Color.green);
    }

    private void OnTriggerEnter(Collider Kaw)
    {
        if(Kaw.transform == ImBelongTo)
        {
            Destroy(this.gameObject);
        }
    }

}
