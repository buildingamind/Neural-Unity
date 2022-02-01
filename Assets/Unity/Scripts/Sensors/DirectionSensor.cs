using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionSensor : BaseSensor
{

    [Header("[?] How often should the sensor apply its value to the target neuron?")]
    public float sampleFrequency = 0.1f;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if (sampleFrequency >= 0)
        {
            InvokeRepeating("SampleDirection", 0f, sampleFrequency);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        
    }

    public void SampleDirection()
    {
        sensorValue = Vector3.Dot(transform.forward, Vector3.forward);
    }
}
