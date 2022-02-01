using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class NeuronClass : MonoBehaviour
{
    public string Message; // We can use this to display public messages to a user.
    public bool ShowInputs = true;
    public bool ShowOutputs = true;
    [Header("___[ ADD CONNECTIONS]______________________________________________________________________")]
    [Tooltip("Drag one or more neurons here to add them to the network.")]
    public List<NeuronClass> AddInput = new List<NeuronClass>();
    public List<NeuronClass> AddOutput = new List<NeuronClass>();
    [Tooltip("Erase all inputs.")]
    public bool ClearInputs;
    [Tooltip("Erase all outputs.")]
    public bool ClearOutputs;
    [Header("___[ INPUT NEURONS ]_______________________________________________________________________")]
    [Tooltip("Readable list of inputs to the neuron.")]
    public List<NeuronClass> inputs = new List<NeuronClass>();
    [Header("___[ OUTPUT NEURONS ]______________________________________________________________________")]
    [Tooltip("Readable list of outputs from the neuron.")]
    public List<NeuronClass> outputs = new List<NeuronClass>();
    [Header("___[ DIVISIBILITY PARAMETERS ]______________________________________________________________")]
    [Tooltip("Should the neuron divide?")]
    public bool divisible = false;
    public int minimumActivations = 600;  // How many activations before we consider to split the neuron
    public enum DivisionMode { random, variation, split, identical };
    [Tooltip("If the neuron should divide, how should prodgeny weights and biases be assigned?")]
    public DivisionMode divisionMode = DivisionMode.random;
    [Tooltip("How many lines of neurons should a neuron maximally produce? Heads up! This is mainly for performance reasons")]
    public int maxDivisions = 0;
    public int numDivisions = 0;  // Keep Track of Divisions
    public int timeSteps = 0;  // Keep Track of Activations (This is slightly different for SNN and CTRNNs
    public float divisionProximity;  // This will publically show how close a neuron's average activation is getting to splitting
    public float[] memoryBuffer;

    // Criterion to split the neruon
    public enum DivisionObjective { MaximumActivation, MinimumActivation };
    public DivisionObjective divisionObjective;
    public int divisbilityOffset;

    public List<float> ActivationHistory = new List<float>();
    public LineRenderer line;
    public List<Vector3> divideDirection;

    public Renderer rend;

    // Start is called before the first frame update
    public virtual void Start()
    {
        line = GetComponent<LineRenderer>();
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        ManageConnections();
    }

    public void DivisionCheck()
    {
        // Calculate information for possibile divisions (Experimental)
        

        if (this.GetType() == typeof(CTRNN))
        {
            float mean = 0;
            foreach (float value in memoryBuffer)
            {
                mean += value;
            }
            mean /= memoryBuffer.Length;

            divisionProximity = Mathf.Abs((float)System.Math.Tanh(mean));
            if (divisible && (numDivisions < maxDivisions) && (timeSteps >= (minimumActivations + divisbilityOffset)))
            {
                if (divisionObjective.Equals(DivisionObjective.MaximumActivation))
                {
                    // Feature Propagation
                    // Low Activation Leads to Split
                    if (divisionProximity <= 0.25f)
                    {
                        CTRNNDivide();
                    }
                }

                if (divisionObjective.Equals(DivisionObjective.MinimumActivation))
                {
                    // Error Propagation
                    // High Activation Leads to Split
                    if (Mathf.Abs(divisionProximity) >= 0.75f)
                    {
                        CTRNNDivide();
                    }
                }
            }
        }
        else if (this.GetType() == typeof(SNN))
        {
            if (divisible && (numDivisions < maxDivisions) && (timeSteps >= (minimumActivations + divisbilityOffset)))
            {
                SNN thisNeuron = this as SNN;
                divisionProximity = thisNeuron.timeSinceLastSpike;
                if (divisionProximity > 5f)
                {
                    SNNDivide();
                }
            }
        }

    }

    void ManageConnections()
    {
        if (line != null)
        {
            line.positionCount = inputs.Count * 2;
            for (int i = 0; i < inputs.Count * 2; i++)
            {
                if (i % 2 == 0)
                {
                    line.SetPosition(i, this.transform.position);
                }
                else
                {
                    if (inputs[i / 2] != null && inputs[i / 2].gameObject.activeSelf)
                    {
                        line.SetPosition(i, inputs[i / 2].transform.position);
                    }
                    else
                    {
                        line.SetPosition(i, this.transform.position);
                    }
                }

            }
        }
        // Editor Clear Buttons ////////////////////
        if (ClearInputs)
        {
            for (int i = inputs.Count - 1; i > -1; i--)
            {
                RemoveInputNeuron(inputs[i]);
            }
            inputs = new List<NeuronClass>();
            if (this.GetType() == typeof(CTRNN))
            {
                CTRNN CTRNNSelf = this as CTRNN;
                CTRNNSelf.inputWeights = new List<float>();
            }
            
            ClearInputs = false;
        }
        if (ClearOutputs)
        {
            for (int i = outputs.Count - 1; i > -1; i--)
            {
                RemoveOutputNeuron(outputs[i]);
            }
            outputs = new List<NeuronClass>();
            ClearOutputs = false;
        }
        // Editor Add Connections ///////////////////
        if (AddInput.Count > 0)
        {
            if (AddInput[0] != null)
            {
                foreach (NeuronClass neuron in AddInput)
                {
                    AddInputNeuron(neuron);
                    neuron.AddOutputNeuron(this);
                }
            }
            AddInput.Clear();
        }

        for (int i = inputs.Count - 1; i > -1; i--)
        {
            if (inputs[i] == null)
            {
                inputs.RemoveAt(i);
                if (this.GetType() == typeof(CTRNN))
                {
                    CTRNN CTRNNSelf = this as CTRNN;
                    CTRNNSelf.inputWeights.RemoveAt(i);;
                }
            }
        }

        if (AddOutput.Count > 0)
        {
            if (AddOutput[0] != null)
            {
                foreach (NeuronClass neuron in AddOutput)
                {
                    AddOutputNeuron(neuron);
                    neuron.AddInputNeuron(this);
                }
            }
            AddOutput.Clear();
        }
        foreach (NeuronClass neuron in outputs)
        {
            if (neuron && neuron.isActiveAndEnabled && neuron.gameObject.activeSelf)
            {
                Debug.DrawLine(transform.position + new Vector3(0, 0.05f, 0), neuron.transform.position + new Vector3(0, 0.05f, 0));
            }
        }
        foreach (NeuronClass neuron in inputs)
        {
            if (neuron && neuron.isActiveAndEnabled && neuron.gameObject.activeSelf)
            {
                Debug.DrawLine(transform.position + new Vector3(0, 0.05f, 0), neuron.transform.position + new Vector3(0, 0.05f, 0));
            }
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
                if (this.GetType() == typeof(CTRNN))
                {
                    CTRNN CTRNNSelf = this as CTRNN;
                    CTRNNSelf.inputWeights.RemoveAt(i); ;
                }
            }
        }
    }

    public void SetMessage(string message)
    {
        Message = message;
        Debug.Log(message);
    }

    public bool RemoveOutputNeuron(NeuronClass otherNeuron)
    {
        outputs.Remove(otherNeuron);
        // Both sides need a transaction
        for (int i = otherNeuron.inputs.Count - 1; i > -1; i--)
        {
            if (otherNeuron.inputs[i].Equals(this))
            {
                // We ask the neuron to remove us from their inputs
                otherNeuron.inputs.RemoveAt(i);
                // And then ask the neuron to remove its weight entry
                if (otherNeuron.GetType() == typeof(CTRNN))
                {
                    CTRNN otherCTRNN = otherNeuron as CTRNN;
                    otherCTRNN.inputWeights.RemoveAt(i);
                }
                
                return true;
            }
        }
        return false;
    }

    public void CheckConnections()
    {
        
    }

    public void AddInputNeuron(NeuronClass neuron)
    {
        if (!inputs.Contains(neuron))
        {
            this.inputs.Add(neuron);
            if (this.GetType() == typeof(CTRNN))
            {
                CTRNN CTRNNSelf = this as CTRNN;
                CTRNNSelf.inputWeights.Add(Random.Range(-50000, 50000) / 10000f);
            }
        }
        else
        {
            SetMessage(neuron.name + " is already an input of " + name + "! Skipping...");
        }
        if (!neuron.outputs.Contains(this))
        {
            neuron.outputs.Add(this);
        }
        else
        {
            SetMessage(neuron.name + " is already an output of " + name + "! Skipping...");
        }
    }

    public void AddOutputNeuron(NeuronClass neuron)
    {
        if (!outputs.Contains(neuron))
        {
            this.outputs.Add(neuron);
            if (neuron.GetType() == typeof(CTRNN))
            {
                CTRNN OtherCTRNN = neuron as CTRNN;
                OtherCTRNN.inputWeights.Add(Random.Range(-50000, 50000) / 10000f);
            }
            
        }
        else
        {
            SetMessage(neuron.name + " is already an output of " + name + "! Skipping...");
        }
        if (!neuron.inputs.Contains(this))
        {
            neuron.inputs.Add(this);
        }
        else
        {
            SetMessage(neuron.name + " is already an input of " + name + "! Skipping...");
        }
    }

    public bool RemoveInputNeuron(NeuronClass otherNeuron)
    {
        // Both sides need a transaction
        otherNeuron.outputs.Remove(this);
        // Outputs can be removed very easily

        for (int i = inputs.Count - 1; i > -1; i--)
        {
            // If we find a trace of the other neuron in our inputs
            if (inputs[i].Equals(otherNeuron))
            {
                // Remove it
                inputs.RemoveAt(i);
                // Along with its weight value
                if (this.GetType() == typeof(CTRNN))
                {
                    CTRNN CTRNNSelf = this as CTRNN;
                    CTRNNSelf.inputWeights.RemoveAt(i);
                }
                
                return true;
            }
        }
        return false;
    }

    // CAUTION: This is a very experimental feature. Too many unbounded divisions will likely crash!
    public void CTRNNDivide()
    {
        // We set some of these so that they inherit, rather than setting them on both later on.
        numDivisions++;
        divideDirection = new List<Vector3>() { transform.right, transform.up, transform.forward };
        CTRNN newNeuron1 = Instantiate(this.gameObject, this.transform.position + (divideDirection[numDivisions % divideDirection.Count] / (numDivisions + 1)), transform.rotation, transform.parent).GetComponent<CTRNN>();
        CTRNN newNeuron2 = Instantiate(this.gameObject, this.transform.position - (divideDirection[numDivisions % divideDirection.Count] / (numDivisions + 1)), transform.rotation, transform.parent).GetComponent<CTRNN>();
        newNeuron1.gameObject.transform.localScale *= (2 / 3f);
        newNeuron2.gameObject.transform.localScale *= (2 / 3f);

        // Determine how the old weights are redistributed
        if (divisionMode.Equals(DivisionMode.split))
        {
            newNeuron1.GenerateSplitWeights(this as CTRNN);
            newNeuron1.GenerateSplitBias(this as CTRNN);
            newNeuron2.GenerateSplitWeights(this as CTRNN);
            newNeuron2.GenerateSplitBias(this as CTRNN);
        }
        if (divisionMode.Equals(DivisionMode.variation))
        {
            newNeuron1.AddWeightVariance();
            newNeuron1.AddBiasVariance();
            newNeuron2.AddWeightVariance();
            newNeuron2.AddBiasVariance();
        }
        if (divisionMode.Equals(DivisionMode.random))
        {
            newNeuron1.RandomizeWeights();
            newNeuron1.RandomizeBias();
            newNeuron2.RandomizeWeights();
            newNeuron2.RandomizeBias();
        }
        if (divisionMode.Equals(DivisionMode.identical))
        { }  // Nothing needs to happen, but let's make it explicit

        // Then, add the New Connections to themselves and each other
        // Self Connection
        newNeuron1.AddInputNeuron(newNeuron1);
        newNeuron2.AddInputNeuron(newNeuron2);
        // Daughter Connection
        newNeuron1.AddInputNeuron(newNeuron2);
        newNeuron2.AddInputNeuron(newNeuron1);
        // Make the next division even further so that divisions are not rapid
        newNeuron1.minimumActivations *= 2;
        newNeuron1.memoryBuffer = new float[newNeuron1.minimumActivations];
        newNeuron2.minimumActivations *= 2;
        newNeuron2.memoryBuffer = new float[newNeuron2.minimumActivations];

        foreach (CTRNN neuron in inputs)
        {
            neuron.AddOutputNeuron(newNeuron1);
            neuron.AddOutputNeuron(newNeuron2);
        }

        foreach (CTRNN neuron in newNeuron1.outputs)
        {
            if (neuron != null)
            {
                neuron.AddInputNeuron(newNeuron1);
            }
        }
        foreach (CTRNN neuron in newNeuron2.outputs)
        {
            if (neuron != null)
            {
                neuron.AddInputNeuron(newNeuron2);
            }
        }
        Destroy(this.gameObject);
    }

    public void SNNDivide()
    {
        // We set some of these so that they inherit, rather than setting them on both later on.
        numDivisions++;
        divideDirection = new List<Vector3>() { transform.right, transform.up, transform.forward };
        SNN newNeuron1 = Instantiate(this.gameObject, this.transform.position + (divideDirection[numDivisions % divideDirection.Count] / (numDivisions + 1)), transform.rotation, transform.parent).GetComponent<SNN>();
        SNN newNeuron2 = Instantiate(this.gameObject, this.transform.position - (divideDirection[numDivisions % divideDirection.Count] / (numDivisions + 1)), transform.rotation, transform.parent).GetComponent<SNN>();
        newNeuron1.gameObject.transform.localScale *= (2 / 3f);
        newNeuron2.gameObject.transform.localScale *= (2 / 3f);

        // Then, add the New Connections to themselves and each other
        // Self Connection
        newNeuron1.AddInputNeuron(newNeuron1);
        newNeuron2.AddInputNeuron(newNeuron2);
        // Daughter Connection
        newNeuron1.AddInputNeuron(newNeuron2);
        newNeuron2.AddInputNeuron(newNeuron1);
        // Make the next division even further so that divisions are not rapid
        newNeuron1.minimumActivations *= 2;
        newNeuron1.memoryBuffer = new float[newNeuron1.minimumActivations];
        newNeuron2.minimumActivations *= 2;
        newNeuron2.memoryBuffer = new float[newNeuron2.minimumActivations];

        newNeuron1.Start();
        newNeuron2.Start();

        foreach (SNN neuron in inputs)
        {
            neuron.AddOutputNeuron(newNeuron1);
            neuron.AddOutputNeuron(newNeuron2);
        }

        foreach (SNN neuron in newNeuron1.outputs)
        {
            if (neuron != null)
            {
                neuron.AddInputNeuron(newNeuron1);
            }
        }
        foreach (SNN neuron in newNeuron2.outputs)
        {
            if (neuron != null)
            {
                neuron.AddInputNeuron(newNeuron2);
            }
        }
        Destroy(this.gameObject);
    }

    // This is called on every frame inside the editor to perform some magic
    private void OnDrawGizmosSelected()
    {
        if (ShowOutputs)
        {
            foreach (NeuronClass neuron in outputs)
            {
                if (neuron)
                {
                    Gizmos.color = Color.red;
                    for (int i = 0; i < 10; i++)
                    {
                        Gizmos.DrawLine(transform.position + new Vector3(i / 1000f, i / 1000f, i / 1000f), neuron.transform.position + new Vector3(i / 1000f, i / 1000f, i / 1000f));
                    }
                }
            }
        }

        if (ShowInputs)
        {
            foreach (NeuronClass neuron in inputs)
            {
                if (neuron)
                {
                    Gizmos.color = Color.blue;
                    for (int i = 0; i < 10; i++)
                    {
                        Gizmos.DrawLine(transform.position + new Vector3(i / 1000f, i / 1000f, i / 1000f), neuron.transform.position + new Vector3(i / 1000f, i / 1000f, i / 1000f));
                    }
                }
            }
        }
    }


}
