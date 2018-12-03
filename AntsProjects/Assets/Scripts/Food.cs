using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public float X, Y, Z;
    public float smoothSpeed;
    public float Kilograms;
    public int FoodCapacity = 30;
    public bool isHeavy;
    public bool ActivateVacuum;
    public bool IsInNest;
    public bool IsBeingCarried;
    public bool isSetOnCarriedPosition;
    public bool isBeingPlaced;
    public bool isDepeleted;
    public bool isBeingLocated;
    public string VoidFoodDecay = "FoodDecay";

    public Material myMats;
    public Material decaymats;

    public Transform Ant;
    public Transform MainCarryAnt;
    public Transform SmallPieceFood;
    public Vector3 offset;

    private void Start()
    {
        myMats = GetComponent<Material>();

        FoodWeightCheck();

        FoodCheck();

        X = transform.localScale.x / FoodCapacity;
        Y = transform.localScale.y / FoodCapacity;
        Z = transform.localScale.z / FoodCapacity;
        
    }

    public void ProcessingFoodBreakdown(Transform _carryPoint, AntsIntermediate _Ant)
    {
        Debug.LogWarning(transform.name + " processing break down...");

        Food _piecefood = Instantiate(SmallPieceFood.GetComponent<Food>(), transform.position, Quaternion.identity);

        _piecefood.MoveTowards(_carryPoint.transform);

        _piecefood.FoodCapacity = 999;

        _piecefood.MainCarryAnt = _Ant.transform;

        _Ant.MainTarget = _piecefood.transform;

        _Ant.OnCarryPointgetTarget(_piecefood.transform);

        //_carryPoint.localPosition = new Vector3(_carryPoint.localPosition.x, 1.5f, _carryPoint.localPosition.z);

        FoodCapacity -= 1;

        FoodWeightCheck();
    }

    public void FoodWeightCheck()
    {
        Kilograms = (transform.localScale.x + transform.localScale.y + transform.localScale.z);

        if(Kilograms > 1.5f)
        {
            isHeavy = true;
        }
        else
        {
            isHeavy = false;
        }

        Debug.LogWarning(transform.name + " is Heavy = " + isHeavy);
    }

    private void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            ConsumingFood(1);
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

    void OnScaleReduced()
    {
        if (Mathf.Round(transform.localScale.x * 10000f) / 10000f == Mathf.Round(X * 10000f) / 10000f)
        {
            
        }
        else
        {
            transform.localScale -= new Vector3(X, Y, Z);
        }
    }

    public void FoodCheck()
    {
        Vector3 A = new Vector3(transform.position.x, transform.localPosition.y-2f, transform.position.z);
        Vector3 B = new Vector3(transform.position.x, transform.localPosition.y+2f, transform.position.z);
        
        Collider[] col = Physics.OverlapCapsule(A,B, transform.localScale.z/2f);
     
        foreach (var i in col)
        {
            if(i.GetComponent<Nest>())
            {
                IsInNest = true;

                return;
            }
            else
            {
                IsInNest = false;

                //isBeingPlaced = false;
            }
        }
    }

    public void ConsumingFood(int _Quantity)
    {
        FoodCapacity -= _Quantity;

        OnScaleReduced();

        if (FoodCapacity <= 0)
        {
            transform.name = "Decayed Food";

            GetComponent<Renderer>().material = decaymats;

            isDepeleted = true;

            Invoke(VoidFoodDecay, 5f);
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

    public void OnBeingPlacedToTheGround()
    {
        IsBeingCarried = false;

        ActivateVacuum = false;

        Ant = null;

        isSetOnCarriedPosition = false;

        isBeingPlaced = true;
    }
}
