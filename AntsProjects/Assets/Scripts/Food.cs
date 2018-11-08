using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public float smoothSpeed;

    public int FoodCapacity = 30;

    public bool ActivateVacuum;
    public bool IsInNest;
    public bool IsBeingCarried;
    public bool isSetOnCarriedPosition;
    public bool isBeingPlaced;
    public bool isDepeleted;

    public string VoidFoodDecay = "FoodDecay";

    public Material myMats;
    public Material decaymats;

    public Transform Ant;
    public Transform MainCarryAnt;

    public Vector3 offset;

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

        if(ActivateVacuum)
        {
            if(Ant!=null)
            {
                Vector3 desiredPos = Ant.transform.position + offset;

                Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.smoothDeltaTime);

                transform.position = smoothedPos;

                if (!isSetOnCarriedPosition)
                {
                    IsBeingCarried = true;

                    if (transform.position == Ant.transform.position)
                    {
                        isSetOnCarriedPosition = true;

                        //Debug.Log("A");
                    }
                }
            }
        }
    }

    public void ConsumingFood(int _Quantity)
    {
        FoodCapacity -= _Quantity;

        if (FoodCapacity <= 0)
        {
            transform.name = "Decayed Food";

            GetComponent<Renderer>().material = decaymats;

            isDepeleted = true;

            //GetComponent<BoxCollider>().enabled = false;

            //Destroy food to smaller pieces? for reproduction?

            //Destroy(this);

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

    public void MoveTowards(Transform ant)
    {
        Ant = ant;

        ActivateVacuum = true;
    }
}
