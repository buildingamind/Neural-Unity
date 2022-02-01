using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCircularNetwork : MonoBehaviour
{
    public GameObject NeuronObject;
    
    NeuronClass CenterNeuronScript;
    public float radius = 5;
    public int density = 5;
    public int layers = 3;

    public bool NeighborConnections;
    public bool RandomConnections;
    public bool SelfConnections;
    public bool DivisibleNeurons;
    public float MaxDistance;

    public int NeuronCount;

    public int NumRandomConnections;

    List<NeuronClass> Network;
    public List<List<NeuronClass>> NetworkLayers = new List<List<NeuronClass>>();

    // Start is called before the first frame update
    void Start()
    {
        CenterNeuronScript = NeuronObject.GetComponent<NeuronClass>();

        for (int i=0; i < layers; i++)
        {
            BuildRing(radius * (i+1), i/6f);
        }

        // Random Connectivity
        for (int i = 0; i < NumRandomConnections; i++)
        {
            int layer1 = Random.Range(0, NetworkLayers.Count);
            int layer2 = Random.Range(0, NetworkLayers.Count);

            int network1 = Random.Range(0, Network.Count);
            int network2 = Random.Range(0, Network.Count);

            // If the two random are the same, we will treat this as a center connection
            if (layer1 == layer2 && network1 == network2)
            {
                try
                {
                    CenterNeuronScript.AddOutputNeuron(NetworkLayers[layer1][network1]);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    i--;
                    continue;
                }
            }
            else
            {
                if (RandomConnections)
                {
                    try
                    {
                        NeuronClass neuron1 = NetworkLayers[layer1][network1];
                        NeuronClass neuron2 = NetworkLayers[layer2][network2];
                        if (MaxDistance <= 0 || Vector3.Distance(neuron1.transform.position, neuron2.transform.position) < MaxDistance)
                        {
                            neuron1.AddInputNeuron(neuron2);
                            neuron2.AddInputNeuron(neuron1);
                        }
                        else {
                            i--;
                            continue;
                        }
                    }
                    catch (System.ArgumentOutOfRangeException){
                        Debug.LogWarning("Layer1: " + layer1.ToString() + ", Network1: " + network1.ToString());
                        Debug.LogWarning("Layer2: " + layer2.ToString() + ", Network2: " + network2.ToString());
                        Debug.LogWarning(NetworkLayers.Count + ", " + Network.Count);
                    }
                }
            }
        }
        Camera.main.orthographicSize = Mathf.Sqrt(NeuronCount) + 5f;
    }

    void BuildRing(float radius=5f, float phase=0f)
    {
        int neuronID = 0;
        Network = new List<NeuronClass>();
        for (float i = 0; i < (Mathf.PI * 2 - 0.01f); i += (Mathf.PI / (density * (1 + NetworkLayers.Count))))
        {
            float a = transform.position.x + radius * Mathf.Cos(i + phase);
            float b = transform.position.y + radius * Mathf.Sin(i + phase);
            GameObject newNeuron = Instantiate(NeuronObject, new Vector3(a, b, 0), Quaternion.identity, transform.parent);
            newNeuron.gameObject.transform.Rotate(new Vector3(0, 0, Mathf.Rad2Deg * i));
            newNeuron.name += neuronID.ToString();

            newNeuron.GetComponent<SignalGenerator>().enabled = false;
            NeuronClass newNeuronScript = newNeuron.GetComponent<NeuronClass>();

            if (newNeuronScript.GetType() == typeof(CTRNN))
            {
                CTRNN CTRNNScript = newNeuronScript as CTRNN;
                CTRNNScript.divisible = DivisibleNeurons;
                CTRNNScript.divisionObjective = NeuronClass.DivisionObjective.MinimumActivation;
                CTRNNScript.activation = CTRNN.Activation.tanh;
                CTRNNScript.RandomizeWeights();
                CTRNNScript.RandomizeBias();
            }
            Network.Add(newNeuronScript);
            neuronID++;
            NeuronCount++;
        }
        
        for (int i = 0; i < Network.Count; i++)
        {
            if (SelfConnections)
            {
                Network[i].AddInputNeuron(Network[i]);
            }
        }

        if (NeighborConnections)
        {
            for (int i = 0; i < Network.Count - 1; i++)
            {
                    Network[i].AddInputNeuron(Network[i + 1]);
                    Network[i + 1].AddInputNeuron(Network[i]);
            }
            // Neuron at Last Position and First Position
            Network[Network.Count - 1].AddInputNeuron(Network[0]);
        }
        NetworkLayers.Add(Network);
    }
}
