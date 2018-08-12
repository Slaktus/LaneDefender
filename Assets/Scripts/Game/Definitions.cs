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
            ScriptableObject.CreateInstance<EnemyDefinition>().Initialize( "Enemy" , 5 , 1 , 6 , Enemies.Default )
        };

        heroDefinitions = new List<HeroDefinition>( ( int ) Heroes.Count )
        {
            ScriptableObject.CreateInstance<HeroDefinition>().Initialize( "Hero" , 5 , 1 , Heroes.Default )
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

    public enum Effects
    {
        PushBack,
        LaneDown,
        LaneUp,
        Damage,
        Split,
        Leap,
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