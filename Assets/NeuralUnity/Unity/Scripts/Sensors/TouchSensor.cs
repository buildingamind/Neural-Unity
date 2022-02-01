using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TouchSensor : BaseSensor
{
    [Header("[?] Layer Mask: The layer type which will activate the target neuron")]
    public LayerMask layerMask;
    public CTRNN feedback;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();    
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Mathf.Log(layerMask.value, 2))
        {
            sensorValue = 1;
            collision.gameObject.GetComponent<Rigidbody>().AddForce((-transform.parent.right) * feedback.value * 5f, ForceMode.Impulse);
        }
        else
        {
            sensorValue = 0;
        }
    }
}
