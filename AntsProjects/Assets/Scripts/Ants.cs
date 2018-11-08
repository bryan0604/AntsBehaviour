using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

//ideas

    // 1. protect the queen
    // 2. attack other race
    // 3. protect eggs
    // 4. certain time outside will be back to nest
    // 5. to Swarm or to Fetch back Food?

public class Ants : MonoBehaviour
{
    #region Variables

    private string Queen = "Queen";
    private string Soldier = "Soldier";

    [Header("RANK")]
    public string Rank;
    public bool isQueen;
    public bool isSoldier;
    public bool isGuardian;
    [Range(0,100)]
    public float Growth = 0;
    public float GrowthSpeedScale = 0.01f;
    public Vector3 AntScaleSize;
    public Vector3 MaxAntScaleSize;
    public Vector3 CurrentPosition;
    public Vector3 PreviousPosition;

    [Header("Attributes Setup")]
    [Range(0,2)]
    public float StoppingDistance = 0.5f;
    //public float InteractionDistance=1f;
    public float MovementSpeed;
    public float DC_movementSpeed=2f;
    //private float MovementSpeedDefault;
    public float HungerRequirement = 50;
    public float HungerCapacity = 100;
    private float HungerCapacityDefault;
    public float HungerSpeedTime = 2f;
    public float HungerCrement = 2f;
    public float FoodSmellRange = 5f;
    public float FeedingPerSec = 2f;
    public float CheckConditionInTime = 2f;
    public float WanderArea = 3f;
    public float WanderRepeatInTime = 5f;
    public float SpawningEggsTime = 5f;
    public float RotateSpeed = 5f;
    public float AngleTT;
    public float PreviousPosCheck = 5f;
    public int BirthPoint = 5;
    public int CurrentCondition = 5;
    public int CurrentFeeling = 5;
    public Text ConditionText;
    public Text FeelText;
    [Space]
    [Header("Boolean")]
    public bool isHungry;
    public bool FoodisInRange;
    public bool isDigestingFood = true;
    public bool isFeeding;
    public bool Moving;
    public bool CanWander = true;
    public bool CanGiveBirth = false;
    public bool isFacingTarget = false;
    public bool isOnFoodPlatform = false;
    public bool isInNestZone = true;
    public bool isOnScoutMode = false;
    public bool isMovingObject = false;
    public bool isGoingBackHome = false;
    public bool isDoingDoubleChecker = false;
    public bool isAssistingMovingobject = false;
    public bool CheckPosition = false;
  
    [ColorUsage(true)]
    public Color32 Indications;
    public string FoodTag = "Food";
    public string FoodPlatformTag = "FoodPlatform";
    public Transform MainTarget;
    public Transform Eggs;
    public Transform BirthPosition;
    public Transform HomeNest;
    public Transform InteractionPoint;
    public Transform _doubleInteraction;
    public List<Transform> ObjectsInteracted;
    public NavMeshAgent Agent707;
    public Coroutine WanderCo;
    public IEnumerator WanderIE;
    //private Vector3 doubleCheckerDPos;

    #endregion

    private void Start()
    {
        if(Agent707!=null)
        {
            Agent707.speed = MovementSpeed;
        }
        else
        {
            Debug.LogError("Agent Not Found!");
        }


        if(isQueen)
        {
            Rank = Queen;

            AntScaleSize = transform.localScale;
        }
        else if(isSoldier)
        {
            AntScaleSize = transform.localScale;

            

            Rank = Soldier;
        }

        if(HomeNest==null)
        {
            HomeNest = Nest.singleton.transform;

            //Nest.singleton.AssignAntInfos(this);
        }

        //doubleCheckerDPos = _doubleInteraction.transform.localPosition;

        HungerCapacityDefault = 100;

        //MovementSpeedDefault = MovementSpeed;

        FoodDigestion(true);

        InvokeRepeating("CheckConditionAndFeeling", CheckConditionInTime / 2, CheckConditionInTime);

        InvokeRepeating("ActivatePositionCheck", PreviousPosCheck, PreviousPosCheck);

        if (HungerCapacity < HungerRequirement)
        {
            isHungry = true;

            CanWander = false;

            SmellForFood();
        }
        else
        {
            CanWander = true;

            Wander();
        }
    }

    #region Wander Sector
    void Wander()
    {
        if(CanWander)
        {
            Debug.Log(transform.name +  " Wander around the map");

            InvokeRepeating("WanderRepeat", 0f, WanderRepeatInTime);
            
        }
        else
        {

            Debug.Log(transform.name + " Cancelled Wander");

            CancelInvoke("WanderRepeat");

            return;
        }
    }

    void WanderRepeat()
    {
        if (Moving == true) return;
        else
        {
            Agent707.speed = MovementSpeed;

            if (!isInNestZone && isQueen)
            {
                Moving = true;

                MainTarget = null;

                TravelBackToNest();
            }
            else
            {
                Moving = true;

                MainTarget = null;

                float AreaX, AreaZ;
                AreaX = Random.Range(-(WanderArea), (WanderArea));
                AreaZ = Random.Range(-(WanderArea), (WanderArea));

                Vector3 V = new Vector3(transform.position.x + AreaX, 0.5f, transform.position.z + AreaZ);

                Debug.DrawLine(transform.position, V, Color.cyan, 5f);

                Agent707.SetDestination(V);

                if (CanGiveBirth && isQueen)
                {
                    CancelInvoke("WanderRepeat");

                    GivingBirth();
                }
            }
            #region UNUSED

            #endregion

        }
    }

    #endregion

    #region Smells for potential Food / Opponent
    void SmellForFood()
    {
        if (FoodisInRange) return;

        bool HaveDestination = false;

        Debug.Log(transform.name+" Searching for Food");

        Collider[] Foods = Physics.OverlapSphere(transform.position, FoodSmellRange);

        List<Transform> FoodList = new List<Transform>();

        //if (Foods == null) return;

        foreach (var food in Foods)
        {
            if(food==null)
            {
                
            }

            if (isHungry)
            {
                //Debug.Log(transform.name);

                if (food.tag == FoodTag && food.GetComponent<Food>().IsInNest)
                {
                    if (food.GetComponent<Food>().IsBeingCarried)
                    {
                        Debug.Log(food.name + " is being carried therefore /eat ");
                    }
                    else if(food.GetComponent<Food>().FoodCapacity <= 0)
                    {
                        Debug.Log(food.name + " is Decayed no Food left!");
                    }
                    else
                    {
                        Debug.Log(transform.name+" Is Hungry for " + food.name + " = " + isHungry);

                        FoodList.Add(food.transform);

                        FoodisInRange = true;

                        HaveDestination = true;

                        //MoveTowardsTargetedPosition(FoodList[Random.Range(0, FoodList.Count)].transform); // might need to move this out
                    }
                }
                else if(food.tag == FoodTag && !food.GetComponent<Food>().IsInNest)
                {
                    //May implement call for help ants in nest
                    Debug.LogWarning(this.transform.name + " No Action here");
                }
            }
            else if(isOnScoutMode)
            {
                //Debug.Log(transform.name);

                if (food.GetComponent<Food>())
                {
                    if (food.tag == FoodTag && !food.GetComponent<Food>().IsInNest)
                    {
                        if(food.GetComponent<Food>().IsBeingCarried)
                        {
                            Debug.Log(transform.name + " - " +food.name + " is being carried therefore /maybe help carry? ");

                            AssistMovingPO(food.transform);

                            isMovingObject = false;
                        }
                        else if (food.GetComponent<Food>().FoodCapacity <= 0)
                        {
                            Debug.Log(food.name + " is Decayed no Food left!2");
                        }
                        else
                        {
                            //Debug.Log(food.name + " is not in Nest = " + food.GetComponent<Food>().IsInNest);

                            FoodList.Add(food.transform);

                            FoodisInRange = true;

                            HaveDestination = true;

                            //MoveTowardsTargetedPosition(FoodList[Random.Range(0, FoodList.Count)].transform); // might need to move this out
                        }
                    }
                    else
                    {
                        //Debug.Log(food.name + " is in Nest =" + food.GetComponent<Food>().IsInNest);
                    }
                }
            }
        }

        if (HaveDestination)
        {
            MoveTowardsTargetedPosition(FoodList[Random.Range(0, FoodList.Count)].transform);
        }
        else
        {
            Debug.Log(transform.name + " have No Destination ");
        }

    }
    #endregion

    #region Move to Destination

    void MoveTowardsTargetedPosition(Transform TargetedObject)
    {
        Debug.Log(transform.name + " is moving to " + TargetedObject);

        Moving = true;

        MainTarget = TargetedObject;

        Agent707.SetDestination(TargetedObject.position);
    }

    #endregion

    #region Condition and Feeling for Birth
    void CheckConditionAndFeeling()
    {
        if (HungerCapacity < HungerRequirement)
        {
            OnConditionsChanged(-1);
        }
        else
        {
            OnConditionsChanged(1);
        }
    }

    void OnConditionsChanged(int ConditionRating)
    {
        CurrentCondition += ConditionRating;

        if(CurrentCondition >= BirthPoint && isQueen)
        {
            CanGiveBirth = true;
        }
    }
    #endregion

    #region Give Birth
    void GivingBirth()
    {
        Debug.Log("Giving Birth");

        FoodDigestion(false);

        CanGiveBirth = false;

        CurrentCondition = 0;

        CanWander = false;

        Invoke("SpawningNewBorn", SpawningEggsTime);
    }

    void SpawningNewBorn()
    {
        Debug.Log("Spawning Eggs");

        Instantiate(Eggs.gameObject, BirthPosition.position, Quaternion.identity);

        //Done

        //Resume Wander

        CanWander = true;

        FoodDigestion(true);

        Wander();
    }
    #endregion

    #region Fixed Update
    private void FixedUpdate()
    {
        CurrentPosition = transform.position;

        GrowthSystem();

        if (isAssistingMovingobject)
        {
            if (MainTarget == null)
            {
                Debug.Log(transform.name+ " No Object to move...nothing to do");

                isAssistingMovingobject = false;

            }
            else
            {
                Debug.LogError(transform.name +"A - here may cause ant stop moving");
                Agent707.SetDestination(MainTarget.position);
            }
        }

        if(Agent707.remainingDistance <= StoppingDistance)
        {
            Agent707.speed = 0f;
            Agent707.velocity = Vector3.zero;

            if (Agent707.speed == 0 && MainTarget !=null)
            {
                RotateToCorrectAngle();
            }

            Indications = Color.red;

            #region Checks After reaching Destination (Run Once)

            if(Moving == true) // things below this is only called once   <<<<<<<<<<<<<<<<
            {
                Moving = false;

                //Debug.Log("Reached Destination");
                if (isAssistingMovingobject)
                {
                    if(MainTarget==null)
                    {
                        Debug.Log(transform.name + " No Object to move...nothing to do2");
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, MainTarget.position) > StoppingDistance)
                        {
                            Debug.Log(transform.name + " distance far from = " + MainTarget.name);
                        }
                        else
                        {
                            //Debug.Log(transform.name + " distance near from = " + MainTarget.name);

                            if(MainTarget.GetComponent<Food>())
                            {
                                Food f = MainTarget.GetComponent<Food>();

                                if(f.IsBeingCarried == false)
                                {
                                    Debug.Log(f.name + " is being placed on the ground");

                                    isAssistingMovingobject = false;

                                    CanWander = true;

                                    Wander();
                                }
                            }
                        }
                    }
                }

                #region Check for the Distant of Nest between this Transform ant
                if (Vector3.Distance(transform.position, HomeNest.position) > Nest.singleton.NestSize)
                {
                    isInNestZone = false;

                    if(!isQueen)
                    {
                        isOnScoutMode = true;
                    }

                    //Debug.Log("  Far  ");
                }
                else
                {
                    isInNestZone = true;

                    isOnScoutMode = false;

                    //Debug.Log("  Close ");
                }
                #endregion

                #region Place Potential Object on the Ground (In Nest)

                if(isGoingBackHome)
                {
                    isGoingBackHome = false;

                    PlacingPotentialObject();
                }

                #endregion

                CheckInteractions();
            }

            #endregion

        }
        else
        {
            if(isMovingObject)
            {
                Agent707.speed = 0.5f;
            }
            else
            {
                Agent707.speed = MovementSpeed;
            }

            Indications = Color.green;

            if(Moving == false)
            {
                Moving = true;
            }
        }

        //if(isDoingDoubleChecker)
        //{
        //    if(_doubleInteraction.localPosition.z >= 1)
        //    {
        //        _doubleInteraction.localPosition = doubleCheckerDPos;
        //    }
        //    else
        //    {
        //        _doubleInteraction.Translate(0, 0, DC_movementSpeed*Time.deltaTime, Space.Self);
        //    }

        //}
        
    }
    #endregion

    #region Unit Stuck Sector
    void ActivatePositionCheck()
    {
        CheckPosition = true;
        
        StartCoroutine(PositionCheck());

    }

    private IEnumerator PositionCheck()
    {
        yield return new WaitForSeconds(PreviousPosCheck);


        CheckPosition = false;
    }

    #endregion

    #region Rotation - Transform Corrections
    void RotateToCorrectAngle()
    {
        Vector3 dir = MainTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.smoothDeltaTime * RotateSpeed).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0);
    }
    #endregion

    #region Interaction 
    void CheckInteractions()
    {
        if(ObjectsInteracted.Count==0 || ObjectsInteracted==null)
        {
            //Debug.Log("Interact Nothing - CI");

            return;
        }

        foreach(Transform o in ObjectsInteracted)
        {
            if(o==null)
            {
                ObjectsInteracted.Remove(o);
            }
            else
            {
                if(isHungry)
                {
                    if (o.tag == FoodTag && isHungry)
                    {
                        Food f = o.GetComponent<Food>();

                        if (isFeeding == false)
                        {
                            isFeeding = true;

                            Debug.Log("Interact with = " + f.gameObject.name);

                            ActivateFeedingMechanism(true);
                            //FoodPlatformManagement(o.gameObject.transform);
                        }
                        else
                        {
                            Debug.LogWarning(transform.name + " Do nothing");
                        }
                    }
                    else
                    {
                        Debug.LogWarning(transform.name + " Do nothing");
                    }
                }
//------------------------------------------------------------------------------------------------------
                if (isOnScoutMode)
                {
                    if(o.GetComponent<Food>()!=null)
                    {
                        Food f = o.GetComponent<Food>();

                        if (f.IsBeingCarried) return;

                        if (f.tag == FoodTag)
                        {
                            if (isMovingObject == false)
                            {
                                Debug.Log(transform.name + " Processing to Move Object = " + f.name);

                                ActivateMovingObjectMechanicsm(f.transform);
                            }
                            else
                            {
                                Debug.LogWarning(transform.name + " Do nothing");
                            }
                        }
                        else
                        {
                            Debug.LogWarning(transform.name + " Do nothing");
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Shift and Place Potential Object To Nest
    void ActivateMovingObjectMechanicsm(Transform PotentialObject)
    {
        if(PotentialObject.GetComponent<Food>())
        {
            Food Food = PotentialObject.GetComponent<Food>();

            if (Food.IsBeingCarried == false)
            {
                Food.IsBeingCarried = true;

                Agent707.speed = 0.5f;

                isMovingObject = true;

                CanWander = false;

                Wander();

                FoodisInRange = false;

                isOnScoutMode = false;

                isAssistingMovingobject = false;

                PotentialObject.position = InteractionPoint.position;

                PotentialObject.SetParent(InteractionPoint);

                Invoke("TravelBackToNest", 2f);
            }
            else
            {
                Debug.Log( Food.name + " Is already being Carried = " + Food.IsBeingCarried);
            }
        }

        // May want to disable growth due to PotentialObject size affected
    }

    void PlacingPotentialObject()
    {
        if(InteractionPoint.childCount == 0)
        {
            
        }
        else
        {
            Debug.Log(this.transform.name + " is Placing Potential Object on the ground = " + InteractionPoint.GetChild(0).name);

            Food food = InteractionPoint.GetChild(0).GetComponent<Food>();

            InteractionPoint.GetChild(0).SetParent(null);

            food.IsBeingCarried = false;

            MainTarget = null;

            isMovingObject = false;

            Moving = false;

            Agent707.speed = MovementSpeed;

            CanWander = true;

            Wander();
        }
    }

    #endregion

    #region Assit Shift and Place Potential Object
    void AssistMovingPO(Transform _PO)
    {
        isAssistingMovingobject = true;

        MainTarget = _PO;

        CanWander = false;

        Wander();// check here for FoodIsInrange

        //Debug.Log(this.transform.name + " is assiting moving PO");
        FoodisInRange = false;

        Moving = true;

        Agent707.SetDestination(_PO.position);
    }
    #endregion

    #region Travel Home
    void TravelBackToNest()
    {
        isGoingBackHome = true;

        Moving = true;

        var X = Nest.singleton.X;
        //var Y = 0f;
        var Z = Nest.singleton.Z;


        float RandomX = Random.Range(-X, X);
        //float RandomY = Random.Range(-Y, Y);
        float RandomZ = Random.Range(-Z, Z);

        Debug.DrawLine(transform.position, new Vector3(RandomX, 0.5f, RandomZ), Color.cyan, 5f);

        Agent707.SetDestination(new Vector3(RandomX, 0.5f, RandomZ));
    }
    #endregion

    #region Feeding Sector

    void ActivateFeedingMechanism(bool ActivateMechanism)
    {
        if(ActivateMechanism)
        {
            Debug.Log("is Feeding");

            FoodDigestion(false);

            CanWander = false;

            Wander();

            CancelInvoke("Feeding");
            InvokeRepeating("Feeding", FeedingPerSec, FeedingPerSec);
        }
        else
        {
            Debug.Log("Cancelling Feeding");

            CancelInvoke("Feeding");

            FoodDigestion(true);

            isHungry = false;

            FoodisInRange = false;

            isFeeding = false;

            MainTarget = null;

            isFacingTarget = false;

            CanWander = true;

            isOnFoodPlatform = false;

            Wander();

            //Wander();
        }
    }

    void Feeding()
    {
        DoubleChecker(true);

        HungerCapacity += HungerCrement;

        if (HungerCapacity >= HungerCapacityDefault)
        {
            Debug.Log("Ant is full");

            HungerCapacity = HungerCapacityDefault;

            ActivateFeedingMechanism(false);
        }
    }
    #endregion

    #region Collision Detect
    private void OnTriggerEnter(Collider Markus)
    {
        if (Markus.gameObject.tag == "Nest")
        {
            isInNestZone = true;
        }
        else if (Markus.gameObject.tag == "Food")
        {
            ObjectsInteracted.Add(Markus.transform);
        }
        else
        {
            
        }
    }

    private void OnTriggerExit(Collider Celine)
    {
        if(Celine.gameObject.tag == "Nest")
        {
            isInNestZone = false;
        }
        else if (Celine.gameObject.tag == "Food")
        {
            ObjectsInteracted.Remove(Celine.transform);
        }
        else
        {
            
        }
    }
    #endregion

    #region Food Digestion Sector
    void FoodDigestion(bool BeginDigestion)
    {
        isDigestingFood = BeginDigestion;
        Debug.Log(transform.name + " Digesting Food = "+BeginDigestion);

        if(BeginDigestion)
        {
            InvokeRepeating("Digest", HungerSpeedTime, HungerSpeedTime);
        }
        else
        {
            CancelInvoke("Digest");
        }
    }

    void Digest()
    {
        HungerCapacity -= HungerCrement;

        if(isAssistingMovingobject)
        {
            //Debug.Log(this.transform.name + " Still moving Potential Object");
        }
        else if(isOnScoutMode)
        {
            Debug.Log(transform.name +" Scout - Searching for Potential..");

            if (HungerCapacity < HungerRequirement)
            {
                isHungry = true;
            }

            SmellForFood();
        }
        else
        {
            if (isMovingObject) return;

            if (HungerCapacity < HungerRequirement)
            {
                isHungry = true;

                CanWander = false;

                SmellForFood();
            }
            else
            {
                CanWander = true;
            }
        }
    }

    #endregion

    // if have food capacity then grow
    void GrowthSystem()
    {
        if (isQueen) return;

        if(AntScaleSize.magnitude >= MaxAntScaleSize.magnitude)
        {
            return;
        }

        AntScaleSize += new Vector3(GrowthSpeedScale, GrowthSpeedScale, GrowthSpeedScale) * Time.deltaTime;

        transform.localScale = AntScaleSize;
    }

    void DoubleChecker(bool _dc)
    {
        isDoingDoubleChecker = _dc;

        if (ObjectsInteracted.Count == 0 || ObjectsInteracted == null)
        {
            Debug.Log("Interact Nothing");

            if (isFeeding)
            {
                Debug.Log("Might need to disable feeding");

                ActivateFeedingMechanism(false);

            }
        }
        else
        {
            if (ObjectsInteracted.Count == 0 || ObjectsInteracted == null)
            {
                Debug.Log("Interact Nothing");

                return;
            }

            foreach (Transform o in ObjectsInteracted)
            {
                if (o == null)
                {
                    ObjectsInteracted.Remove(o);
                }
                else
                {
                    if (isHungry)
                    {
                        if (o.tag == FoodTag)
                        {
                            if(o.GetComponent<Food>().FoodCapacity <= 0)
                            {
                                Debug.Log("Deactivate Feeding");

                                ActivateFeedingMechanism(false);
                            }
                            else
                            {
                                //Debug.Log("Consuming Food");

                                o.GetComponent<Food>().ConsumingFood(1);
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
        }
    }
}
