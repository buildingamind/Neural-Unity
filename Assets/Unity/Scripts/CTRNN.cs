using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CTRNN : NeuronClass
{
    // I'm not sure if a neuron could compute log10 as an activation function, but maybe another network could.
    public enum Activation { none, sigmoid, tanh, abs, relu, linearSaturated, log10, log2, unitStep, unimode, custom };
    [Header("___[ THIS NEURON ]_________________________________________________________________________")]
    [Tooltip("Readable activated value of the neuron.")]
    public float value;
    [Tooltip("Read and writable bias of the neuron.")]
    public float bias;
    [Tooltip("Read and writable list of weights per input neuron.")]
    public List<float> inputWeights = new List<float>();
    [Header("___[ NEURON PARAMETERS ]___________________________________________________________________")]
    [Tooltip("The function used to activate this neuron.")]
    public Activation activation;
    [Tooltip("An optional customizable activation function for use on Activation.custom")]
    public AnimationCurve customActivation;
    [Tooltip("Should the neuron's weights be randomly initialized or set by hand?")]
    public bool randomWeights = true;
    [Tooltip("Should the neuron's bias be randomly initialized or set by hand?")]
    public bool randomBias = true;
    [Tooltip("How often should the neuron perform Euler Integration on its ODE?")]
    public float timeScale = 1 / 60f;  // Unity Default timescale (Once per frame)
    [Tooltip("Whether to store the history of the neuron's activation")]
    public bool enableHistory = false;
    [Tooltip("This is how often the history is captured - independent from how often the neuron is activated.")]
    public float historyFrequency = 1/ 60f;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        SetMessage("CTRNN Script Loaded");  // This executes in Editor

        memoryBuffer = new float[minimumActivations];
        if (divisible)
        {
            divisbilityOffset = Random.Range(0, 200); // This is so neurons don't all split at once, but somewhat stochastically
        }

        if (randomWeights)
        {
            RandomizeWeights();
        }

        if (randomBias)
        {
            RandomizeBias();
        }

        for (var i = outputs.Count - 1; i > -1; i--)
        {
            if (outputs[i] == null)
                outputs.RemoveAt(i);
        }
        for (var i = inputs.Count - 1; i > -1; i--)
        {
            if (inputs[i] == null)
            {
                inputs.RemoveAt(i);
                inputWeights.RemoveAt(i);
            }
        }

        if (Application.isPlaying)
        {
            InvokeRepeating(nameof(Activate), 0, timeScale);
        }
    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < inputWeights.Count; i++)
        {
            inputWeights[i] = Random.Range(-50000, 50000) / 10000f;
        }
    }

    public void GenerateSplitWeights(CTRNN parent)
    {
        /// When a Neuron Splits, this is the case where the weights and Biases are split in half
        /// i.e: If a neuron had a weight of 0.8 and bias of -0.4, then the new values are w=0.4 and b=-0.2 each.
        for (int i = 0; i < inputWeights.Count; i++)
        {
            inputWeights[i] = parent.inputWeights[i] / 2f;
        }
    }

    public void GenerateSplitBias(CTRNN parent)
    {
        bias = parent.bias / 2f;
    }

    public void AddWeightVariance()
    {
        for (int i = 0; i < inputWeights.Count; i++)
        {
            inputWeights[i] += Random.Range(-50000, 50000) / 100000f;
        }
    }

    public void RandomizeBias()
    {
        bias = Random.Range(-10000, 10000) / 10000f;
    }

    public void AddBiasVariance()
    {
        bias += Random.Range(-50000, 50000) / 100000f;
    }

    // Perform non-linear activation on the units
    void Activate()
    {   
        if (maxDivisions > 3)
        {
            // Warn the user if maximum divisions may cause performance issues. Currently 3 is a considerable amount, especially if the network is large.
            Debug.LogWarning("[WARN] Maximum Divisions is large ("+ maxDivisions +") for " + this.gameObject.name + ". Performance may slow down if too many neurons divide!");
        }
        // Initialize the sum as the bias
        float sum = bias;
        try
        {
            // Perform weight multiplication
            for (int i = 0; i < inputs.Count; i++)
            {
                CTRNN input_ctrnn = inputs[i] as CTRNN;
                sum += (input_ctrnn.value * inputWeights[i]);
            }
        }
        catch (System.ArgumentOutOfRangeException)
        {
            Debug.LogError(this.name + ": Inputs Out of Range. Check Connections.");
            Debug.LogError(this.name + " has " + inputs.Count + " inputs and " + inputWeights.Count + " weights.");
        }

        // Perform a non-linear activation
        if (activation == Activation.sigmoid)
        {
            value += (Sigmoid(sum) - value) * timeScale;
        }
        else if (activation == Activation.tanh)
        {
            value += ((float)System.Math.Tanh((double)sum) - value) * timeScale;
        }
        else if (activation == Activation.relu)
        {
            value += (ReLU(sum) - value) * timeScale;
        }
        else if (activation == Activation.abs)
        {
            value += (Mathf.Abs(sum) - value) * timeScale;
        }
        else if (activation == Activation.log10)
        {
            value += (Mathf.Log10(sum + 0.001f) - value) * timeScale;
        }
        else if (activation == Activation.log2)
        {
            value += (Mathf.Log(sum + 0.001f, 2) - value) * timeScale;
        }
        else if (activation == Activation.unimode)
        {   
            value += (Unimode(sum) - value) * timeScale;
        }
        else if (activation == Activation.unitStep)
        {
            value += (UnitStep(sum) - value) * timeScale;
        }
        else if (activation == Activation.custom)
        {
            value += (customActivation.Evaluate(sum) - value) * timeScale;
        }
        else if (activation == Activation.linearSaturated)
        {
            value += (LinearSaturation(sum) - value) * timeScale;
        }
        else
        {
            // Perform no activation function
            value += (sum - value) * timeScale;
        }
        if (enableHistory)
        {
            ActivationHistory.Add(value);
        }
        memoryBuffer[timeSteps % memoryBuffer.Length] = value;
        timeSteps++;

        if (value >= 0)
        {
            rend.material.SetColor("_BaseColor", Color.Lerp(Color.black, Color.white, value));  // For Universal Rendering
            rend.material.SetColor("_Color", Color.Lerp(Color.black, Color.white, value));  // For Standard Shading
        }
        else
        {
            rend.material.SetColor("_BaseColor", Color.Lerp(Color.black, Color.red, Mathf.Abs(value)));  // For Universal Rendering
            rend.material.SetColor("_Color", Color.Lerp(Color.black, Color.red, Mathf.Abs(value)));  // For Standard Shading
        
        }
        DivisionCheck();
    }

    public static float Sigmoid(double value)
    {
        return 1 / (1.0f + Mathf.Exp(-(float)value));
    }

    public static float ReLU(double value)
    {
        if (value <= 0)
            return 0;
        return (float)value;
    }

    public static float UnitStep(double value)
    {
        if (value >= 0)
            return 1;
        return 0;
    }

    public static float LinearSaturation(double value)
    {
        if (value < 0)
            return 0;
        if (value > 1)
            return 1;
        return (float)value;
    }

    public static float Unimode(double value)
    {
        return (1 / (Mathf.Sqrt(2 * Mathf.PI))) * Mathf.Exp(-(Mathf.Pow((float) value, 2f) / 2));
    }
}
