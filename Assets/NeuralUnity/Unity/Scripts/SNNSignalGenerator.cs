using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SNN))]
public class SNNInputSignal : SignalGenerator
{

    // This enforces a neuron to fire at a particular rate. May not work as intended with other SNN inputs.
    [Header("Read Only Attributes")]
    public float frequencyEstimate = 0;
    public float clock;

    public SNN target = null;

    // Start is called before the first frame update
    void Start()
    {
        clock = 0f;
        if (target == null)
        {
            target = GetComponent<SNN>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        target.timeSinceLastSpike += Time.deltaTime;
        clock += ((phase + Time.deltaTime) / (signalRate == 0 ? 1 : signalRate)); // Prevent zero-division
        frequencyEstimate = signal.Evaluate(clock) * amplifier; // Get the frequency according to the graph
        if (target.timeSinceLastSpike > frequencyEstimate)
        {
            target.timeSinceLastSpike = 0;
            target.currentPotential = target.baseActivationVoltage;
        }
    }
}
