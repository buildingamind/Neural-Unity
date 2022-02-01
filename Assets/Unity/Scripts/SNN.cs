using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class SNN : NeuronClass
{

    [Header("____[Neuron Attributes]________________________________________")]
    public bool ForceSpike; // Force a spike;
    public float currentPotential = 0f;
    public float restingPotential = 0f;
    public float baseActivationVoltage = -55f;
    public float currentActivationVoltage;
    public float refractionPotential = -2f;

    [Header("____[Rate Attributes]________________________________________")]
    public float potentialLeak = 0.001f;
    public float recoveryRate = 0.5f;
    public float activationLeak = 0.1f;
    
    public bool stochasticFiring = false; // Can Neurons Spontaneously Fire?
    public int stochasticityOdds = 10000;  // This is an idea to do with preventing too strong of weight decay (other ideas welcome)
    public float weightDecay = 1f;
    public float competitionRadius = 1f;

    public float[] outputWeights;

    [Header("____[Spike Timing Dependent Plasticity]________________________")]

    public bool EnableSpikeTimingPlasticity = false;
    public float learningRate = 1f;
    // vvvvvvvvvvvvvvvvvvvvvvvvvv
    // Possibly make leakRate non-linear (interpolation or other)
    // a leakRate of 0 would be an Integrate and Fire Model
    // any leakRate > 0 is a Leaky Integrate and Fire


    [Header("____[Other Information]________________________________________")]
    public int numSpikes = 0;
    public float timeSinceLastSpike = 0f;
    AudioSource audioSource;
    public int network_layer;
    public int layer_position;
    public bool isAnOutput;
    // Setting this to <= 1/60f ensures that the signal generator doesn't run faster than the checks
    // You would sample in-between signals and likely overshoot your activation threshold on every step

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        // SetMessage("SNN Script Loaded");  // This executes in Editor
        timeSinceLastSpike = 0f;
        audioSource = GetComponent<AudioSource>();
        outputWeights = new float[outputs.Count];

        // Forced Parameter Setting
        for (int i = 0; i < outputWeights.Length; i++)
        {
            outputWeights[i] = RandomGaussian(40, 50) / (float)(outputs.Count);
        }
        recoveryRate = RandomGaussian(100, 200)/1000f;
        refractionPotential = RandomGaussian(-8, -12);
        baseActivationVoltage = RandomGaussian(30, 40);
        restingPotential = RandomGaussian(0, 1);
        currentPotential = restingPotential;
        currentActivationVoltage = baseActivationVoltage;
    }

    public override void Update()
    {
        if (Application.isPlaying)
        {
            Activate();
        }
    }

    public void Activate()
    {
        // HANDLE TIMESTEPS
        timeSteps++;
        //timeSinceLastSpike += Time.deltaTime;
        timeSinceLastSpike += 1/60f;
        rend.material.SetColor("_Color", Color.Lerp(rend.material.color, Color.black, 0.4f));
        rend.material.SetColor("_BaseColor", Color.Lerp(rend.material.color, Color.black, 0.4f));

        if (ForceSpike || (stochasticFiring && Random.Range(0, stochasticityOdds) == 0))
        {
            currentPotential = currentActivationVoltage;
        }

        // STDP Spike Timing Dependent Plasticity
        if (EnableSpikeTimingPlasticity)
        {
            for (int p = 0; p < inputs.Count; p++)
            {
                SNN priorNeuron = inputs[p] as SNN;
                if (priorNeuron.inputs.Count > 0)
                {
                    for (int j = 0; j < priorNeuron.outputWeights.Length; j++)
                    {
                        if (priorNeuron.outputs[j] == this)
                        {
                            priorNeuron.outputWeights[j] += Unimode(priorNeuron.timeSinceLastSpike) * learningRate;
                        }
                        else
                        {
                            priorNeuron.outputWeights[j] -= Unimode(priorNeuron.timeSinceLastSpike) * learningRate;
                        }
                    }
                }
            }
        }

        if (currentPotential <= restingPotential)
        {
            //currentPotential += (recoveryRate * Time.deltaTime);
            currentPotential += recoveryRate;
        }
        else
        {
            if (currentActivationVoltage > baseActivationVoltage)
            {
                currentActivationVoltage *= (1 - activationLeak);
            }

            // ACTIVATION / FIRE / SPIKE
            if (currentPotential >= currentActivationVoltage)
            {
                currentPotential = refractionPotential;

                // Competition Rule
                foreach (Collider neighbor in Physics.OverlapSphere(this.transform.position, competitionRadius))
                {
                    SNN neighborNeuron = neighbor.GetComponent<SNN>();
                    if (neighborNeuron == null || (neighborNeuron.network_layer != network_layer) || neighborNeuron.Equals(this))
                    {
                        continue;
                    }
                    neighborNeuron.currentPotential = neighborNeuron.refractionPotential;
                }
                
                Color inverse = new Color(1 - rend.material.color.r, 1 - rend.material.color.g, 1 - rend.material.color.b);
                rend.material.SetColor("_BaseColor", inverse); rend.material.SetColor("_Color", inverse);
                ForceSpike = false;
                audioSource.Play();
                timeSinceLastSpike = 0;
                numSpikes++;

                // Propagate Capacitance to Forward Neurons
                foreach (SNN neuron in outputs)
                {
                    if (neuron.currentPotential >= neuron.restingPotential)
                    {
                        foreach (float capacitance in outputWeights)
                        {
                            neuron.currentPotential += capacitance;
                        }
                    }
                }
                currentActivationVoltage *= 2f;
            }
            else
            {
                currentPotential *= (1 - potentialLeak);
                if (EnableSpikeTimingPlasticity)
                {
                    for (int i = 0; i < outputWeights.Length; i++)
                    {
                        outputWeights[i] *= weightDecay;
                    }
                }
            }
            //ActivationHistory.Add(activationVoltage);
            //DivisionCheck();
        }
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    public static float Unimode(double value)
    {
        return (1 / (Mathf.Sqrt(2 * Mathf.PI))) * Mathf.Exp(-(Mathf.Pow((float)value, 2f) / 2));
    }
}
