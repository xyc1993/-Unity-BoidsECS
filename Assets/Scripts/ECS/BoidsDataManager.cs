using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;

public class BoidsDataManager : MonoBehaviour
{
    public static BoidsDataManager Instance;

    [HideInInspector] public EntityManager manager;
    [HideInInspector] public BuildPhysicsWorld buildPhysicsWorld;
    [HideInInspector] public float minSpeed;
    [HideInInspector] public float maxSpeed;
    [HideInInspector] public float neighbourDistance;
    [HideInInspector] public float avoidDistance;
    [HideInInspector] public float avoidCollidersDistance;
    [HideInInspector] public bool simpleBehaviour = false;
    [HideInInspector] public float3 goalPos = float3.zero;
    [HideInInspector] public Bounds bounds;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
}