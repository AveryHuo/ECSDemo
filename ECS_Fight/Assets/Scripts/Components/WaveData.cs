using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct WaveData : IComponentData
{
    public float amplitude;
    public float xOffset;
    public float yOffset;
}

