using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class BoidsDataManager : MonoBehaviour
{
    public static BoidsDataManager Instance;

    [HideInInspector] public EntityManager manager;
    [HideInInspector] public float neighbourDistance;
    [HideInInspector] public float avoidDistance;
    [HideInInspector] public float3 goalPos = float3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
}