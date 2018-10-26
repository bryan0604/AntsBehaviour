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

public class Ants : MonoBehaviour
{
    #region Variables

    private string Queen = "Queen";
    private string Soldier = "Soldier";

    [Header("RANK")]
    public string Rank;
    public bool isQueen;
    public bool isSoldier;
    [Range(0,100)]
    public float Growth = 0;
    public float GrowthSpeedScale = 0.01f;
    public Vector3 AntScaleSize;
    public Vector3 MaxAntScaleSize;

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

            Nest.singleton.AssignAntInfos(this);
        }
        

        HungerCapacityDefault = 100;

        //MovementSpeedDefault = MovementSpeed;

        FoodDigestion(true);

        InvokeRepeating("CheckConditionAndFeeling", CheckConditionInTime / 2, CheckConditionInTime);

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
            Debug.Log("Wander around the map");

            InvokeRepeating("WanderRepeat", 0f, WanderRepeatInTime);
            
        }
        else
        {

            Debug.Log("Cancelled Wander");

            CancelInvoke("WanderRepeat");

            return;
        }
    }

    void WanderRepeat()
    {
        if (Moving == true) return;
        else
        {
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
        //if (FoodisInRange) return;

        Debug.Log(transform.name+" Searching for Food");

        Collider[] Foods = Physics.OverlapSphere(transform.position, FoodSmellRange);

        List<Transform> FoodList = new List<Transform>();

        //if (Foods == null) return;

        foreach (var food in Foods)
        {
            //Debug.Log(food.gameObject.name);

            if (isHungry)
            {
                //Debug.Log(transform.name);

                if (food.tag == FoodTag && food.GetComponent<Food>().IsInNest)
                {
                    if (food.GetComponent<Food>().IsBeingCarried)
                    {
                        Debug.Log(food.name + " is being carried therefore /eat ");
                    }
                    else
                    {
                        Debug.Log(" Is Hungry for " + food.name + " = " + isHungry);

                        FoodList.Add(food.transform);

                        FoodisInRange = true;

                        //MoveTowardsTargetedPosition(food.transform);
                        MoveTowardsTargetedPosition(FoodList[Random.Range(0, FoodList.Count)].transform);
                    }
                }
                else
                {
                    //Debug.Log("S");
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
                            Debug.Log(food.name + " is being carried therefore /maybe help carry? ");

                            isMovingObject = false;
                        }
                        else
                        {
                            Debug.Log(food.name + " is not in Nest = " + food.GetComponent<Food>().IsInNest);

                            FoodList.Add(food.transform);

                            FoodisInRange = true;

                            //MoveTowardsTargetedPosition(food.transform);
                            MoveTowardsTargetedPosition(FoodList[Random.Range(0, FoodList.Count)].transform);
                        }
                    }
                    else
                    {
                        Debug.Log(food.name + " is in Nest =" + food.GetComponent<Food>().IsInNest);
                    }
                }
            }
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
        GrowthSystem();

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
            Agent707.speed = MovementSpeed;

            Indications = Color.green;

            if(Moving == false)
            {
                Moving = true;
            }
        }

        if(isDoingDoubleChecker)
        {
            if(_doubleInteraction.localPosition.z >= 1)
            {

            }
            else
            {
                _doubleInteraction.Translate(0, 0, DC_movementSpeed*Time.deltaTime, Space.Self);
            }
            
        }
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
            //Debug.Log("Interact Nothing");

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
                        if (isFeeding == false)
                        {
                            isFeeding = true;

                            Debug.Log("Interact with = " + o.gameObject.name);

                            ActivateFeedingMechanism(true);
                            //FoodPlatformManagement(o.gameObject.transform);
                        }
                    }
                    else
                    {
                        Debug.Log("Do nothing");
                    }
                }
//------------------------------------------------------------------------------------------------------
                if (isOnScoutMode)
                {
                    if(o.GetComponent<Food>()!=null)
                    {
                        Food f = o.GetComponent<Food>();

                        if (o.tag == FoodTag)
                        {
                            if (isMovingObject == false)
                            {
                                Debug.Log("Processing to Move Object = " + o.name);

                                ActivateMovingObjectMechanicsm(o);
                            }
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

                isMovingObject = true;

                CanWander = false;

                Wander();

                FoodisInRange = false;

                isOnScoutMode = false;

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

            CanWander = true;

            Wander();
        }
    }

    #endregion

    #region 
    void TravelBackToNest()
    {
        isGoingBackHome = true;

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
        else
        {
            ObjectsInteracted.Add(Markus.transform);
        }
    }

    private void OnTriggerExit(Collider Celine)
    {
        if(Celine.gameObject.tag == "Nest")
        {
            isInNestZone = false;
        }
        else
        {
            ObjectsInteracted.Remove(Celine.transform);
        }
    }
    #endregion

    #region Food Digestion Sector
    void FoodDigestion(bool BeginDigestion)
    {
        isDigestingFood = BeginDigestion;
        Debug.Log("Digesting Food = "+BeginDigestion);

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

        if(isOnScoutMode)
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
    }
}
