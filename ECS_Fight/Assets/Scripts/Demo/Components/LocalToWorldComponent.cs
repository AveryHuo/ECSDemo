using Unity.Entities;
using Unity.Mathematics;

namespace Test
{
    public struct LocalToWorldComponent : IComponentData
    {
        public float4x4 Value;
    }
}