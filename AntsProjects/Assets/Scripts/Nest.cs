using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public List<AntsIntermediate> Anntts;

    public static Nest singleton;
    public float NestSize;
    public float X;
    public float Y;
    public float Z;
    public Material SphereNest;
	
	void Start ()
    {
        singleton = this;

        X = transform.localScale.x/2;
        Y = transform.localScale.y/2;
        Z = transform.localScale.z/2;

        NestCheck();
	}

    public void NestCheck()
    {
        //Collider[] ObjectInCollide = Physics.OverlapSphere(transform.position, NestSize);


    }

    public void AssignAntInfos(AntsIntermediate ant)
    {
        Anntts.Add(ant);

        int Number = 1;
        foreach (var _ant in Anntts)
        {
            _ant.transform.name = "Ant" + Number;

            Number++;
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, NestSize);
    }
}
