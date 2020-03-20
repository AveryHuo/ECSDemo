using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Test
{
    public struct PositionComponent : IComponentData
    {
        public float3 Value;
    }
}