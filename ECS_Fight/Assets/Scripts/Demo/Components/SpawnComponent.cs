using Unity.Entities;

namespace Test
{
    public struct SpawnComponent : IComponentData
    {
        public int XSize;
        public int YSize;
        public float Spacing;
    }
}