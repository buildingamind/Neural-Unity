using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CTRNN))]
public class OutputFromNeuron : MonoBehaviour
{

    [Header("[?] This script is placed on a controlling neuron where the target is specified")]
    public GameObject target;  // The target object to be controlled
    public float amplifier = 1f;
    CTRNN neuron;
    public enum Action { MoveX, TurnX, TurnY, TurnZ, PistonX }
    public Action action;

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
                target.transform.position -= target.transform.right * neuron.value * Time.deltaTime * amplifier;
                break;

            case Action.TurnZ:
                target.transform.Rotate(new Vector3(0, 0, neuron.value * Time.deltaTime * amplifier));
                break;

            case Action.TurnY:
                target.transform.Rotate(new Vector3(0, neuron.value * Time.deltaTime * amplifier, 0));
                break;

            case Action.TurnX:
                target.transform.Rotate(new Vector3(neuron.value * Time.deltaTime * amplifier, 0, 0));
                break;

            case Action.PistonX:
                target.transform.position = pistonStartPos.transform.position + (-transform.right * amplifier * neuron.value);
                break;
        }
    }
}
