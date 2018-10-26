using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;

public class TRex : MonoBehaviour
{
    [Header("Timer")]
    public float StartRoam;
    public float RepeatRoam;

    [Header("Hunger")]
    public float HungerTime;
    public float HungerRoamTime;
    public float MinimumHungerCapacity;
    public float DetectFoodRange;
    public float EatingRange;
    public float ConsumeFoodTime;

    [Header("Roam Setup")]
    public float RoamRange;
    public float StopDistance;

    [Header("Attributes")]
    public float FullStomach = 100;
    public float MovementSpeed;
    public float UnitTurnSpeed;
    public float Condition = 20f;
    public float MinusCondition = 2f;
    public float SleepTime;
    // 1. Walk around the Area
    // 2. Mating
    // 3. Give Birth/Lay Eggs
    // 4. Eat & Hunger
    // 5. Sleep
    // 6. Attack
    // 7. Run
    // 8. Panic/ Feels : happy/sad/etc
    // 9. Idle
    // 10. Growth
    public Transform MainDestination;
    public NavMeshAgent Agent;
    public Text conditionText;
    public Text feelingText;
    public GameObject _pO;
    public bool isIdle;
    public bool isHungry;
    public bool SawFood;
    public bool isEating;
    public bool isSuitableToGiveBirth;
    public bool NeedSleep;
    public bool MovementStop;

	void Start ()
    {
        Agent.speed = MovementSpeed;

        Invoke("Roam",StartRoam);
        Invoke("StomachDigestion", HungerTime);
	}

    void StomachDigestion()
    {
        if (NeedSleep == true) return;

        if(isEating)
        {

        }
        else
        {
            FullStomach -= 10f;
        }

        if (FullStomach <= 0f)
        {
            //Panic and Hungry

            isHungry = true;
        }
        else if(FullStomach <= MinimumHungerCapacity)
        {
            //Looking for food
            if(isHungry == false)
            {
                isHungry = true;

                CancelInvoke("Roam");

                Invoke("HungerRoam", 1f);
            }
            else
            {

            }
        }

        Invoke("StomachDigestion", HungerTime);
    }

    void HungerRoam()
    {
        conditionText.text = "Looking for Food";
        feelingText.text = "Hungry";

        Collider[] Col = Physics.OverlapSphere(transform.position, DetectFoodRange);

        foreach (Collider _object in Col)
        {
            if (_object.gameObject.tag == "Canivore" || _object.gameObject.tag == "Herbivore" || _object.gameObject.tag == "Food")
            {
                if(SawFood==false)
                {
                    MainDestination = _object.gameObject.transform;

                    SawFood = true;
                }
            }
        }

        if(SawFood==false)
        {
            CheckCondition(false);

            float RandomX, RandomZ;
            RandomX = Random.Range(-RoamRange, RoamRange);
            RandomZ = Random.Range(-RoamRange, RoamRange);

            Debug.DrawLine(transform.position, new Vector3(RandomX, 0.2f, RandomZ), Color.red, 5f);

            GameObject P = Instantiate(_pO, new Vector3(RandomX, 0.2f, RandomZ), Quaternion.identity);

            MainDestination = P.transform;

            Agent.speed = MovementSpeed;

            Agent.SetDestination(MainDestination.position);

            Invoke("HungerRoam", HungerRoamTime);
        }
        else
        {
            Agent.speed = MovementSpeed;

            Agent.SetDestination(MainDestination.position);
        }
    }

    void CheckCondition(bool isGood)
    {
        if(isGood)
        {
            Condition += MinusCondition;
        }
        else
        {
            Condition -= MinusCondition;
        }

        if(Condition >= 100)
        {
            if(isSuitableToGiveBirth==false)
            {
                MovementSlowed();

                isSuitableToGiveBirth = true;

                //Invoke("GivingBirth", 2f);
            }
        }
        else
        {

        }

    }

    void MovementSlowed()
    {
        MovementStop = true;
    }

    void GivingBirth()
    {
        conditionText.text = "Giving Birth";
        feelingText.text = "Normal";

        CancelInvoke("StomachDigestion");
        CancelInvoke("Roam");

        MovementStop = false;

        Agent.speed = 0;
        Agent.velocity = Vector3.zero;

        StartCoroutine(SpawningNewBorn());
    }

    private IEnumerator SpawningNewBorn()
    {
        yield return new WaitForSeconds(5f);

        Condition = 0;
        isSuitableToGiveBirth = false;
        NeedSleep = true;

        Invoke("StomachDigestion", HungerTime);

        conditionText.text = "Sleeping";
        feelingText.text = "Weak";

        Invoke("WakingUp", SleepTime);
    }

    void WakingUp()
    {
        conditionText.text = "Normal";
        feelingText.text = "Normal";

        NeedSleep = false;

        
        Invoke("Roam", RepeatRoam);
    }

    void Roam()
    {
        conditionText.text = "Roaming...";

        CheckCondition(true);

        Agent.speed = MovementSpeed;

        float RandomX, RandomZ;
        RandomX = Random.Range(-RoamRange, RoamRange);
        RandomZ = Random.Range(-RoamRange, RoamRange);

        Debug.DrawLine(transform.position, new Vector3(RandomX,0.2f,RandomZ), Color.red, 5f);

        GameObject P = Instantiate(_pO, new Vector3(RandomX, 0.2f, RandomZ), Quaternion.identity);

        MainDestination = P.transform;

        Agent.SetDestination(P.transform.position);

        Invoke("Roam", RepeatRoam);
    }


    private void FixedUpdate()
    {
        if(MainDestination==null)
        {

        }
        else
        {
            if(MovementStop)
            {
                if(Agent.speed <= 0)
                {
                    if(isSuitableToGiveBirth == true)
                    {
                        GivingBirth();
                    }
                    
                }
                else
                {
                    Agent.speed -= Time.deltaTime;
                }
            }
            else
            {
                if (SawFood)
                {
                    if (Vector3.Distance(transform.position, MainDestination.position) < EatingRange)
                    {
                        Vector3 dir = MainDestination.transform.position - transform.position;
                        Quaternion lookRotation = Quaternion.LookRotation(dir);
                        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.smoothDeltaTime * UnitTurnSpeed).eulerAngles;
                        transform.rotation = Quaternion.Euler(0f, rotation.y, 0);


                        if (isEating == false)
                        {
                            isEating = true;

                            Eating();
                        }

                        Destroy(MainDestination.gameObject);
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, MainDestination.position) < StopDistance)
                    {
                        if (isIdle == false)
                        {
                            isIdle = true;

                            Idle();
                        }

                        Destroy(MainDestination.gameObject);
                    }
                    else
                    {
                        if (isIdle == true)
                        {
                            isIdle = false;
                        }
                    }
                }
            }
            
        }
    }

    void Eating()
    {
        conditionText.text = "Eating...";

        CheckCondition(true);

        Agent.velocity = Vector3.zero;
        Agent.speed = 0f;

        Invoke("AddingFoodToStomach", 1f);
    }

    void AddingFoodToStomach()
    {
        if(FullStomach < 100)
        {
            FullStomach += 10f;

            Invoke("AddingFoodToStomach", ConsumeFoodTime);
        }
        else
        {
            isHungry = false;
            SawFood = false;
            isEating = false;

            feelingText.text = "Joy";

            Invoke("Roam", StartRoam);
        }
    }

    void Idle()
    {
        conditionText.text = "Idle'ing...";

        Agent.velocity = Vector3.zero;
        Agent.speed = 0f;

        if (isSuitableToGiveBirth)
        {
            CheckCondition(true);
        }
    }
}
