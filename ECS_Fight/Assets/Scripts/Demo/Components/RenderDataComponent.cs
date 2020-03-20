using System;
using Unity.Entities;
using UnityEngine;

namespace Test
{
    public struct RenderDataComponent : ISharedComponentData, IEquatable<RenderDataComponent>
    {
        public Mesh MeshValue;
        public Material MaterialValue;

        public bool Equals(RenderDataComponent other)
        {
            return Equals(MeshValue, other.MeshValue) && Equals(MaterialValue, other.MaterialValue);
        }

        public override bool Equals(object obj)
        {
            return obj is RenderDataComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((MeshValue != null ? MeshValue.GetHashCode() : 0) * 397) ^ (MaterialValue != null ? MaterialValue.GetHashCode() : 0);
            }
        }
    }
}