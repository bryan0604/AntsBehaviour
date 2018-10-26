﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public List<Ants> Anntts;

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
	}

    public void NestCheck()
    {
        Collider[] ObjectInCollide = Physics.OverlapSphere(transform.position, NestSize);

        for (int i = 0; i < ObjectInCollide.Length; i++)
        {
            //Debug.Log(ObjectInCollide[i].name);
            if (ObjectInCollide[i].gameObject.tag == "Ants")
            {
                //Ants Ant = ObjectInCollide[i].GetComponent<Ants>();

                //Ant.isInNestZone = true;
            }
            else if(ObjectInCollide[i].gameObject.tag == "Food")
            {
                Food Foodie = ObjectInCollide[i].GetComponent<Food>();

                Foodie.IsInNest = true;
            }
        }
    }

    public void AssignAntInfos(Ants ant)
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
