using Unity.Entities;

[GenerateAuthoringComponent]
public struct PrefabConversion : IComponentData
{
    public Entity prefab;
}
