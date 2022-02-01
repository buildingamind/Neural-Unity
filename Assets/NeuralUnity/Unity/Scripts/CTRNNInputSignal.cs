using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CTRNN))]
public class CTRNNInputSignal : SignalGenerator
{
    public CTRNN target;
    public float outputValue;

    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
        {
            target = GetComponent<CTRNN>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float time = ((Time.time + phase) * signalRate) % 1;
        outputValue = signal.Evaluate(time) * amplifier;
        target.value = outputValue;
    }
}
