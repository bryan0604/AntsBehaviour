using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AntsIntermediate : MonoBehaviour
{
    public List<Food> DetectedFoods = new List<Food>();
    public List<Food> FoodsInNest = new List<Food>();
    public List<Vector3> StorageLocations = new List<Vector3>();
    public float CarryRange;
    public float RandomTimeRange;
    public float FoodDetectDistance = 1f;
    public int PatrolsAmount;
    private int _PatrolsAmount;
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
    public bool isPatrolling;
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
    public bool isSendingDistressSignal;
    public bool TestMode;
    public bool AntIsStucked;
    private bool _FoodRangeManagement;
    public Transform DistressSignalObject;
    public Transform InteractionPoint;
    public Transform CarryPoint;
    public Transform MainTarget;
    public Transform Selectioncircle;
    public SphereCollider _InteractCollider;
    #endregion

    #region Start
    void Start ()
    {
        if (TestMode) return;

        _DoingNothing = Time_DoingNothing;

        _Feeding = Time_Feeding;

        _PatrolsAmount = PatrolsAmount;
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

        InvokeRepeating("DebugManagement", 5f, 5f);
	}
    #endregion

    void DebugManagement()
    {
        if(AntIsStucked)
        {
            return;
        }

        StorageLocations.Add(transform.position);
        Debug.Log(StorageLocations.Count);

        if(StorageLocations.Count > 2)
        {
            StorageLocations.RemoveAt(0);
        }

        if(StorageLocations.Count == 2)
        {
            if(StorageLocations[0].magnitude == StorageLocations[1].magnitude)
            {
                Debug.Log(transform.name + " doesn't move" + StorageLocations[0].magnitude + " " + StorageLocations[1].magnitude);

                StorageLocations.Clear();

                AntIsStucked = true;
            }

        }
    }

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
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //RaycastHit hit;

            //if (Physics.Raycast(ray, out hit))
            //{
            //    Vector3 _scales = new Vector3(transform.localScale.x*2, 0.01f, transform.localScale.z*2);

            //    GameObject _SelectionCircle = Instantiate(Selectioncircle.gameObject, hit.point, Quaternion.identity);

            //    _SelectionCircle.transform.localScale = _scales;

            //    _SelectionCircle.GetComponent<SelectionCircle>().ImBelongTo = transform;

            //}
            #endregion
        }

        if(isGoingToEatFood)
        {
            if(Agent260.remainingDistance < 0.5f)
            {
                RotateToCorrectAngle();
            }
        }

        if (Agent260.hasPath)
        {
            if (Agent260.remainingDistance < StopDistance)
            {
                if (isWandering || isPatrolling)
                {
                    Idle();
                }

                if (isCarryingAnObject)
                {
                    PlaceItem();
                }
            }
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

        //RotateToCorrectAngle();

        // this loop eating
        FeedingManagement();

        MainTargetManager();
 
    }

    //void FoodRangeManagement()
    //{
    //    bool _gotCha = false;

    //    if (_MainFood != null)
    //    {
    //        //RotateToCorrectAngle();
    //        RaycastHit[] hit;
    //        hit = Physics.RaycastAll(transform.position, transform.forward, 100f);

    //        for (int i = 0; i < hit.Length; i++)
    //        {
    //            RaycastHit h = hit[i];
    //            if(h.transform.GetComponent<Food>())
    //            {
    //                Food f = h.transform.GetComponent<Food>();

    //                if(f==_MainFood)
    //                {
    //                    Debug.Log(transform.name + " is facing " + f.transform.name);

    //                    _gotCha = true;
    //                }
    //            }
    //        }

    //        if(_gotCha)
    //        {
    //            //MovementSpeedManagement(false, 0);

    //            isGoingToEatFood = false;

    //            ConsumeFood();
    //            //isFeeding = true;

    //            //CancelInvoke("Digestion");
    //        }
    //    }
    //}

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

                Collider[] colly = Physics.OverlapSphere(InteractionPoint.position, 2f);

                foreach (var item in colly)
                {
                    if (item.GetComponent<Food>())
                    {
                        Food _MainFood = item.GetComponent<Food>();

                        if (_MainFood.transform == MainTarget.transform)
                        {
                            if (_MainFood.GetComponent<Food>())
                            {
                                if (_MainFood.isDepeleted)
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
                }

                if (FoodisAtMouth == false)
                {
                    isFeeding = false;

                    float t = Random.Range(0, RandomTimeRange); // 

                    Invoke("Wander",t);
                }
                else
                {
                    //MovementSpeedManagement(false, 0);

                    _Feeding = Time_Feeding;

                    StomachCapacity += StomachCrement;

                    isAbleToDoNextTask = false;

                    Debug.Log(transform.name + " stomach = " + StomachCapacity + " " + FoodisAtMouth);

                    if (StomachCapacity >= 100)
                    {
                        InteractionPoint.GetComponent<SphereCollider>().enabled = true;

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
                Food _MainFood = MainTarget.GetComponent<Food>();

                if (_MainFood.IsBeingCarried) // once food is being carried, check
                {
                    if (_MainFood.MainCarryAnt != transform)//if its not you carry , then u assist
                    {
                        if (Vector3.Distance(transform.position, _MainFood.transform.position) < StopDistance + 0.3f)
                        {
                            //Debug.Log(transform.name + " NEAR");
                            //RotateToCorrectAngle();

                            //Agent260.speed = 0;
                        }
                        else
                        {
                            //Debug.Log(transform.name + " FAR");

                            Agent260.speed = MovementSpeed;
                        }

                        Agent260.SetDestination(_MainFood.transform.position); // follow the food "Note" may put a range
                    }
                    else
                    {
                        if (_MainFood.isSetOnCarriedPosition) // if you are the one carrying,
                        {
                            if (!isCarryingAnObject) // travel home once isCarryAnObj is true
                            {
                                isCarryingAnObject = true;

                                TravelHome();
                            }
                        }
                    }
                }
                else if(_MainFood.IsBeingCarried == false)// if food is not being carried and is being placed
                {
                    if (_MainFood.isBeingPlaced)
                    {
                        AssistReset();
                    }
                }
                else
                {
                   
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

        float t = Random.Range(0, RandomTimeRange); // 

        Invoke("Wander", t);
    }
    #endregion

    #region Rotation - Transform Corrections
    void RotateToCorrectAngle()
    {
        if (MainTarget == null) return;

        Vector3 dir = MainTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.smoothDeltaTime * 5f).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0);
    }
    #endregion

    #region Place Item In Nest
    void PlaceItem()
    {
        Food f;

        isCarryingAnObject = false;

        if (MainTarget.GetComponent<Food>()!=null)
        {
            f = MainTarget.GetComponent<Food>();

            f.OnBeingPlacedToTheGround();

            f.FoodCheck();

            OffMainTargetManager = true;

            isGoingToCarryFood = false;

            Agent260.avoidancePriority = 50;

            Debug.Log(transform.name + " placing Item");

            if (isHungry)
            {
                if (f.IsInNest)
                {
                    Debug.Log(transform.name + " food placed IsInnest = " + f.IsInNest);

                    MovementSpeedManagement(false, 0);

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

        Collider[] AllContactedObjects = Physics.OverlapSphere(TargetPoint, 0.01F);

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

        if(isAbleToDoNextTask)
        {
            if(isSendingDistressSignal)
            {
                PatrollingAroundFood();

                return;
            }

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
            //Debug.Log("No food in nest");

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
            Debug.Log(transform.name + " hasn't found any food - relocating food");

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
            Debug.Log("No Food left.");

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

            isPatrolling = false;

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

            _InteractCollider.enabled = true;

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

        if(other.GetComponent<Food>())
        {
            Food _MainFood = other.GetComponent<Food>();

            if (_MainFood.transform == other.transform)
            {
                MovementSpeedManagement(false, 0);
                Agent260.velocity = Vector3.zero;
                //Debug.Log(transform.name + " Check interaction ");

                if (_MainFood.isHeavy && !_MainFood.IsInNest)
                {
                    if (_MainFood.isBeingLocated == false)
                    {
                        Debug.Log("Food is Heavy!!");

                        _MainFood.isBeingLocated = true;

                        MainTarget = _MainFood.transform;
                        


                        DistressSignal(_MainFood);
                    }
                    else
                    {
                        Debug.Log(transform.name + " processing breaking down " + _MainFood.name);

                        OnBreakingDownFood(_MainFood);
                    }
                }
                else if (_MainFood.IsBeingCarried)
                {
                    if (isHelpingCarryAnObject) return;

                    Debug.Log(transform.name + " Initiate helping to carry  ");

                    _InteractCollider.enabled = false;

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

    #region Break down Mechanism
    void OnBreakingDownFood(Food food)
    {
        //food.MainCarryAnt = transform;
        _InteractCollider.enabled = false;

        food.ProcessingFoodBreakdown(CarryPoint,this);
    }
    #endregion

    #region Distress Signal
    void DistressSignal(Food foodie)
    {
        Debug.Log(transform.name + " sending a distress signal...");

        isSendingDistressSignal = true;

        DistressSignalManager DS = Instantiate(DistressSignalObject.GetComponent<DistressSignalManager>(), transform.position, Quaternion.identity);

        DS.TargetedFood = foodie.transform;

        DS.DistressSender = transform;

        PatrollingAroundFood();
    }
    #endregion

    #region Distress Behaviour - Secure Perimeter
    void PatrollingAroundFood()
    {
        //Debug.Log(_PatrolsAmount);
        if (_PatrolsAmount <= 0)
        {
            Debug.Log(transform.name + " is done securing the perimeter");

            isPatrolling = false;

            isSendingDistressSignal = false;

            BreakDownFood();

            _PatrolsAmount = PatrolsAmount;
        }
        else
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
                if (i.gameObject.tag == "Food") // may add more in the future
                {
                    AbleToMove = false;
                }
                else if (i.gameObject.tag == "Ant")
                {
                    AbleToMove = false;
                }
            }

            if (AbleToMove)
            {
                Debug.Log(transform.name + " is Patrolling");

                _PatrolsAmount -= 1;

                Agent260.SetDestination(V);

                isPatrolling = true;

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
                Debug.Log(transform.name + " is not able to Patrol");
                Debug.DrawLine(transform.position, V, Color.red, 5f);

                Invoke("PatrollingAroundFood", 1f);
            }
        }
    }
    #endregion

    #region Breaking Down Food
    void BreakDownFood()
    {
        Debug.Log(transform.name + " going to break down food " + MainTarget.name);

        MovementSpeedManagement(true, 0);

        Agent260.SetDestination(MainTarget.position);
    }
    #endregion

    #region Carry Mechanic
    void MovingObjectMechanism()
    {
        if(MainTarget!=null)
        {
            if(MainTarget.GetComponent<Food>())
            {
                Food _MainFood = MainTarget.GetComponent<Food>();

                _MainFood.MainCarryAnt = transform;

                OnCarryPointgetTarget(_MainFood.transform);

                _MainFood.MoveTowards(CarryPoint);

                Debug.Log(transform.name + " Moving object");
            }        
        }
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

    public void OnCarryPointgetTarget(Transform ObjectToBeCarry)
    {
        float SizeArea = CarryRange* (ObjectToBeCarry.localPosition.z + ObjectToBeCarry.localPosition.y + ObjectToBeCarry.localPosition.x);

        CarryPoint.localPosition = new Vector3(CarryPoint.localPosition.x, SizeArea,CarryPoint.localPosition.z );
    }

    public void OnReceivedSignals(Transform _targetedFood)
    {
        isIdling = false;

        MainTarget = _targetedFood;

        OffMainTargetManager = false;

        MovementSpeedManagement(true, 0);

        Agent260.SetDestination(_targetedFood.position);
    }
}
