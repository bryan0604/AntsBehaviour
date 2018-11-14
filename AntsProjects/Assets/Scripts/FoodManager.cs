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

    private string SpawnFoodSectorStr = "SpawnFoodSector";

	void Start ()
    {
        if (ActivateSpawn == false) return;

        InvokeRepeating(SpawnFoodSectorStr,FoodSpawn,FoodSpawn);
	}
	
    void SpawnFoodSector()
    {
        float RandomX, RandomZ;

        RandomX = Random.Range(-Radius, Radius);
        RandomZ = Random.Range(-Radius, Radius);

        Debug.DrawLine(transform.position, new Vector3(transform.position.x + RandomX, transform.position.y ,transform.position.z + RandomZ),Color.red,5f );

        Instantiate(FoodPrefab.gameObject, new Vector3(transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ),Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
