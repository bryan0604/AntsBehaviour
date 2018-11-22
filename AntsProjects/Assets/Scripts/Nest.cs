using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public List<AntsIntermediate> Anntts;
    public List<Food> FoodAreInNest;
    public static Nest singleton;
    public float NestSize;
    public float X;
    public float Y;
    public float Z;
    public bool InsufficientFood;
    public Material SphereNest;
	
	void Start ()
    {
        singleton = this;

        X = transform.localScale.x/2;
        Y = transform.localScale.y/2;
        Z = transform.localScale.z/2;

        //InvokeRepeating("OnNestCheckSupply", 0f, 10f);
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            OnNestCheckSupply();
        }
    }

    public void OnNestCheckSupply()
    {
        Collider[] StuffsInNest = Physics.OverlapSphere(transform.position, NestSize);
        FoodAreInNest = new List<Food>();

        foreach (var item in StuffsInNest)
        {
            if(item.gameObject.tag == "Food")
            {
                FoodAreInNest.Add(item.GetComponent<Food>());
            }
        }

        if(FoodAreInNest.Count == 0)
        {
            InsufficientFood = true;
        }
        else
        {
            if (FoodAreInNest.Count < Mathf.RoundToInt(Anntts.Count / 2))
            {
                InsufficientFood = true;
            }
            else
            {
                InsufficientFood = false;
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, NestSize);
    }
}
