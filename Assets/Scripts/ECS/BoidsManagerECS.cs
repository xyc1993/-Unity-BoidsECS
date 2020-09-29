using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

public class BoidsManagerECS : MonoBehaviour
{
    [Header("Flock Settings")]
    public GameObject[] agentsPrefabs;
    public GameObject goal;
    public int numAgents = 20;
    public Vector3 moveLimits = new Vector3(5.0f, 5.0f, 5.0f); //bounds of the flock movement
    public bool simpleBehaviour = false;

    private BlobAssetStore store;
    private Entity[] entityPrefabs;

    [Header("Agent Settings")]
    [Range(0.0f, 5.0f)] public float minSpeed;
    [Range(0.0f, 5.0f)] public float maxSpeed;
    [Range(1.0f, 10.0f)] public float neighbourDistance;
    [Range(0.0f, 3.0f)] public float avoidDistance;
    [Range(0.0f, 5.0f)] public float rotationSpeed;
    [Range(0.0f, 10.0f)] public float avoidCollidersDistance;

    private void Start()
    {
        InitData();
        SetBoidDataManagerAgentData();
        SpawnEntities();
    }

    private void SetBoidDataManagerAgentData()
    {
        BoidsDataManager.Instance.minSpeed = minSpeed;
        BoidsDataManager.Instance.maxSpeed = maxSpeed;
        BoidsDataManager.Instance.neighbourDistance = neighbourDistance;
        BoidsDataManager.Instance.avoidDistance = avoidDistance;
        BoidsDataManager.Instance.avoidCollidersDistance = avoidCollidersDistance;
        BoidsDataManager.Instance.simpleBehaviour = simpleBehaviour;
        BoidsDataManager.Instance.bounds = new Bounds(transform.position, 2.0f * moveLimits);
    }

    private void InitData()
    {
        store = new BlobAssetStore();
        BoidsDataManager.Instance.manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, store);

        entityPrefabs = new Entity[agentsPrefabs.Length];
        for (int i = 0; i < agentsPrefabs.Length; i++)
        {
            entityPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentsPrefabs[i], settings);
        }
    }

    private void SpawnEntities()
    {
        for (uint i = 0; i < numAgents; i++)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0, entityPrefabs.Length);
            float speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            float3 position = GetRandomPositionWithinMoveLimits();            

            Entity instantiatedEntity = BoidsDataManager.Instance.manager.Instantiate(entityPrefabs[randomIndex]);
            BoidsDataManager.Instance.manager.SetComponentData(instantiatedEntity, new Translation { Value = position });
            BoidsDataManager.Instance.manager.SetComponentData(instantiatedEntity, new BoidData { Index = i, Speed = speed, RotationSpeed = rotationSpeed, CurrentPosition = position });
        }        
    }

    private void Update()
    {
        SetBoidDataManagerAgentData(); //for supporting inspector values change
        UpdateGoalPosition();
    }

    private void UpdateGoalPosition()
    {
        if (goal != null) BoidsDataManager.Instance.goalPos = goal.transform.position;
        else if (UnityEngine.Random.Range(0, 100) < 2) BoidsDataManager.Instance.goalPos = GetRandomPositionWithinMoveLimits();
    }

    private void OnDestroy()
    {
        store.Dispose();
    }

    private float3 GetRandomPositionWithinMoveLimits()
    {
        return (float3)transform.position + new float3(UnityEngine.Random.Range(-moveLimits.x, moveLimits.x),
                                                       UnityEngine.Random.Range(-moveLimits.y, moveLimits.y),
                                                       UnityEngine.Random.Range(-moveLimits.z, moveLimits.z));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, 2.0f * moveLimits);
    }
}