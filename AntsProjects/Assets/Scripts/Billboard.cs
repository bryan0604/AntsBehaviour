using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera MainCam;

	void Start ()
    {
	    if(MainCam==null)
        {
            MainCam = Camera.main;
        }
	}

	void FixedUpdate ()
    {
        if (MainCam == null) return;
      
        this.transform.LookAt(this.transform.position + MainCam.transform.rotation * Vector3.forward, MainCam.transform.rotation* Vector3.up);
	}
}
