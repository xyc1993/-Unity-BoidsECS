using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BoidData : IComponentData
{
    public int Index;
    public float Speed;
    public float RotationSpeed;
    public float3 CurrentPosition;
}