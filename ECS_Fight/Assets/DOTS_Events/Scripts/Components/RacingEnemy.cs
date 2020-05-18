using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum RacingEnemyType
{
    Pipe,
    Car
}

[GenerateAuthoringComponent]
public struct RacingEnemy : IComponentData {

    public RacingEnemyType enemyType;

}