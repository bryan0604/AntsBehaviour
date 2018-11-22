using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Work
// 1. Continue Behaviour after misplacement of Food outside the nest. (line.458)
// 4. Growth
// 5. Solve Wander sizing issue
#endregion
#region Errors
// 1. Ant being spawned, when moving object it'll triggered Initiate Help function
// 2. Array error after placing object and ishungry 
#endregion
    
public class AntsIntermediate : MonoBehaviour
{
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

    #region Debugger
    void DebugCheck()
    {
        
        DebugBool.Add(isIdling);
        DebugBool.Add(isWandering);
        DebugBool.Add(isAbleToDoNextTask);
        DebugBool.Add(isHungry);
        DebugBool.Add(isGoingToCarryFood);
        DebugBool.Add(isGoingToEatFood);
        DebugBool.Add(isGoingToHelpCarryAnObject);
        DebugBool.Add(isFeeding);
        DebugBool.Add(isCarryingAnObject);
        DebugBool.Add(isHelpingCarryAnObject);
        DebugBool.Add(OffMainTargetManager);

        Invoke("DebugCheck2", 20f);
    }

    void DebugCheck2()
    {
        DebugBool2.Add(isIdling);
        DebugBool2.Add(isWandering);
        DebugBool2.Add(isAbleToDoNextTask);
        DebugBool2.Add(isHungry);
        DebugBool2.Add(isGoingToCarryFood);
        DebugBool2.Add(isGoingToEatFood);
        DebugBool2.Add(isGoingToHelpCarryAnObject);
        DebugBool2.Add(isFeeding);
        DebugBool2.Add(isCarryingAnObject);
        DebugBool2.Add(isHelpingCarryAnObject);
        DebugBool2.Add(OffMainTargetManager);

        bool ChangesBeenMade = false;

        foreach (var h in DebugBool)
        {
            foreach (var i in DebugBool2)
            {
                if(h!=i)
                {
                    ChangesBeenMade = true;
                }
                else 
                {

                }
            }
        }
        
        if(ChangesBeenMade)
        {
            Debug.LogWarning(transform.name + " - Positive");
        }
        else
        {
            Debug.LogWarning(transform.name + " - Negative");
        }

        Invoke("DebugCheck", 5f);
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

                    Wander();
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

                        Wander();
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

        Wander();
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

        isCarryingAnObject = false;

        Food f = MainTarget.GetComponent<Food>();

        f.OnBeingPlacedToTheGround();

        f.FoodCheck();

        OffMainTargetManager = true;

        Agent260.avoidancePriority = 50;

        Debug.Log(transform.name + " placing Item");

        if(isHungry)
        {
            if(f.IsInNest)
            {
                Debug.Log(transform.name + " food placed IsInnest = " +f.IsInNest);

                SearchForFood(false, true);
            }
            else
            {
                Debug.Log(transform.name + " food placed but IsInnest = " + f.IsInNest);

                // Might want to continue if Food is being placed out of nest
            }
        }
        else
        {
            MainTarget = null;

            Invoke("Wander", 2f);
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

        TargetPoint = Nest.singleton.transform.TransformPoint(TargetPoint );

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

            Invoke("TravelHome", 0.1f);
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

                SearchForFood(true,false);
            }
            else
            {
                Debug.Log(transform.name + " not hungry therefore WANDER");

                Wander();

                isAbleToDoNextTask = false;
            }
        }
    }
    #endregion

    #region Search Food to Eat
    void SearchForFood(bool isRandomJob, bool EatFood)
    {
        Debug.Log(transform.name + " searching for food");

        #region Version 1
        //Collider[] AllFoods = Physics.OverlapSphere(transform.position, DetectRange);

        //List<Food> Foods = new List<Food>();

        //List<Food> FoodsAreInNest = new List<Food>();

        //foreach (var food in AllFoods)
        //{
        //    if (food.GetComponent<Food>())
        //    {
        //        Food f = food.GetComponent<Food>();

        //        if (f.isDepeleted || f.FoodCapacity <= 0)
        //        {

        //        }
        //        else if(f.IsInNest) // if any food found in nest just go eat
        //        {
        //            FoodsAreInNest.Add(f);

        //            //MoveToDestination(f.transform);

        //            //return;
        //        }
        //        else if(f.FoodCapacity > 0)
        //        {
        //            Foods.Add(f);
        //        }
        //    }
        //}

        //if(FoodsAreInNest.Count > 0)
        //{
        //    MoveToDestination(FoodsAreInNest[Random.Range(0, FoodsAreInNest.Count)].transform);

        //    return;
        //}
        //else
        //{
        //    foreach (var item in Foods)
        //    {
        //        if (item.isDepeleted || item.FoodCapacity <= 0 || item == null)
        //        {
        //            Foods.Remove(item);
        //        }
        //    }

        //    if (Foods.Count > 0)
        //    {
        //        MoveToDestination(Foods[Random.Range(0, Foods.Count)].transform);
        //    }
        //    else
        //    {
        //        Debug.Log(transform.name + " no food found!");

        //        Wander();
        //    }
        //}
        #endregion

        #region Version 2
        Collider[] AllObjectsInZone = Physics.OverlapSphere(transform.position, DetectRange);
        List<Food> DetectedFoods = new List<Food>();

        Food food;

        foreach (var i in AllObjectsInZone)
        {
            if(i.gameObject.tag == "Food")
            {
                Food foodo = i.GetComponent<Food>();

                if(foodo.FoodCapacity > 0 && !foodo.isDepeleted)
                {
                    DetectedFoods.Add(i.GetComponent<Food>());
                }
            }
        }

        if (DetectedFoods.Count > 0)
        {
            int Number = (Random.Range(0, 2));

            List<Food> FoodsInNest = new List<Food>();

            MovementSpeedManagement(true, 0);

            if (EatFood)
            {
                foreach (var foodie in DetectedFoods)
                {
                    if(foodie.IsInNest)
                    {
                        FoodsInNest.Add(foodie);
                    }
                }

                if (FoodsInNest.Count>0)
                {
                    food = FoodsInNest[Random.Range(0, FoodsInNest.Count)];
                }
                else
                {
                    Debug.LogWarning("A - Food not found");

                    return;
                    //food = FoodsInNest[Random.Range(0, FoodsInNest.Count)];
                }
            }
            else
            {
                food = DetectedFoods[Random.Range(0, DetectedFoods.Count)];
            }

            if (isRandomJob)
            {
                if (Number == 0) // carry
                {
                    if (food.IsInNest)
                    {
                        Debug.LogWarning(transform + " Revoking (In Nest)" + food.transform.name);

                        SearchForFood(true, false);

                    }
                    else if (!food.IsInNest) // Carry (New Carry with Lifting Mechanism?)
                    {
                        Debug.Log(transform + " moving to (Out Nest)" + food.transform.name);

                        food.isBeingPlaced = false;

                        isGoingToCarryFood = true;

                        OffMainTargetManager = false;

                        MainTarget = food.transform;

                        Agent260.SetDestination(food.transform.position);
                    }
                }
                else // go Eat
                {
                    if (food.IsInNest)
                    {
                        Debug.Log(transform + " moving to (In Nest)" + food.transform.name);

                        isGoingToEatFood = true;

                        MainTarget = food.transform;

                        Agent260.SetDestination(food.transform.position);
                    }
                    else if (!food.IsInNest) // Carry (New Carry with Lifting Mechanism?)
                    {
                        Debug.LogWarning(transform + " Revoking (Out Nest)" + food.transform.name);

                        SearchForFood(true, false);
                    }
                }
            }
            else
            {
                if (food.IsInNest)
                {
                    Debug.Log(transform + " Commencing Feeding Mechanism " + food.transform.name);

                    //isGoingToEatFood = true;

                    //MainTarget = food.transform;

                    //Agent260.SetDestination(food.transform.position);
                }
                //else if (!food.IsInNest) // Carry (New Carry with Lifting Mechanism?)
                //{
                //    Debug.Log(transform + " moving to (Out Nest)" + food.transform.name);

                //    food.isBeingPlaced = false;

                //    isGoingToCarryFood = true;

                //    OffMainTargetManager = false;

                //    MainTarget = food.transform;

                //    Agent260.SetDestination(food.transform.position);
                //}
            }
        }
        else
        {
            Debug.Log(transform.name + " no food found!");

            Wander();
        }
        #endregion
    }
    #endregion

    #region Move to Destination
    void MoveToDestination(Food food)
    {
        MovementSpeedManagement(true, 0);

        #region Version 1
        //if (TargetTransform.GetComponent<Food>())
        //{
        //    Food food = TargetTransform.GetComponent<Food>();

        //    if (food.IsInNest)
        //    {
        //        Debug.Log(transform + " moving to (In Nest)" + food.transform.name);

        //        isGoingToEatFood = true;

        //        MainTarget = food.transform;

        //        Agent260.SetDestination(food.transform.position);
        //    }
        //    else if(!food.IsInNest) // Carry (New Carry with Lifting Mechanism?)
        //    {
        //        Debug.Log(transform + " moving to (Out Nest)" + food.transform.name);

        //        food.isBeingPlaced = false;

        //        isGoingToCarryFood = true;

        //        OffMainTargetManager = false;

        //        // x 10 to move carryPoint Y position
        //        //Debug.Log(food.GetComponent<Renderer>().bounds.size + " " + food.GetComponent<Renderer>().bounds.extents);

        //        MainTarget = food.transform;

        //        Agent260.SetDestination(food.transform.position);
        //    }
        //    else if(food.IsBeingCarried)
        //    {
        //        Debug.Log(transform + " moving to help carry " + food.transform.name);
        //    }
        //}
        #endregion

        #region Version 2

       
        #endregion

    }
    #endregion

    #region Interaction
    void Interaction()
    {
        Collider[] Col = Physics.OverlapSphere(InteractionPoint.position,0.1f);

        foreach (var item in Col)
        {
            Debug.Log(item.name);
        }

        Debug.Log(transform.name + " Check interaction ");

        if(isGoingToEatFood)
        {
            isGoingToEatFood = false;
        }

        if (isGoingToCarryFood)
        {
            isGoingToCarryFood = false;
        }
    }
    #endregion

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
        //if(NestManager.InsufficientFood)
        //{

        //    Debug.Log(transform.name + " nest has insufficient food");

        //    isAbleToDoNextTask = true;

        //    return;
        //}
        //else
        //{
        //    Debug.Log(transform.name + " nest has sufficient food");
            
        //}

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

            Invoke("Wander", 0.1f);
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
