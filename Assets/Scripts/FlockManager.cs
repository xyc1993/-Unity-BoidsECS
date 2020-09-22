using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public GameObject[] agentsPrefabs;
    public int numAgents = 20;
    public Vector3 moveLimits = new Vector3(5.0f, 5.0f, 5.0f); //bounds of the flock movement
    internal GameObject[] allAgents;
    public GameObject goal;
    internal Vector3 goalPos;

    public bool simpleBehaviour = false;
    public bool debug = true;

    [Space(10)]
    [Header("Agent Settings")]
    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    [Range(1.0f, 10.0f)]
    public float neighbourDistance;
    [Range(0.0f, 3.0f)]
    public float avoidDistance;
    [Range(0.0f, 5.0f)]
    public float rotationSpeed;
    [Range(0.0f, 10.0f)]
    public float avoidCollidersDistance;

    private void Start ()
    {
        allAgents = new GameObject[numAgents];
        for(int i = 0; i < numAgents; i++)
        {
            Vector3 pos = GetRandomPositionWithinMoveLimits();
            int randomIndex = (int)Random.Range(0, agentsPrefabs.Length);
            allAgents[i] = (GameObject)Instantiate(agentsPrefabs[randomIndex], pos, Quaternion.identity);
            allAgents[i].GetComponent<Flock>().flockManager = this;
        }
    }

    private void Update ()
    {
        if (goal != null)
            goalPos = goal.transform.position;
        else if(Random.Range(0, 100) < 2)
            goalPos = GetRandomPositionWithinMoveLimits();
    }

    private Vector3 GetRandomPositionWithinMoveLimits()
    {
        return this.transform.position + new Vector3(Random.Range(-moveLimits.x, moveLimits.x),
                                                                Random.Range(-moveLimits.y, moveLimits.y),
                                                                Random.Range(-moveLimits.z, moveLimits.z));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, 2.0f * moveLimits);
    }
}
