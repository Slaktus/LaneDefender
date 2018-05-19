﻿using System.Collections.Generic;
using UnityEngine;

public static class Definitions
{
    public static EnemyDefinition Enemy( Enemies enemy ) => enemyDefinitions[ ( int ) enemy ];

    private static List<EnemyDefinition> enemyDefinitions { get; }

    static Definitions()
    {
        enemyDefinitions = new List<EnemyDefinition>( ( int ) Enemies.Count )
        {
            new EnemyDefinition( "Enemy" , Color.white , 5 , 1 , 10 , 3 , 6 )
        };
    }

    public enum Enemies
    {
        Default = 0,
        Count = 1
    }
}

public class EnemyDefinition : EntityDefinition
{
    public Color color { get; }
    public float speed { get; }
    public int health { get; }

    public EnemyDefinition( string name , Color color , float width , float laneHeightPadding , float speed , int health , int value ) : base( name , width , laneHeightPadding , value )
    {
        this.color = color;
        this.speed = speed;
        this.health = health;
    }
}

public abstract class EntityDefinition
{
    public int value { get; }
    public string name { get; }
    public float width { get; }
    public float laneHeightPadding { get; }

    public EntityDefinition( string name , float width , float laneHeightPadding , int value )
    {
        this.name = name;
        this.width = width;
        this.value = value;
        this.laneHeightPadding = laneHeightPadding;
    }
}