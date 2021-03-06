﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;

public class BoidSystem : JobComponentSystem
{
    public struct BoidSystemData
    {
        public float minSpeed { get; private set; }
        public float maxSpeed { get; private set; }
        public float agentNeighbourDistance { get; private set; }
        public float agentAvoidDistance { get; private set; }
        public float avoidCollidersDistance { get; private set; }
        public bool simpleBehaviour { get; private set; }
        public float3 goalPos { get; private set; }
        public Bounds bounds { get; private set; }
        public uint baseSeed { get; private set; }

        public void SetData()
        {
            minSpeed = BoidsDataManager.Instance.minSpeed;
            maxSpeed = BoidsDataManager.Instance.maxSpeed;
            agentNeighbourDistance = BoidsDataManager.Instance.neighbourDistance;
            agentAvoidDistance = BoidsDataManager.Instance.avoidDistance;
            avoidCollidersDistance = BoidsDataManager.Instance.avoidCollidersDistance;
            simpleBehaviour = BoidsDataManager.Instance.simpleBehaviour;
            goalPos = BoidsDataManager.Instance.goalPos;
            bounds = BoidsDataManager.Instance.bounds;
            baseSeed = (uint)UnityEngine.Random.Range(500, 6000);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        EntityQuery entityQuery = BoidsDataManager.Instance.manager.CreateEntityQuery(ComponentType.ReadOnly<BoidData>());
        NativeArray<BoidData> boidsData = entityQuery.ToComponentDataArray<BoidData>(Allocator.TempJob);

        BoidSystemData boidSystemData = new BoidSystemData();
        boidSystemData.SetData();
        PhysicsWorld world = BoidsDataManager.Instance.buildPhysicsWorld.PhysicsWorld;

        var jobHandle = Entities
            .WithName("BoidSystem")
            .ForEach((ref Translation position, ref Rotation rotation, ref BoidData boid) =>
            {
                if (boidSystemData.simpleBehaviour)
                {
                    SimpleUpdate(ref position, ref rotation, ref boid, boidsData, boidSystemData.agentNeighbourDistance, 
                                 boidSystemData.agentAvoidDistance, boidSystemData.goalPos, deltaTime);
                }
                else
                {
                    FullUpdate(ref position, ref rotation, ref boid, boidsData, boidSystemData.agentNeighbourDistance, boidSystemData.agentAvoidDistance, boidSystemData.goalPos, deltaTime,
                                boidSystemData.minSpeed, boidSystemData.maxSpeed, boidSystemData.bounds, boidSystemData.baseSeed, boidSystemData.avoidCollidersDistance, world);
                }
            })
            .Schedule(inputDeps);

        jobHandle.Complete();
        boidsData.Dispose();

        return jobHandle;
    }

    private static void FullUpdate(ref Translation position, ref Rotation rotation, ref BoidData boid, NativeArray<BoidData> boidsData, 
                                    float agentNeighbourDistance, float agentAvoidDistance, float3 goalPos, float deltaTime,
                                    float minSpeed, float maxSpeed, Bounds bounds, uint baseSeed, float avoidCollidersDistance, PhysicsWorld world)
    {
        float3 direction = float3.zero;
        float3 forward = math.forward(rotation.Value);
        bool turning;

        RaycastInput rayInput = new RaycastInput
        {
            Start = boid.CurrentPosition,
            End = boid.CurrentPosition + avoidCollidersDistance * forward,
            Filter = CollisionFilter.Default
        };

        if (!bounds.Contains(boid.CurrentPosition)) //handle getting out of bounds
        {
            turning = true;
            direction = (float3)bounds.center - boid.CurrentPosition;
        }
        else if (world.CastRay(rayInput, out Unity.Physics.RaycastHit hit))
        {
            turning = true;
            direction = (float3)Vector3.Reflect(forward, hit.SurfaceNormal);
        }
        else turning = false;

        if (turning) //adjust path if needed
        {
            rotation.Value = math.slerp(rotation.Value, quaternion.LookRotation(direction, new float3(0, 1, 0)), boid.RotationSpeed * deltaTime);
        }
        else //random direction or direction calculated based on the flocking rules
        {
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random(baseSeed + boid.Index);
            
            if (rand.NextInt(0, 100) < 10)
                boid.Speed = rand.NextFloat(minSpeed, maxSpeed);
            if (rand.NextInt(0, 100) < 20)
                ApplyBoidRules(ref boid, ref rotation, boidsData, agentNeighbourDistance, agentAvoidDistance, goalPos, deltaTime);
        }

        //move agent forward, along its direction
        position.Value += boid.Speed * math.forward(rotation.Value) * deltaTime;
        boid.CurrentPosition = position.Value;
    }

    private static void SimpleUpdate(ref Translation position, ref Rotation rotation, ref BoidData boid, NativeArray<BoidData> boidsData, 
                                        float agentNeighbourDistance, float agentAvoidDistance, float3 goalPos, float deltaTime)
    {
        ApplyBoidRules(ref boid, ref rotation, boidsData, agentNeighbourDistance, agentAvoidDistance, goalPos, deltaTime);
        position.Value += boid.Speed * math.forward(rotation.Value) * deltaTime;
        boid.CurrentPosition = position.Value;
    }

    private static void ApplyBoidRules(ref BoidData boid, ref Rotation rotation, NativeArray<BoidData> boidsData, 
                                        float agentNeighbourDistance, float agentAvoidDistance, float3 goalPos, float deltaTime)
    {
        float3 centre = float3.zero;
        float3 avoid = float3.zero;
        float groupSpeed = 0.01f;
        float neighbourDistance;
        int groupSize = 0;

        for (int i = 0; i < boidsData.Length; i++)
        {
            if (boid.Index != boidsData[i].Index)
            {
                float3 boidsVector = boid.CurrentPosition - boidsData[i].CurrentPosition;
                neighbourDistance = math.length(boidsVector);
                if (neighbourDistance <= agentNeighbourDistance)
                {
                    groupSize++;
                    groupSpeed += boidsData[i].Speed;
                    centre += boidsData[i].CurrentPosition;
                    if (neighbourDistance < agentAvoidDistance)
                    {
                        avoid += boidsVector;
                    }
                }
            }
        }

        if (groupSize > 0)
        {
            centre /= groupSize;
            boid.Speed = groupSpeed / groupSize;

            float3 direction = (centre + goalPos + avoid) - boid.CurrentPosition;
            if (math.length(direction - float3.zero) > 0.05f)
            {
                rotation.Value = math.slerp(rotation.Value, quaternion.LookRotation(direction, new float3(0, 1, 0)), boid.RotationSpeed * deltaTime);
            }
        }
    }
}