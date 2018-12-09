using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistressSignalManager : MonoBehaviour
{
    public Transform TargetedFood;
    public Transform DistressSender;

    private void Start()
    {
        //InvokeRepeating("Signals", 5f, 5f);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Signals();
        }
    }

    void Signals()
    {
        Debug.LogWarning("Processing Signals System..." );

        List<AntsIntermediate> _allAnts = Nest.singleton.Anntts;

        AntsIntermediate _ant = _allAnts[Random.Range(0, _allAnts.Count)];

        if(_ant.transform != DistressSender)
        {
            if (_ant.isIdling)
            {
                Debug.LogWarning(_ant.transform.name + " received a distress Signals");

                _ant.OnReceivedSignals(TargetedFood);

                DistressSender = null;
            }
            else
            {
                // Do nothing
            }
        }
        else
        {
            // Do nothing
        }
    }
}
