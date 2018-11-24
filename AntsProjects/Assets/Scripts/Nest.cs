using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Nest : MonoBehaviour
{
    public List<AntsIntermediate> Anntts;
    public List<Food> FoodAreInNest;
    public static Nest singleton;
    [SerializeField]
    public float NestSize;
    public float X;
    public float Y;
    public float Z;
    public bool InsufficientFood;
    public Material SphereNest;
    
    public bool UpdateSize;

    private void OnValidate()
    {
        NestSize = transform.localScale.z/2;
    }

    void Start ()
    {
        singleton = this;

        X = transform.localScale.x/2;
        Y = transform.localScale.y/2;
        Z = transform.localScale.z/2;

        //InvokeRepeating("OnNestCheckSupply", 0f, 5f);
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
        //Debug.LogWarning("Running Supply Check");
        Collider[] StuffsInNest = Physics.OverlapSphere(transform.position, NestSize);
        FoodAreInNest = new List<Food>();

        foreach (var item in StuffsInNest)
        {
            if(item.gameObject.tag == "Food")
            {
                Food food = item.GetComponent<Food>();

                if(food.isDepeleted||food.FoodCapacity <= 0)
                {

                }
                else
                {
                    if (food.IsInNest == false)
                    {

                    }
                    else
                    {
                        FoodAreInNest.Add(food);
                    }
                }   
            }
        }

        //if(FoodAreInNest.Count == 0)
        //{
        //    InsufficientFood = true;
        //}
        //else
        //{
        //    // 1 ant * 2 foods = required amount

        //    int Required_Amount_Foods = Anntts.Count * 1;

        //    if(FoodAreInNest.Count >= Required_Amount_Foods)
        //    {
        //        InsufficientFood = false;

        //        //Debug.LogWarning("Sufficient food in nest");
        //    }
        //    else
        //    {
        //        InsufficientFood = true;

        //        //Debug.LogWarning("Food need = " + (Required_Amount_Foods-FoodAreInNest.Count));
        //    }
        //}
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, NestSize);
    }
}
