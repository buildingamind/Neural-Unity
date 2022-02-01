using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSensor : BaseSensor
{

    // Keep in mind: This script is giving access to a very non-biologically plausible metric (especially if it were vision).
    // However, for something like sound or temperature, this might not be unrealistic.
    [Header("[?] How often should the sensor apply its value to the target neuron?")]
    public float sampleFrequency = 0.1f;

    

    public GameObject otherObject;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if (sampleFrequency >= 0)
        {
            InvokeRepeating("SampleDistance", 0f, sampleFrequency);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        
    }

    public void SampleDistance()
    {
        sensorValue = Vector3.Distance(transform.position, otherObject.transform.position);
    }
}
