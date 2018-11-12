using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Works
//1. Queen wander only in nest
//2. Feeding Mechanics
//3. Step after Feeding
#endregion

public class AntsQueen : MonoBehaviour
{
    public float StopDistance;
    public float WanderRange;
    public float WanderTime;
    public float IdleTime;
    public float MovementSpeed;
    public float StomachCapacity;
    public float StomachHungryRange;
    public float DigestionTime;
    public float DigestAmount;
    public float SmellFoodRange;

    private float _IdleTime;

    public int BirthCondition;
    public int BirthIncrement;

    public bool isHungry;
    public bool isIdle;
    public bool isFeeding;
    public bool isInNest;
    public bool isWandering;
    public bool isMoving;
    public bool isInteracting;
    public bool isHeadingToFood;

    public Transform SelectionCircle;
    public Transform InteractionPoint;
    public Transform LayEggPoint;
    public Transform MainTarget;

    public NavMeshAgent QueenWanda;

    private void Start()
    {
        QueenWanda.speed = MovementSpeed;
        _IdleTime = IdleTime;

        Invoke("Wander", 1f);

        InvokeRepeating("Digestion", DigestionTime, DigestAmount);
    }

    private void FixedUpdate()
    { 
        if(QueenWanda.hasPath)
        {
            if (QueenWanda.remainingDistance < StopDistance)
            {
                QueenWanda.ResetPath();

                if (isWandering)
                {
                    StartCoroutine(Idle());
                }
            }
        }
        
    }

    void Interaction()
    {
        Debug.Log(transform.name + " checking Interaction");

        Movement(false);

        if(!isInteracting)
        {
            isInteracting = true;
        }
        else
        {
            return;
        }
    }

    private IEnumerator Idle()
    {
        Debug.Log(transform.name + " is Idling");

        isIdle = true;

        isWandering = false;

        Movement(false);

        yield return new WaitForSeconds(IdleTime);

        Condition();
    }

    void Condition()
    {
        Debug.Log(transform.name + " is checking condition");

        if (isHungry)
        {
            BirthCondition -= BirthIncrement;

            LookForFood();
        }
        else
        {
            BirthCondition += BirthIncrement;

            Wander();
        }
    }


    void LookForFood()
    {
        Debug.Log(transform.name + " is looking for food");

        List<Food> AvailableFoods = new List<Food>();

        Collider[] Collins = Physics.OverlapSphere(transform.position, SmellFoodRange);

        bool FoodisFound = false;

        foreach (var Object in Collins)
        {
            if(Object.gameObject.tag == "Food")
            {
                if(Object.GetComponent<Food>())
                {
                    Food food = Object.GetComponent<Food>();

                    if(food.IsInNest)
                    {
                        AvailableFoods.Add(food);

                        FoodisFound = true;
                    }
                }
            }
        }


        if(FoodisFound)
        {
            if(AvailableFoods.Count >0)
            {
                Movement(true);

                isHeadingToFood = FoodisFound;

                Transform ChosenOne = AvailableFoods[Random.Range(0, AvailableFoods.Count)].transform;

                MainTarget = ChosenOne;

                QueenWanda.SetDestination(MainTarget.position);
            }
        }
        else
        {
            Debug.Log(transform.name + " food not found!");

            Wander();
        }
    }

    void Wander()
    {
        Debug.Log(transform.name + " is wandering");

        float AreaX, AreaZ;

        AreaX = Random.Range(-(WanderRange), (WanderRange));

        AreaZ = Random.Range(-(WanderRange), (WanderRange));

        Vector3 V = new Vector3(transform.position.x + AreaX, 0.1f, transform.position.z + AreaZ);

        Collider[] WullyWollys = Physics.OverlapSphere(V , transform.localScale.z);

        bool isAbleToMove = true;

        foreach (var item in WullyWollys)
        {
            if(item.transform.tag == "Food")
            {
                isAbleToMove = false;
            }
            else if (item.transform.tag == "Ants")
            {
                isAbleToMove = false;
            }
        }
        
        if(isAbleToMove)
        {
            Movement(true);

            Vector3 scales = new Vector3(transform.localScale.x * 2, 0.01f, transform.localScale.z * 2);

            GameObject _SelectionCircle = Instantiate(SelectionCircle.gameObject, V, Quaternion.identity);

            _SelectionCircle.transform.localScale = scales;

            _SelectionCircle.GetComponent<SelectionCircle>().ImBelongTo = transform;

            QueenWanda.SetDestination(V);

            isWandering = true;
        }
        else
        {
            Debug.DrawLine(transform.position, V, Color.red, 1f);

            Invoke("Wander", 0.1f);
        }
    }

    void Movement(bool Activate)
    {
        Debug.Log(transform.name + " movement speed = " + Activate);

        if (Activate)
        {
            QueenWanda.speed = MovementSpeed;
            isMoving = true;
        }
        else
        {
            QueenWanda.speed = 0;
            QueenWanda.velocity = Vector3.zero;
            isMoving = false;
        }
    }

    void Digestion()
    {
        StomachCapacity -= DigestAmount;
         
        if(StomachCapacity <= StomachHungryRange)
        {
            isHungry = true;
        }
    }

    private void OnTriggerEnter(Collider o)
    {
        if(o.transform == MainTarget)
        {
            if (isHeadingToFood)
            {
                Interaction();
            }
        }
    }
}
