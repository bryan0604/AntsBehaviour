﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Work
// 1. Continue Behaviour after misplacement of Food outside the nest. (line.458) (DONE)
// 4. Growth
// 5. Solve Wander sizing issue (DONE)
#endregion
#region Errors
// 1. Ant being spawned, when moving object it'll triggered Initiate Help function (SOLVED)
// 2. Array error after placing object and ishungry (SOLVED)
#endregion

public class AntsIntermediate : MonoBehaviour
{
    public List<Food> DetectedFoods = new List<Food>();

    public List<Food> FoodsInNest = new List<Food>();

    public float RandomTimeRange;

    #region Variables
    public float StomachCapacity=100f;
    public float StomachRequirement=95f;
    public float StomachCrement = 2f;
    public float StomachDigestTime = 2f;
    public float StopDistance = 0.5f;
    public float MovementSpeed = 2f;
    public float WanderRange;
    public float DetectRange=6f;
    public float Time_DoingNothing=5f;
    public float Time_Feeding = 2f;
    //private float _StopDistance;
    private float _DoingNothing;
    private float _Feeding;
    public NavMeshAgent Agent260;
    public Nest NestManager;

    public bool isIdling;
    public bool isWandering;
    public bool isAbleToDoNextTask;
    public bool isHungry;
    public bool isGoingToCarryFood;
    public bool isGoingToEatFood;
    public bool isGoingToHelpCarryAnObject;
    public bool isFeeding;
    public bool isCarryingAnObject;
    public bool isHelpingCarryAnObject;
    public bool OffMainTargetManager;

    public List<bool> DebugBool = new List<bool>();
    public List<bool> DebugBool2 = new List<bool>();
    

    public bool TestMode;

    public Transform InteractionPoint;
    public Transform CarryPoint;
    public Transform MainTarget;
    public Transform Selectioncircle;
    #endregion

    #region Start
    void Start ()
    {
        if (TestMode) return;

        _DoingNothing = Time_DoingNothing;

        _Feeding = Time_Feeding;

        //_StopDistance = StopDistance;

        Agent260.speed = MovementSpeed;

        if(NestManager!=null)
        {

        }
        else
        {
            NestManager = Nest.singleton;
        }

        NestManager.AssignAntInfos(this);

        Invoke("Wander", 0.2f);

        InvokeRepeating("Digestion", StomachDigestTime, StomachDigestTime);

        //DebugCheck();
	}
    #endregion

    void MovementSpeedManagement(bool Activate, int Divide)
    {
        Debug.Log(transform.name + " speed activate = " + Activate);
        if(Activate)
        {
            if(Divide>0)
            {
                Agent260.speed = MovementSpeed/Divide;
            }
            else
            {
                Agent260.speed = MovementSpeed;
            }

        }
        else
        {
            Agent260.speed = 0;
            Agent260.velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            #region  Cursor Movement
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //RaycastHit hit;

            //if (Physics.Raycast(ray, out hit))
            //{
            //    Agent260.SetDestination(hit.point);
            //}
            //Agent260.SetDestination(Pos);
            #endregion

            #region Selection Circle
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 _scales = new Vector3(transform.localScale.x*2, 0.01f, transform.localScale.z*2);

                GameObject _SelectionCircle = Instantiate(Selectioncircle.gameObject, hit.point, Quaternion.identity);

                _SelectionCircle.transform.localScale = _scales;

                _SelectionCircle.GetComponent<SelectionCircle>().ImBelongTo = transform;

            }
            #endregion

        }

        if (Agent260.hasPath)
        {
            if (Agent260.remainingDistance < StopDistance)
            {
                if (isWandering)
                {
                    //Debug.Log("B");
                    Idle();
                }

                if (isCarryingAnObject)
                {
                    PlaceItem();
                }
            }
            else
            {

            }
        }
        else
        {
            //Debu/*g*/.Log("/*Stucked*/");
        }

        // this make wander and idle connections
        if (!isAbleToDoNextTask) // Do 1
        {
            if (isIdling) // Do Multi
            {
                _DoingNothing -= Time.deltaTime;

                if (_DoingNothing <= 0)
                {
                    isIdling = false;

                    TaskManagement();
                }
            }
        }

        // this loop eating
        FeedingManagement();

        MainTargetManager();
 
    }

    #region Feeding
    void FeedingManagement()
    {
        if (isFeeding)
        {
            _Feeding -= Time.deltaTime;

            RotateToCorrectAngle();

            if (_Feeding <= 0)
            {
                //Food enter mouth
                bool FoodisAtMouth = false;

                Collider[] colly = Physics.OverlapSphere(InteractionPoint.position, 0.1f);

                foreach (var item in colly)
                {
                    if (item.gameObject.tag == "Food" && MainTarget == item.transform)
                    {
                        if(MainTarget.GetComponent<Food>())
                        {
                            Food f = MainTarget.GetComponent<Food>();

                            if(f.isDepeleted)
                            {
                                FoodisAtMouth = false;

                                MainTarget = null;

                                isFeeding = false;

                                //Destroy(f);

                                Debug.Log(transform.name + " food depeleted!");
                            }
                            else
                            {
                                FoodisAtMouth = true;

                                MainTarget.GetComponent<Food>().ConsumingFood(1);
                            }
                        }
                    }
                }

                if (FoodisAtMouth == false)
                {
                    isFeeding = false;

                    float t = Random.Range(0, RandomTimeRange); // 

                    Invoke("Wander",t);
                }
                else
                {
                    _Feeding = Time_Feeding;

                    StomachCapacity += StomachCrement;

                    isAbleToDoNextTask = false;

                    Debug.Log(transform.name + " stomach = " + StomachCapacity + " " + FoodisAtMouth);

                    if (StomachCapacity >= 100)
                    {
                        isFeeding = false;

                        isHungry = false;

                        MainTarget = null;

                        Debug.Log(transform.name + " is full so not Eating and continue Wander");

                        InvokeRepeating("Digestion", StomachDigestTime, StomachDigestTime);

                        float t = Random.Range(0, RandomTimeRange); // 

                        Invoke("Wander", t);
                    }
                }
            }
        }
    }
    #endregion

    void MainTargetManager()
    {
        if (OffMainTargetManager == true) return;

        if (MainTarget != null)
        {
            if (MainTarget.GetComponent<Food>())
            {
                Food food = MainTarget.GetComponent<Food>();

                if (food.IsBeingCarried) // once food is being carried, check
                {
                    if (food.MainCarryAnt != transform)//if its not you carry , then u assist
                    {
                        if (Vector3.Distance(transform.position, food.transform.position) < StopDistance + 0.3f)
                        {
                            //Debug.Log(transform.name + " NEAR");
                            RotateToCorrectAngle();

                            Agent260.speed = 0;
                        }
                        else
                        {
                            //Debug.Log(transform.name + " FAR");

                            Agent260.speed = MovementSpeed;
                        }

                        Agent260.SetDestination(food.transform.position); // follow the food "Note" may put a range
                    }
                    else
                    {
                        if (food.isSetOnCarriedPosition) // if you are the one carrying,
                        {
                            if (!isCarryingAnObject) // travel home once isCarryAnObj is true
                            {
                                isCarryingAnObject = true;

                                TravelHome();
                            }
                        }
                    }
                }
                else // if food is not being carried and is being placed
                {
                    if (food.isBeingPlaced)
                    {
                        AssistReset();
                    }
                }
            }
        }
    }

    #region Reset Assist Ant Stats
    void AssistReset()
    {
        Debug.LogWarning(transform.name + " resume normal AI behaviours ");

        OffMainTargetManager = true;

        isHelpingCarryAnObject = false;

        isGoingToCarryFood = false;

        MainTarget = null;

        InteractionPoint.GetComponent<SphereCollider>().enabled = true;

        float t = Random.Range(0, RandomTimeRange); // 

        Invoke("Wander", t);
    }
    #endregion

    #region Rotation - Transform Corrections
    void RotateToCorrectAngle()
    {
        if (MainTarget == null)
            return;
        Vector3 dir = MainTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.smoothDeltaTime * 5f).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0);
    }
    #endregion

    #region Place Item In Nest
    void PlaceItem()
    {
        MovementSpeedManagement(false,0);

        Food f;

        isCarryingAnObject = false;

        if(MainTarget.GetComponent<Food>()!=null)
        {
            f = MainTarget.GetComponent<Food>();

            f.OnBeingPlacedToTheGround();

            f.FoodCheck();

            OffMainTargetManager = true;

            Agent260.avoidancePriority = 50;

            Debug.Log(transform.name + " placing Item");

            if (isHungry)
            {
                if (f.IsInNest)
                {
                    Debug.Log(transform.name + " food placed IsInnest = " + f.IsInNest);

                    isFeeding = true;
                }
                else
                {
                    Debug.Log(transform.name + " food placed but IsInnest = " + f.IsInNest);

                    // Might want to continue if Food is being placed out of nest
                    float t = Random.Range(0, RandomTimeRange); // 

                    Invoke("Wander", t);
                }
            }
            else
            {
                MainTarget = null;

                float t = Random.Range(0, RandomTimeRange); // 

                Invoke("Wander", t);
            }
        }
    }
    #endregion

    #region Travel Home
    void TravelHome()
    {
        Debug.Log(transform.name + " travel home");

        float X = Nest.singleton.NestSize/2;
        
        float Z = Nest.singleton.NestSize/2;

        float RandomX = Random.Range(-X, X);
        
        float RandomZ = Random.Range(-Z, Z);

        Vector3 TargetPoint = new Vector3(RandomX, 0.1f, RandomZ);

        TargetPoint = Nest.singleton.transform.TransformPoint(TargetPoint/2f );

        Collider[] AllContactedObjects = Physics.OverlapSphere(TargetPoint, 0.25F);

        bool _FoundNest = false;

        foreach (var i in AllContactedObjects)
        {
            if(i.gameObject.tag == "Nest")
            {
                _FoundNest = true;
            }
        }

        if(_FoundNest)
        {
            if (isCarryingAnObject)
            {
                MovementSpeedManagement(true, 5);
            }
            else
            {
                MovementSpeedManagement(true, 0);
            }

            Debug.Log(transform.name + " nest located!");

            Debug.DrawLine(transform.position, TargetPoint, Color.cyan, 5f);

            Agent260.avoidancePriority = 49;

            Agent260.SetDestination(TargetPoint);
        }
        else
        {
            Debug.LogWarning(transform.name + " failed to locating Nest");

            Debug.DrawLine(transform.position, TargetPoint, Color.red, 5f);

            float t = Random.Range(0, RandomTimeRange); // 

            Invoke("TravelHome", t);
        } 
    }

    #endregion

    #region Task Management
    void TaskManagement()
    {
        if(!isAbleToDoNextTask)
        {
            isAbleToDoNextTask = true;
        }

        Debug.Log(transform.name + " queuing next task");

        if(isAbleToDoNextTask)
        {
            if(isHungry)
            {
                Debug.Log(transform.name + " is hungry");

                float t = Random.Range(0, RandomTimeRange); // 

                Invoke("SearchForFood", t);
            }
            else
            {
                Debug.Log(transform.name + " not hungry therefore WANDER");

                float t = Random.Range(0, RandomTimeRange); // 

                Invoke("Wander",t);

                isAbleToDoNextTask = false;
            }
        }
    }
    #endregion

    #region Search Food to Eat
    void SearchForFood()
    {
        Debug.Log(transform.name + " searching for food");

        #region Version 2
        Collider[] AllObjectsInZone = Physics.OverlapSphere(transform.position, DetectRange);

        DetectedFoods = new List<Food>();

        FoodsInNest = new List<Food>();

        NestManager.OnNestCheckSupply();

        foreach (var i in AllObjectsInZone)
        {
            if(i.gameObject.tag == "Food")
            {
                Food food = i.GetComponent<Food>();

                if(food.IsInNest)
                {

                }
                else
                {
                    if (food.FoodCapacity > 0 && !food.isDepeleted)
                    {
                        DetectedFoods.Add(food);
                    }
                }
            }
        }

        if(NestManager.FoodAreInNest.Count > 0) // add there might be a slim chance it will go carry(top up) supplies
        {
            int chance = Random.Range(0, 3);

            if (chance == 1) 
            {
                OnFoodsInNest();
            }
            else
            {
                //Debug.Log(transform.name + "Food found in nest but going to top up.");

                OnGoingDetectedFoods(DetectedFoods);
            }
        }
        else
        {
            Debug.Log("No food in nest");

            OnGoingDetectedFoods(DetectedFoods);
        }
        #endregion
    }
    #endregion

    void OnGoingDetectedFoods(List<Food> DetectedFoods)
    {
        Food ChosenFood;

        if (DetectedFoods.Count > 0)
        {
            MovementSpeedManagement(true, 0);

            ChosenFood = DetectedFoods[Random.Range(0, DetectedFoods.Count)];

            ChosenFood.isBeingPlaced = false;

            isGoingToCarryFood = true;

            OffMainTargetManager = false;

            MainTarget = ChosenFood.transform;

            Agent260.SetDestination(ChosenFood.transform.position);

            Debug.Log(transform + " moving to (Out Nest)" + ChosenFood.transform.name);
        }
        else
        {
            Debug.LogWarning(transform.name + " hasn't found any food - relocating food");

            OnFoodsInNest();

        }
    }

    void OnFoodsInNest()
    {
        Food ChosenFood;

        MovementSpeedManagement(true, 0);

        if(NestManager.FoodAreInNest.Count > 0)
        {
            ChosenFood = NestManager.FoodAreInNest[Random.Range(0, NestManager.FoodAreInNest.Count)]; // call nest to check again b4 this

            Debug.Log(transform + " moving to (In Nest) " + ChosenFood.transform.name);

            isGoingToEatFood = true;

            MainTarget = ChosenFood.transform;

            Agent260.SetDestination(ChosenFood.transform.position);
        }
        else
        {
            Debug.LogWarning("No Food left.");

            float t = Random.Range(0, RandomTimeRange); // 

            Invoke("Wander",t);
        }
    }

    #region Idle 
    void Idle()
    {
        if (isAbleToDoNextTask) return;

        if(!isIdling) // Do 1
        {
            Debug.Log(transform.name + " is idling");

            isWandering = false;

            isIdling = true;

            _DoingNothing = Time_DoingNothing;

            MovementSpeedManagement(false, 0);

            ConditionCheck();
        }
    }
    #endregion

    #region Conditions
    void ConditionCheck()
    {
        Debug.Log(transform.name + " checking Condition");

        if(StomachCapacity < StomachRequirement)
        {
            isHungry = true;
        }
        else
        {
            
        }
    }
    #endregion

    #region Wander System
    void Wander() // Do 1 && Note may cast a small area to check if point is clear to move to
    {
        Agent260.ResetPath();

        float AreaX, AreaZ;

        AreaX = Random.Range(-(WanderRange), (WanderRange));

        AreaZ = Random.Range(-(WanderRange), (WanderRange));

        Vector3 V = new Vector3(transform.position.x + AreaX, 0.1f, transform.position.z + AreaZ);

        bool AbleToMove = true;
        Collider[] ListObjects = Physics.OverlapSphere(V, transform.localScale.z);

        foreach (var i in ListObjects)
        {
            if(i.gameObject.tag == "Food") // may add more in the future
            {
                AbleToMove = false;
            }
            else if (i.gameObject.tag == "Ant")
            {
                AbleToMove = false;
            }
        }

        if(AbleToMove)
        {
            Debug.Log(transform.name + " is Wandering");

            Agent260.SetDestination(V);

            isWandering = true;

            isAbleToDoNextTask = false;

            MovementSpeedManagement(true, 0);

            //Debug.DrawLine(transform.position, V, Color.cyan, 5f);

            Vector3 _scales = new Vector3(transform.localScale.x * 2, 0.01f, transform.localScale.z * 2);

            GameObject _SelectionCircle = Instantiate(Selectioncircle.gameObject, V, Quaternion.identity);

            _SelectionCircle.transform.localScale = _scales;

            _SelectionCircle.GetComponent<SelectionCircle>().ImBelongTo = transform;
        }
        else
        {
            Debug.Log(transform.name + " is not able to Wandering");
            Debug.DrawLine(transform.position, V, Color.red, 5f);

            Invoke("Wander", 1f);
        }

    }
    #endregion

    #region Digestion
    void Digestion()
    {
        StomachCapacity -= StomachCrement;
    }
    #endregion

    #region Collision
    private void OnTriggerEnter(Collider other)
    {
        if (MainTarget == null) return; // must have a Target then check Interacting;

        if(MainTarget == other.transform)
        {
            Agent260.speed = 0f;
            Agent260.velocity = Vector3.zero;
            //Debug.Log(transform.name + " Check interaction ");
            if (MainTarget.GetComponent<Food>())
            {
                Food food = MainTarget.GetComponent<Food>();

                if(food.IsBeingCarried)
                {
                    if (isHelpingCarryAnObject) return;

                    Debug.Log(transform.name + " Initiate helping to carry  ");

                    InteractionPoint.GetComponent<SphereCollider>().enabled = false;

                    MovementSpeedManagement(true, 0);

                    //StopDistance = 0.7f;

                    isHelpingCarryAnObject = true;
                }
                else
                {
                    if (isGoingToEatFood)
                    {
                        isGoingToEatFood = false;

                        ConsumeFood();
                    }

                    if (isGoingToCarryFood)
                    {
                        isGoingToCarryFood = false;

                        MovingObjectMechanism();
                    }
                }
            }
        }
    }
    #endregion

    #region Carry Mechanic
    void MovingObjectMechanism()
    {
        Food Food = MainTarget.GetComponent<Food>();

        Food.MainCarryAnt = transform;

        Food.MoveTowards(CarryPoint);

        Debug.Log(transform.name + " Moving object");
    }
    #endregion

    #region Eat Food
    void ConsumeFood()
    {
        Debug.Log(transform.name + " is Eating");

        CancelInvoke("Digestion");

        isFeeding = true;
    }
    #endregion
}
