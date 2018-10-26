using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eggs : MonoBehaviour
{
    public int BornTime = 5;
    [Header("Not Touch")]
    public float BornTimeFloat;
    public bool isSpawned;
    public Transform NewBornLarvae;

    private void Start()
    {
        BornTimeFloat = (float)BornTime;
    }

    private void Update()
    {
        if (isSpawned) return;
        
        if(BornTimeFloat <= 0f)
        {
            isSpawned = true;

            Spawn();
        }
        else
        {
            BornTimeFloat -= Time.smoothDeltaTime;
        }
    }

    void Spawn()
    {
        Instantiate(NewBornLarvae.gameObject, transform.position, Quaternion.identity);

        Destroy(gameObject, 5f);
    }

}
