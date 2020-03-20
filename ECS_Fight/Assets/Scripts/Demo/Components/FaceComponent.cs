using Unity.Entities;
using UnityEngine;

namespace Test
{
    public struct FaceComponent : IComponentData
    {
        public Quaternion Value;
    }
}