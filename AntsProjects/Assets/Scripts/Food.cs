using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int FoodCapacity = 30;


    public bool IsInNest;
    public bool IsBeingCarried;
    public string VoidFoodDecay = "FoodDecay";

    public Material myMats;
    public Material decaymats;

    private void Start()
    {
        myMats = GetComponent<Material>();
    }

    private void OnTransformParentChanged()
    {
        Nest.singleton.NestCheck();
    }

    private void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            FoodCapacity -= 1;
        }
    }

    public void ConsumingFood(int _Quantity)
    {
        FoodCapacity -= _Quantity;

        if (FoodCapacity <= 0)
        {
            //transform.tag = "DecayFood";
            transform.name = "Decayed Food";
            GetComponent<Renderer>().material = decaymats;
            //Destroy food to smaller pieces? for reproduction?

            Invoke(VoidFoodDecay, 5f);
        }
        else
        {
            
            
            
        }
        

        
    }
     
    void FoodDecay()
    {
        Destroy(gameObject);
    }
}
