using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public class BoidSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        EntityQuery entityQuery = BoidsDataManager.Instance.manager.CreateEntityQuery(ComponentType.ReadOnly<BoidData>());
        NativeArray<BoidData> boidsData = entityQuery.ToComponentDataArray<BoidData>(Allocator.TempJob);

        float agentNeighbourDistance = BoidsDataManager.Instance.neighbourDistance;
        float agentAvoidDistance = BoidsDataManager.Instance.avoidDistance;
        float3 goalPos = BoidsDataManager.Instance.goalPos;

        var jobHandle = Entities
            .WithName("BoidSystem")
            .ForEach((ref Translation position, ref Rotation rotation, ref BoidData boidData) =>
            {
                //apply rules
                float3 centre = float3.zero;
                float3 avoid = float3.zero;
                float groupSpeed = 0.01f;
                float neighbourDistance;
                int groupSize = 0;

                for (int i = 0; i < boidsData.Length; i++)
                {
                    if (boidData.Index != boidsData[i].Index)
                    {
                        float3 boidsVector = boidData.CurrentPosition - boidsData[i].CurrentPosition;
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
                    centre = centre / groupSize + (goalPos - boidData.CurrentPosition);
                    boidData.Speed = groupSpeed / groupSize;

                    float3 direction = (centre + avoid) - boidData.CurrentPosition;
                    if (math.length(direction - float3.zero) > 0.05f)
                    {
                        rotation.Value = math.slerp(rotation.Value, quaternion.LookRotation(direction, new float3(0,1,0)), boidData.RotationSpeed * deltaTime);
                    }                        
                }

                position.Value += boidData.Speed * math.forward(rotation.Value) * deltaTime;
                boidData.CurrentPosition = position.Value;
            })
            .Schedule(inputDeps);

        jobHandle.Complete();
        boidsData.Dispose();

        return jobHandle;
    }
}