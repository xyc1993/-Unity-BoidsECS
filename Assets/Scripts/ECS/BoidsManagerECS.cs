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
    public int numAgents = 20;
    public Vector3 moveLimits = new Vector3(5.0f, 5.0f, 5.0f); //bounds of the flock movement

    private BlobAssetStore store;
    private EntityManager entityManager;
    private Entity[] entityPrefabs;

    private void Start()
    {
        InitData();
        SpawnEntities();
    }

    private void InitData()
    {
        store = new BlobAssetStore();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, store);

        entityPrefabs = new Entity[agentsPrefabs.Length];
        for (int i = 0; i < agentsPrefabs.Length; i++)
        {
            entityPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentsPrefabs[i], settings);
        }
    }

    private void SpawnEntities()
    {
        for (int i = 0; i < numAgents; i++)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0, entityPrefabs.Length);
            float3 position = GetRandomPositionWithinMoveLimits();

            Entity instantiatedEntity = entityManager.Instantiate(entityPrefabs[randomIndex]);
            entityManager.SetComponentData(instantiatedEntity, new Translation { Value = position });
        }        
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
}