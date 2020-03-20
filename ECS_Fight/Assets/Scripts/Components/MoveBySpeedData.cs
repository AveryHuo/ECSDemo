using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct MoveBySpeedData : IComponentData
{
    public float Value;
}
