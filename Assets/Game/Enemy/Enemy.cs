using UnityEngine;

public class Enemy : LaneEntity
{
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }

    public Enemy( EnemyDefinition definition , EnemySettings settings , Lane lane ) : base( definition.name , settings.speed , definition.width , definition.laneHeightPadding , settings.health , definition.value , lane )
    {
        color = settings.color;
    }
}