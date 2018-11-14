using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class korbryan : MonoBehaviour
{
    
    private void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Point();
        }
    }

    void Point()
    {
        float X, Z, RandomX, RandomZ;

        X = Nest.singleton.NestSize/2;
        Z = Nest.singleton.NestSize/2;

        RandomX = Random.Range(-X, X);
        RandomZ = Random.Range(-Z, Z);

        Vector3 TargetPoint = new Vector3(RandomX, 0.1f, RandomZ);

        TargetPoint = Nest.singleton.transform.TransformPoint(TargetPoint/2f);

        Debug.DrawLine(transform.position, TargetPoint, Color.green, 5f);
    }
}
