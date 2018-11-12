using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform FoodPrefab;

    [Header("Timer")]
    public float FoodSpawn = 10;

    [Header("SpawnSize")]
    public float Radius=8f;

    public bool ActivateSpawn;

    public List<Food> AllFoods = new List<Food>();

    public static FoodManager singleton;

    private string SpawnFoodSectorStr = "SpawnFoodSector";

	void Start ()
    {
        singleton = this;

        FoodCheck();

        if (ActivateSpawn == false) return;

        InvokeRepeating(SpawnFoodSectorStr,FoodSpawn,FoodSpawn);
	}
	
    void SpawnFoodSector()
    {
        float RandomX, RandomZ;

        RandomX = Random.Range(-Radius, Radius);
        RandomZ = Random.Range(-Radius, Radius);

        Debug.DrawLine(transform.position, new Vector3(transform.position.x + RandomX, transform.position.y ,transform.position.z + RandomZ),Color.red,5f );

        GameObject Food = Instantiate(FoodPrefab.gameObject, new Vector3(transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ),Quaternion.identity);
    }

    private void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            FoodCheck();
        }
    }

    public void FoodCheck()
    {
        Debug.LogWarning("All Foods condition are updated");

        CancelInvoke(SpawnFoodSectorStr);

        foreach (var item in AllFoods)
        {
            if(item.GetComponent<Food>())
            {
                Food f = item.GetComponent<Food>();

                f.FoodCheck();
            }
        }  
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
