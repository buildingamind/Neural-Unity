using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawUnitVector : MonoBehaviour
{

    LineRenderer line;
    public Vector3 value;
    public string seed = "Joshua";
    public GameObject pointer;

    public bool readAsLoop;
    public bool zeroBetweenReads;
    public float lerpSpeed = 0.5f;
    public float loopSpeed = 0.5f;

    Renderer rend;
    int counter = 1;
    public string currentlyEncoded;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        rend = pointer.GetComponent<Renderer>();
        line.positionCount =  2;
        counter = 0;

        if (readAsLoop)
        {
            Zero();
            InvokeRepeating(nameof(Next), 1f, loopSpeed);
            if (zeroBetweenReads)
            {
                InvokeRepeating(nameof(Zero), 1 + (loopSpeed / 2f), loopSpeed);
            }
        }
        else
        {
            InvokeRepeating(nameof(Next), 0f, 0.16f);
        }

    }

    void Next()
    {
        if (readAsLoop)
        {
            if (seed.Length == 0)
            {
                currentlyEncoded = "";
            }
            else
            {
                currentlyEncoded = seed[counter % seed.Length].ToString();
                counter++;
            }
        }
        else
        {
            currentlyEncoded = seed;
        }

        value = GetRandomVector3(currentlyEncoded).normalized;
        value[0] = (float)Math.Tanh(value[0] - 0.5f);
        value[1] = (float)Math.Tanh(value[1] - 0.5f);
        value[2] = (float)Math.Tanh(value[2] - 0.5f);
        value = (value.normalized) / 2f;
    }

    void Zero()
    {
        value = Vector3.zero;
    }

    public static float Sigmoid(double value)
    {
        return 1 / (1.0f + Mathf.Exp(-(float)value));
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, transform.position);
        pointer.transform.position = Vector3.Lerp(pointer.transform.position, transform.position + value, lerpSpeed);
        line.SetPosition(1, Vector3.Lerp(pointer.transform.position, pointer.transform.position, lerpSpeed));

        float vectorSum = Math.Abs(value[0]) + Math.Abs(value[1]) + Math.Abs(value[2]);
        Color valueColor = new Color(Math.Abs(pointer.transform.localPosition.x)/vectorSum, Math.Abs(pointer.transform.localPosition.y)/vectorSum, Math.Abs(pointer.transform.localPosition.z)/vectorSum);
        line.startColor = valueColor;
        line.endColor = valueColor;

        rend.material.SetColor("_BaseColor", valueColor);  // For Universal Rendering
        rend.material.SetColor("_Color", valueColor);  // For Standard Shading
    }

    // We generate a random Vector given a string seed
    private static Vector3 GetRandomVector3(string seed)
    {
        Vector3 vec = new Vector3(0, 0, 0);
        using (var algo = System.Security.Cryptography.SHA256.Create()) 
        {
            var hash = BitConverter.ToInt32(algo.ComputeHash(Encoding.UTF8.GetBytes(seed)), 0);
            System.Random generator = new System.Random(hash);
            for (int i=0; i<3; i++)
            {
                vec[i] = generator.Next();
            }
        }
        return vec;
    }
}
