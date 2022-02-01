using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveNetwork : MonoBehaviour
{
    // This script is used to export neuron histories to a text file
    CTRNN[] network;
    public float SaveFrequency = 30f; // This is independent of the neuron's history logging frequency, it's only the frequency that the data is exported.
    int SaveID = 1;

    public bool UseAgentManager;
    [Header("If UseAgentManger is enabled, this values will get set automatically.")]
    public List<GameObject> NetworkParents;
    
    // Start is called before the first frame update
    void Start()
    {
        if (UseAgentManager)
        {
            NetworkParents = AgentManager.agents;
        }
        if (SaveFrequency > 0)
        {
            InvokeRepeating(nameof(ExportNetworkHistory), 0, SaveFrequency);
        }
        
    }

    void ExportNetworkHistory()
    {
        Debug.Log("Saving Network History...");
        using (StreamWriter w = new StreamWriter(Application.dataPath + string.Format("/CTRNN_checkpoint_{0}.csv", SaveID.ToString("00000"))))
        {
            foreach (GameObject networkParent in NetworkParents)
            {
                foreach (CTRNN neuron in networkParent.GetComponentsInChildren<CTRNN>())
                {
                    string values = networkParent.name + "_" + neuron.gameObject.name;
                    foreach (float value in neuron.ActivationHistory)
                    {
                        values += "," + value.ToString();
                    }
                    w.WriteLine(values);
                    w.Flush();
                }
            }
        }
    }
}
