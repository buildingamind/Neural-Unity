using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BaseAgent : MonoBehaviour
{
    Renderer rend;
    public Color bodyColor = Color.white;
    public float food = 10f;
    public float lifetime;


    // Update is called once per frame
    public virtual void Start()
    {
        if (!AgentManager.agents.Contains(this.gameObject))
        {
            AgentManager.agents.Add(this.gameObject);
        }
        rend = GetComponent<Renderer>();
    }

    public virtual void Update()
    {
        lifetime += Time.deltaTime;

        if (AgentManager.leaderAgent == null || lifetime > AgentManager.leaderAgent.GetComponent<BaseAgent>().lifetime)
        {
            AgentManager.leaderAgent = this.gameObject;
        }
        

        if (AgentManager.leaderAgent != null && AgentManager.leaderAgent.Equals(this.gameObject))
        {
            rend.material.SetColor("_BaseColor", Color.blue);  // For Universal Rendering
            rend.material.SetColor("_Color", Color.blue);  // For Standard Shading
        }
        else
        {
            rend.material.SetColor("_BaseColor", bodyColor);  // For Universal Rendering
            rend.material.SetColor("_Color", bodyColor);  // For Standard Shading
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("killzone"))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (AgentManager.agents.Contains(this.gameObject))
        {
            AgentManager.agents.Remove(this.gameObject);
        }
    }
}
