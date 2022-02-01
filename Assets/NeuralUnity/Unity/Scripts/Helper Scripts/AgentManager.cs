using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static GameObject leaderAgent;
    public static List<GameObject> agents;
    public bool TimeWarp = false;


    // Start is called before the first frame update
    void Awake()
    {
        agents = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (leaderAgent == null || leaderAgent.GetComponent<BaseAgent>().lifetime < 300f && TimeWarp)
        {
            Time.timeScale = 10f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
