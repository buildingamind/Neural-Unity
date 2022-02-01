using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungryAgent : BaseAgent
{

    [Header("If used, routes the hunger value to a neuron in a network as input.")]
    public CTRNN HungerNeuron;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        food -= Time.deltaTime;
        HungryAgent leaderAgentScript = AgentManager.leaderAgent.GetComponent<HungryAgent>();
        if (AgentManager.leaderAgent == null)
        {
            AgentManager.leaderAgent = this.gameObject;
        }
        if (lifetime > leaderAgentScript.lifetime)
        {
            AgentManager.leaderAgent = this.gameObject;
        }

        if (food <= 0)
        {
            Destroy(this.gameObject);  // Once an object is destroyed, any references will become null.
        }
        if (HungerNeuron != null)
        {
            HungerNeuron.value = food;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer.Equals(9))
        {
            food += 10;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.layer.Equals(11))
        {
            food -= 100;
            Destroy(collision.gameObject);
        }
    }
}
