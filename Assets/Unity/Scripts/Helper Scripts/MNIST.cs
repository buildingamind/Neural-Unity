using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MNIST : MonoBehaviour
{
    public int currentDigit = 0;

    [Header("The array of images associated with each digit")]
    public Texture2D[] digit0;
    public Texture2D[] digit1;
    public Texture2D[] digit2;
    public Texture2D[] digit3;
    public Texture2D[] digit4;
    public Texture2D[] digit5;
    public Texture2D[] digit6;
    public Texture2D[] digit7;
    public Texture2D[] digit8;
    public Texture2D[] digit9;

    [Header("The distribution of spikes from each output neuron according to number")]
    public List<int> Dist0 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist1 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist2 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist3 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist4 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist5 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist6 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist7 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist8 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public List<int> Dist9 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    [Header("The list of neurons which represent the output layer")]
    public List<SNN> outputNeurons;
    public List<int> associatedNeuronIndex;

    public List<Texture2D[]> digits;
    [SerializeField] public List<List<int>> counts = new List<List<int>>();

    public List<int> digitHistory = new List<int>();

    public float ChangeFrequency = 3f;
    private Renderer rend;

    void Start()
    {
        associatedNeuronIndex = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        counts = new List<List<int>>() { Dist0, Dist1, Dist2, Dist3, Dist4, Dist5, Dist6, Dist7, Dist8, Dist9 };
        digits = new List<Texture2D[]>() { digit0, digit1, digit2, digit3, digit4, digit5, digit6, digit7, digit8, digit9 };
        rend = GetComponent<Renderer>();


        if (ChangeFrequency > 0)
        {
            InvokeRepeating(nameof(ChangeDigit), 0f, ChangeFrequency);
        }     
    }

    private void Update()
    {
        int neuronIndex = 0; foreach (SNN neuron in outputNeurons)
        {
            if (neuron.numSpikes > 0)
            {
                counts[currentDigit][neuronIndex] += neuron.numSpikes;
            }
            neuron.numSpikes = 0;
            neuronIndex++;
        }
    }

    private void OnGUI()
    {
        string text = "Neuron:\t0\t1\t2\t3\t4\t5\t6\t7\t8\t9\tMax Association\n";
        for (int x = 0; x < counts.Count; x++) 
        {
            text += "Digit " + x.ToString() + ":\t";
            for (int y = 0; y < counts[x].Count; y++)
            {
                text += counts[x][y] + "\t";
            }
            string showAssociation = counts[x].Sum() == 0 ? "-" : "(Neuron " + associatedNeuronIndex[x].ToString() + ")";
            text += showAssociation + "\n";
        }

        GUI.Label(new Rect(20, 10, 3000, 300), text);
    }

    void ChangeDigit()
    {
        for (int i = 0; i < 10; i++)
        {
            if (counts[i].Sum() != 0)
            {
                associatedNeuronIndex[i] = ArgMax(counts[i]);
            }
        }

        currentDigit = Random.Range(0, 10);
        int instance = Random.Range(0, digits[currentDigit].Length);
        rend.material.mainTexture = digits[currentDigit][instance];
        digitHistory.Add(currentDigit);
    }

    public int ArgMax(List<int> argmax)
    {
        List<int> copy = argmax;
        int[] c = copy.ToArray();
        argmax.CopyTo(c);
        copy = c.ToList();
        copy.Sort();
        return argmax.IndexOf(copy[copy.Count - 1]);
    }
}
