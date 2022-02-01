using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSensor : MonoBehaviour
{
    [Header("___[ SENSOR VALUES ] ____________________________________________________________________")]
    public CTRNN targetNeuron;  // This is the CTRNN whose value we will be controlling
    public float sensorValue = 0f;
    public float sustain = 0.5f;

    // Start is called before the first frame update
    public virtual void Start()
    {
    }

    // Update is called once per frame
    public virtual void Update()
    {
    }
}
