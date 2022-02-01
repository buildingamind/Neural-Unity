using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    public bool spawnAgent = true;
    public bool spawnFood = true;
    public bool spawnBomb = false;

    public GameObject agentPrefab;
    public GameObject foodPrefab;
    public GameObject bombPrefab;


    public float range;
    public float agentRate = 4f;
    public float foodRate = 2f;
    public float bombRate = 10f;

    int AgentID = 0;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnAgent", 1f, agentRate);
        InvokeRepeating("SpawnFood", 0f, foodRate);
        InvokeRepeating("SpawnBomb", 0f, bombRate);
    }

    void SpawnAgent()
    {
        if (!spawnAgent)
        {
            return;
        }

        GameObject newAgent;
        AgentID++;
        if (Random.Range(0, 100) < 50f && AgentManager.leaderAgent != null)
        {
            GameObject leaderAgent = AgentManager.leaderAgent.gameObject;
            newAgent = Instantiate(leaderAgent, transform.position + new Vector3(Random.Range(-90, 90) / 10f, 0f, Random.Range(-100, 100) / 10f), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), transform.parent);

            BaseAgent agentScript = newAgent.GetComponent<BaseAgent>();
            CTRNN[] leaderNetwork = leaderAgent.GetComponentsInChildren<CTRNN>();
            CTRNN[] newAgentNetwork = GetComponentsInChildren<CTRNN>();

            for (int i = 0; i < newAgentNetwork.Length; i++)
            {
                for (int j = 0; j < newAgentNetwork[i].inputWeights.Count; j++)
                {
                    newAgentNetwork[i].inputWeights[j] = leaderNetwork[i].inputWeights[j] + Random.Range(-100, 100) / 1000f;
                    newAgentNetwork[i].bias += Random.Range(-1000, 1000) / 10000f;
                }
            }
            agentScript.bodyColor = Color.grey;
            newAgent.name = "ClonedAgent_" + AgentID.ToString();
        }
        else
        {
            newAgent = Instantiate(agentPrefab, transform.position + new Vector3(Random.Range(-90, 90) / 10f, 0f, Random.Range(-100, 100) / 10f),Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), transform.parent);
            BaseAgent agentScript = newAgent.GetComponent<BaseAgent>();
            agentScript.bodyColor = Color.white;
            newAgent.name = "RandomAgent_" + AgentID.ToString();
        }
    }

    void SpawnFood()
    {
        if (!spawnFood)
        {
            return;
        }
        Instantiate(foodPrefab, transform.position + new Vector3(Random.Range(-range * 10, range * 10) / 10f, 0f, Random.Range(-range * 10, range * 10) / 10f), 
            Quaternion.identity, transform.parent);
    }

    void SpawnBomb()
    {
        if (!spawnBomb)
        {
            return;
        }
        Instantiate(bombPrefab, transform.position + new Vector3(Random.Range(-range * 10, range * 10) / 10f, 0f, Random.Range(-range * 10, range * 10) / 10f),
            Random.rotation, transform.parent);
    }

}
