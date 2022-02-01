using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSystem : MonoBehaviour
{
    public enum ColorSensitivity { red, green, blue , grayscale};
    public ColorSensitivity colorSensitivity = ColorSensitivity.grayscale;

    public bool useStochasticHiddenLayers = false;
    public int stochasticityOdds = 10000;  // As in: 1/odds chance

    // Start is called before the first frame update
    public int width = 16;
    public int height = 16;
    public bool visualize;
    public bool showColor = true;

    public GameObject NeuronPrefab;
    public RenderTexture image;
    
    SNN[] VisualLayer;
    public float FrequencyScalar = 1f;

    public bool lateralConnections;
    public bool recurrentEdgeConnections;
    public bool layerRecurrency;

    public List<int> layerSizes = new List<int> { 10, 10, 10 };

    public float neuronSpacing = 1f;

    public bool TrainOnMNIST = false;
    public MNIST mnistScript;

    public int trainingSteps = 10000;
    public int currentStep;
    public List<SNN[]> layers;
    

    void Start()
    {
        layers = new List<SNN[]>();
        image.width = width;
        image.height = height;
        VisualLayer = new SNN[width * height];
        BuildNetwork(layerSizes.Count);
    }

    void FixedUpdate()
    {
        SendValues();
        if (trainingSteps > 0 && currentStep >= trainingSteps)
        {
            Debug.Log("Training Finished");
            foreach(SNN[] layer in layers)
            {
                foreach(SNN neuron in layer)
                {
                    neuron.EnableSpikeTimingPlasticity = false;  // Freeze Learning Dynamics
                }
            }
        }
       
    }

    public void BuildNetwork(int numLayers)
    {
        // Image Layer
        GameObject imageLayerObject = new GameObject("Retina");
        imageLayerObject.transform.parent = this.gameObject.transform;
        int neuronIndex = 0;
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                GameObject neuron = Instantiate(NeuronPrefab, transform.position + new Vector3(w, h, 0), Quaternion.identity, imageLayerObject.transform);
                neuron.name = "Video (" + w.ToString() + ", " + h.ToString() + ")";
                VisualLayer[neuronIndex] = neuron.GetComponent<SNN>();
                /*VisualLayer[neuronIndex].stochasticFiring = true;
                VisualLayer[neuronIndex].stochasticityOdds = 2;*/
                VisualLayer[neuronIndex].EnableSpikeTimingPlasticity = false;
                neuronIndex++;
            }
        }

        for (int i = 0; i < numLayers; i++)
        {
            layers.Add(new SNN[layerSizes[i]]);
        }

        // Hidden Layers
        int layerNumber = 0;
        float sqsum = width + 1;
        foreach (SNN[] layer in layers)
        {
            string layerName = "Hidden Layer ";
            if (layerNumber.Equals(numLayers - 1))
            {
                layerName = "Output Layer ";
            }
            GameObject layerParent = new GameObject(layerName + (1 + layerNumber).ToString());
            layerParent.transform.position = new Vector3(0, (int)((height - layer.Length) / 2), layerNumber * 3f);
            layerParent.transform.parent = this.gameObject.transform;

            float sqlayer = Mathf.Sqrt(layer.Length);
            
            // Individual Neurons
            for (int i = 0; i < layer.Length; i++)
            {
                float xpos = (int)(i/sqlayer) * neuronSpacing;
                float ypos = (i % sqlayer) * neuronSpacing;

                GameObject NeuronI;
                if (layerNumber < numLayers - 1)
                {
                    NeuronI = Instantiate(NeuronPrefab, transform.position + new Vector3(sqsum + xpos, ypos + (height - sqlayer)/2, 0), Quaternion.identity, layerParent.transform);
                }
                else
                {
                    NeuronI = Instantiate(NeuronPrefab, transform.position + new Vector3(sqsum, (i * neuronSpacing) + (height - layer.Length)/2, 0), Quaternion.identity, layerParent.transform);
                    SNN outputNeuron = NeuronI.GetComponent<SNN>();
                    mnistScript.outputNeurons.Add(outputNeuron);
                    outputNeuron.isAnOutput = true;
                }
                SNN neuronScript = NeuronI.GetComponent<SNN>();
                layer[i] = neuronScript;
                /*if (layerNumber < numLayers - 1)
                {
                    neuronScript.stochasticFiring = useStochasticHiddenLayers;
                    neuronScript.stochasticityOdds = stochasticityOdds;
                }*/
                if (!neuronScript.isAnOutput)
                {
                    NeuronI.name = "Layer" + layerNumber.ToString() + "_" + i.ToString();
                }
                else
                {
                    neuronScript.competitionRadius = 0f;
                    NeuronI.name = "Output_" + i.ToString();
                }
                neuronScript.network_layer = layerNumber;
                neuronScript.layer_position = i;

                // Connection Portion
                // Full Connectivity
                if (layerNumber > 0)
                {
                    foreach (SNN lastNeuron in layers[layerNumber - 1])
                    {
                        layer[i].AddInputNeuron(lastNeuron);
                        if (layerRecurrency && !neuronScript.isAnOutput && layerNumber > 0)
                        {
                            layer[i].AddOutputNeuron(lastNeuron);
                        }
                    }
                }

                if (lateralConnections && layerNumber < (numLayers - 1))
                {
                    // Individual Neurons
                    for (int j = 0; j < i; j++)
                    {
                        if (Vector3.Distance(layer[i].transform.position, layer[j].transform.position) <= Mathf.Sqrt(2))
                        {
                            layer[j].AddInputNeuron(layer[i]);
                            layer[i].AddInputNeuron(layer[j]);
                        }
                    }
                }


                if (recurrentEdgeConnections && layerNumber < (numLayers - 1))
                {
                    if (i % sqlayer == 0 || i < sqlayer || i + sqlayer > (sqlayer * sqlayer) || (i+1) % sqlayer == 0)
                    {
                        layer[i].AddInputNeuron(layer[i]); // Edge Self-Connection
                    }
                            
                }
            }
            layerNumber++;
            sqsum += sqlayer + 1;
        }

        for (int j = 0; j < VisualLayer.Length; j++)
        {
            SNN visualCell = VisualLayer[j] as SNN;
            foreach (SNN neuron in layers[0])
            {
                neuron.AddInputNeuron(visualCell);
            }
        }
    }

    static public Color[] GetRTPixels(RenderTexture rt)
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
        return tex.GetPixels();
    }

    void SendValues()
    {
        Color[] pixels = GetRTPixels(image);
        currentStep++;

        if (VisualLayer[0].GetType() == typeof(SNN))
        {
            for (int i = 0; i < VisualLayer.Length; i++)
            {
                Color pixel = pixels[i];
                if (colorSensitivity == ColorSensitivity.red)
                {
                    pixel.g = 0f;
                    pixel.b = 0f;
                }
                else if (colorSensitivity == ColorSensitivity.green)
                {
                    pixel.r = 0f;
                    pixel.b = 0f;
                }
                else if (colorSensitivity == ColorSensitivity.blue)
                {
                    pixel.r = 0f;
                    pixel.g = 0f;
                }

                float frequency = pixel.grayscale;
                SNN neuron = VisualLayer[i] as SNN;

                if (frequency > 0.1f)
                {
                    neuron.currentPotential += (frequency * FrequencyScalar);
                }
                if (visualize)
                {
                    if (showColor)
                    {
                        neuron.GetComponent<Renderer>().material.color = pixel; // Color Vis
                    }
                    else
                    {
                        neuron.GetComponent<Renderer>().material.color = new Color(frequency, frequency, frequency); // Frequency Vis
                    }
                }
            }
        }
    }
}
