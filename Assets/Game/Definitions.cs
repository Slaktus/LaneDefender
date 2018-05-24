using System.Collections.Generic;
using UnityEngine;

public static class Definitions
{
    public static HeroDefinition Hero( Heroes hero ) => heroDefinitions[ ( int ) hero ];

    public static EnemyDefinition Enemy( Enemies enemy ) => enemyDefinitions[ ( int ) enemy ];

    public static ItemDefinition Item( Items item ) => itemDefinitions[ ( int ) item ];

    private static List<HeroDefinition> heroDefinitions { get; }
    private static List<ItemDefinition> itemDefinitions { get; }
    private static List<EnemyDefinition> enemyDefinitions { get; }

    static Definitions()
    {
        enemyDefinitions = new List<EnemyDefinition>( ( int ) Enemies.Count )
        {
            new EnemyDefinition( "Enemy" , 5 , 1 , 6 )
        };

        heroDefinitions = new List<HeroDefinition>( ( int ) Heroes.Count )
        {
            new HeroDefinition( "Hero" , 5 , 1 )
        };

        itemDefinitions = new List<ItemDefinition>( ( int ) Items.Count )
        {
            new ItemDefinition( Items.LaneUp ) ,
            new ItemDefinition( Items.LaneDown ) ,
            new ItemDefinition( Items.Damage ) ,
            new ItemDefinition( Items.Split ) ,
            new ItemDefinition( Items.Leap ) ,
            new ItemDefinition( Items.Part ) ,
            new ItemDefinition( Items.Wreck ) ,
        };
    }

    public enum Heroes
    {
        Default = 0,
        Count = 1
    }

    public enum Enemies
    {
        Default = 0,
        Count = 1
    }

    public enum Items
    {
        LaneUp = 0,
        LaneDown = 1,
        Damage = 2,
        Split = 3,
        Leap = 4,
        Part = 5,
        Wreck = 6,
        Count = 7,
    }
}

public class ItemSettings
{
    public void SetLevel( int level ) => this.level = level;

    public int level { get; private set; }
    public int damage
    {
        get
        {
            switch ( level )
            {
                default:
                    return 1;
            }
        }
    }

    public ItemSettings ( int level  )
    {
        this.level = level;
    }
}

public class HeroSettings
{
    public Color color { get; }
    public int health { get; }

    public HeroSettings( Color color , int health )
    {
        this.color = color;
        this.health = health;
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

public class ItemDefinition
{
    public Definitions.Items type { get; }

    public ItemDefinition( Definitions.Items type )
    {
        this.type = type;
    }
}

public class HeroDefinition : EntityDefinition
{
    public Definitions.Heroes type { get; }

    public HeroDefinition( string name , float width , float laneHeightPadding  ) : base( name , width , laneHeightPadding , 0 )
    {
        type = Definitions.Heroes.Default;
    }
}

public class EnemyDefinition : EntityDefinition
{
    public Definitions.Enemies type { get; }

    public EnemyDefinition( string name , float width , float laneHeightPadding , int value ) : base( name , width , laneHeightPadding , value )
    {
        type = Definitions.Enemies.Default;
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