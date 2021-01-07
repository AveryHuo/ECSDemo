using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerSpawnTag : IComponentData
{
    public Entity prefab;
}


