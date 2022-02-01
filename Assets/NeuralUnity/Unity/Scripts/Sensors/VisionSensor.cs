using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionSensor : BaseSensor
{
    [Header("[?] Layer Mask: The layer type which will activate the neuron")]
    public LayerMask layerMask;

    public enum Channel { distance, red, green, blue };
    [Header("[?] What Vision Component does the sensor detect? [Default: Distance]")]
    public Channel channel = Channel.distance;

    [Header("[?] Range: The Distance at which the sensor can detect")]
    public float range = 10f;
    Vector3 cast;
    Renderer rend;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    public override void Update()
    {
        cast = transform.position + (transform.forward * range);
        RaycastHit hit;

        // This is just eye candy

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Linecast(transform.position, cast, out hit, layerMask))
        {
            switch (channel)
            {
                case Channel.distance:
                    sensorValue = hit.distance;
                    break;

                case Channel.red:
                    sensorValue = hit.collider.gameObject.GetComponent<Renderer>().material.color.r;
                    break;

                case Channel.green:
                    sensorValue = hit.collider.gameObject.GetComponent<Renderer>().material.color.g;
                    break;

                case Channel.blue:
                    sensorValue = hit.collider.gameObject.GetComponent<Renderer>().material.color.b;
                    break;
            }
            Debug.DrawLine(transform.position, cast, Color.red);
            
        }
        else
        {
            sensorValue *= sustain;
            Debug.DrawLine(transform.position, cast, Color.blue);
        }
        targetNeuron.value = sensorValue;

        switch (channel)
        {
            case Channel.distance:
                rend.material.SetColor("_BaseColor", Color.Lerp(Color.white, Color.black, sensorValue));  // For Universal Rendering
                rend.material.SetColor("_Color", Color.Lerp(Color.white, Color.black, sensorValue));  // For Standard Shading
                break;
            case Channel.red:
                rend.material.SetColor("_BaseColor", Color.Lerp(Color.white, new Color(sensorValue, 0, 0), sensorValue));  // For Universal Rendering
                rend.material.SetColor("_Color", Color.Lerp(Color.white, new Color(sensorValue, 0, 0), sensorValue));  // For Standard Shading
                break;
            case Channel.green:
                rend.material.SetColor("_BaseColor", Color.Lerp(Color.white, new Color(0, sensorValue, 0), sensorValue));  // For Universal Rendering
                rend.material.SetColor("_Color", Color.Lerp(Color.white, new Color(0, sensorValue, 0), sensorValue));  // For Standard Shading
                break;
            case Channel.blue:
                rend.material.SetColor("_BaseColor", Color.Lerp(Color.white, new Color(0, 0, sensorValue), sensorValue));  // For Universal Rendering
                rend.material.SetColor("_Color", Color.Lerp(Color.white, new Color(0, 0, sensorValue), sensorValue));  // For Standard Shading
                break;
        }
    }

}
