using UnityEngine;

public class Enemy : LaneEntity
{
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }

    public Enemy( EnemyDefinition enemyDefinition , Lane lane ) : base( enemyDefinition.name , enemyDefinition.speed , enemyDefinition.width , enemyDefinition.laneHeightPadding , enemyDefinition.health , enemyDefinition.value , lane )
    {
        color = enemyDefinition.color;
    }
}