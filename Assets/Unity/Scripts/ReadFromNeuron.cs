using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadFromNeuron : MonoBehaviour
{
    public float amplifier = 1f;

    [Header("[?] This script is placed on a target where the controlling neuron is specified")]
    public CTRNN neuron;
    public enum Action { MoveX, TurnX, TurnY, TurnZ,PistonX }
    public Action action;

    [Header("[?] Where is the resting state of the piston? (If Action is a Piston)")]
    public Transform pistonStartPos;

    // Start is called before the first frame update
    void Start()
    {
        neuron = GetComponent<CTRNN>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (action)
        {
            case Action.MoveX:
                this.gameObject.transform.position -= this.gameObject.transform.right * neuron.value * Time.deltaTime * amplifier;
                break;

            case Action.TurnZ:
                this.gameObject.transform.Rotate(new Vector3(0, 0, neuron.value * Time.deltaTime * amplifier));
                break;

            case Action.TurnY:
                this.gameObject.transform.Rotate(new Vector3(0, neuron.value * Time.deltaTime * amplifier, 0));
                break;

            case Action.TurnX:
                this.gameObject.transform.Rotate(new Vector3(neuron.value * Time.deltaTime * amplifier, 0, 0));
                break;

            case Action.PistonX:
                this.gameObject.transform.position = pistonStartPos.transform.position + (-transform.right * amplifier * neuron.value);
                break;
        }
    }
}
