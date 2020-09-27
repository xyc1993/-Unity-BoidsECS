using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    [Header("Flock Settings")]
    public GameObject[] agentsPrefabs;
    public GameObject goal;
    public int numAgents = 20;
    public Vector3 moveLimits = new Vector3(5.0f, 5.0f, 5.0f); //bounds of the flock movement
    public bool simpleBehaviour = false;
    public bool debug = true;

    [Header("Agent Settings")]
    [Range(0.0f, 5.0f)] public float minSpeed;
    [Range(0.0f, 5.0f)] public float maxSpeed;
    [Range(1.0f, 10.0f)] public float neighbourDistance;
    [Range(0.0f, 3.0f)] public float avoidDistance;
    [Range(0.0f, 5.0f)] public float rotationSpeed;
    [Range(0.0f, 10.0f)] public float avoidCollidersDistance;

    internal Vector3 goalPos;
    internal Bounds bounds;

    public Boid[] Boids { get; private set; }

    private void Start ()
    {
        Boids = new Boid[numAgents];

        for(int i = 0; i < numAgents; i++)
        {
            Vector3 pos = GetRandomPositionWithinMoveLimits();
            int randomIndex = (int)Random.Range(0, agentsPrefabs.Length);
            GameObject boidObject = (GameObject)Instantiate(agentsPrefabs[randomIndex], pos, Quaternion.identity, transform);
            Boid boid = boidObject.GetComponent<Boid>();
            boid.boidsManager = this;
            Boids[i] = boid;
        }
    }

    private void Update()
    {
        UpdateBounds();
        UpdateGoalPosition();
        UpdateBoids();
    }

    private void UpdateBounds()
    {
        bounds = new Bounds(transform.position, 2.0f * moveLimits);
    }

    private void UpdateGoalPosition()
    {
        if (goal != null) goalPos = goal.transform.position;
        else if (Random.Range(0, 100) < 2) goalPos = GetRandomPositionWithinMoveLimits();
    }

    private void UpdateBoids()
    {
        if (simpleBehaviour)
        {
            for (int i = 0; i < Boids.Length; i++)
                Boids[i].SimpleUpdate();
        }
        else
        {
            for (int i = 0; i < Boids.Length; i++)
                Boids[i].FullUpdate();
        }
    }

    private Vector3 GetRandomPositionWithinMoveLimits()
    {
        return transform.position + new Vector3(Random.Range(-moveLimits.x, moveLimits.x),
                                                Random.Range(-moveLimits.y, moveLimits.y),
                                                Random.Range(-moveLimits.z, moveLimits.z));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, 2.0f * moveLimits);
    }
}