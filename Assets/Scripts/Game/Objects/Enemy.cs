using System.Collections;
using UnityEngine;

public class Enemy : LaneEntity
{
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }

    public Enemy( EnemyDefinition definition , int level , EnemySettings settings , Lane lane , float entryPoint = 0 , GameObject parent = null ) : base( definition.name , settings.speed , definition.width , definition.laneHeightPadding , settings.health , definition.Value(level) , lane )
    {
        color = settings.color;
        enter = Enter( entryPoint * lane.width );

        if ( parent != null )
            container.transform.SetParent( parent.transform );
    }
}

public class EnemySettings
{
    public Color color { get; }
    public float speed { get; }
    public int health { get; }

    public EnemySettings( Color color , int health , float speed )
    {
        this.color = color;
        this.speed = speed;
        this.health = health;
    }
}