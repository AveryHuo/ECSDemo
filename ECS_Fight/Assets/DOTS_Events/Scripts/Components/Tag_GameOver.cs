using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Tag_GameOver : IComponentData {
    public Entity overCanvas;
}