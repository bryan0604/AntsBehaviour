﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public float FeedIntervalTime;
    public float FeedAmount;
    public float BirthIntervals;
    public float BirthChannelingTime;

    private float _BirthChannel;
    private float _FeedInterval;
    //private float _IdleTime;

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
    public bool isGivingBirth;

    public Transform SelectionCircle;
    public Transform InteractionPoint;
    public Transform LayEggPoint;
    public Transform MainTarget;
    public Transform Eggs;

    public NavMeshAgent QueenWanda;

    private void Start()
    {
        QueenWanda.speed = MovementSpeed;
        //_IdleTime = IdleTime;
        _FeedInterval = FeedIntervalTime;
        _BirthChannel = BirthChannelingTime;

        Invoke("Wander", 1f);

        InvokeRepeating("Digestion", DigestionTime, DigestionTime);

        InvokeRepeating("Birth", BirthIntervals, BirthIntervals);
    }

    void Birth()
    {
        if(isHungry)
        {
            BirthCondition -= BirthIncrement;
        }
        else
        {
            BirthCondition += BirthIncrement;
        }

        if (BirthCondition >= 100)
        {
            GiveBirth();
        }
    }

    void GiveBirth()
    {
        if (isGivingBirth) return;

        Debug.Log(transform.name + " Processing Giving Birth");

        isGivingBirth = true;

        if (isFeeding)
        {
            CancelFeedInfo();
        }

        CancelInvoke("Birth");
        CancelInvoke("Digestion");
    }

    private void FixedUpdate()
    { 
        if(QueenWanda.hasPath)
        {
            if (QueenWanda.remainingDistance < StopDistance)
            {
                QueenWanda.ResetPath();

                if (isWandering && !isGivingBirth)
                {
                    StartCoroutine(Idle());
                }
            }
        }

        if(isFeeding)
        {
            _FeedInterval -= Time.deltaTime;

            if(_FeedInterval <= 0)
            {
                StomachCapacity += FeedAmount;

                _FeedInterval = FeedIntervalTime;

                Debug.Log(transform.name + " stomach = " + StomachCapacity);

                if(StomachCapacity >= 100)
                {
                    CancelFeedInfo();

                    Debug.Log(transform.name + " is full " + StomachCapacity);

                    InvokeRepeating("Digestion", DigestionTime, DigestionTime);

                    StartCoroutine(Idle());
                }
            }
        }
        
        if(isGivingBirth)
        {
            _BirthChannel -= Time.deltaTime;

            if(_BirthChannel <= 0)
            {
                Debug.Log(transform.name + " Birth!");

                Instantiate(Eggs, LayEggPoint.position, Quaternion.identity);

                _BirthChannel = BirthChannelingTime;

                BirthCondition = 0;

                isGivingBirth = false;

                InvokeRepeating("Digestion", DigestionTime, DigestionTime);

                InvokeRepeating("Birth", BirthIntervals, BirthIntervals);

                StartCoroutine(Idle());

                return;
            }
        }
    }

    void CancelFeedInfo()
    {
        isFeeding = false;

        isHungry = false;

        isInteracting = false;

        isHeadingToFood = false;
    }

    void Interaction()
    {
        Debug.Log(transform.name + " checking Interaction");

        Movement(false);

        if(!isInteracting)
        {
            isInteracting = true;

            if(isHungry)
            {
                Feeding();
            }
        }
        else
        {
            return;
        }
    }

    void Feeding()
    {
        isFeeding = true;

        CancelInvoke("Digestion");
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
        if (isGivingBirth) return;

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

        float X, Z, RandomX, RandomZ;

        X = Nest.singleton.NestSize / 2;
        Z = Nest.singleton.NestSize / 2;

        RandomX = Random.Range(-X, X);
        RandomZ = Random.Range(-Z, Z);

        Vector3 TargetPoint = new Vector3(RandomX, 0.1f, RandomZ);

        TargetPoint = Nest.singleton.transform.TransformPoint(TargetPoint / 2f);

        Collider[] WullyWollys = Physics.OverlapSphere(TargetPoint , transform.localScale.z);

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

            GameObject _SelectionCircle = Instantiate(SelectionCircle.gameObject, TargetPoint, Quaternion.identity);

            _SelectionCircle.transform.localScale = scales;

            _SelectionCircle.GetComponent<SelectionCircle>().ImBelongTo = transform;

            QueenWanda.SetDestination(TargetPoint);

            isWandering = true;
        }
        else
        {
            Debug.DrawLine(transform.position, TargetPoint, Color.red, 1f);

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
