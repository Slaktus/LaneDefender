using System.Collections.Generic;
using UnityEngine;

public static class Definitions
{
    public static HeroDefinition Hero( Heroes hero ) => heroDefinitions[ ( int ) hero ];

    public static EnemyDefinition Enemy( Enemies enemy ) => enemyDefinitions[ ( int ) enemy ];

    public static ItemDefinition Item( Items item ) => itemDefinitions[ ( int ) item ];

    public static List<Effects> GetEffects(Items item)
    {
        switch (item)
        {
            case Items.Damage:
                return new List<Effects>() { Effects.Damage, Effects.PushBack };

            case Items.LaneDown:
                return new List<Effects>() { Effects.LaneDown };

            case Items.LaneUp:
                return new List<Effects>() { Effects.LaneUp };

            case Items.Leap:
                return new List<Effects>() { Effects.Leap, Effects.Damage, Effects.PushBack };

            case Items.Part:
                return new List<Effects>() { Effects.Damage, Effects.PushBack };

            case Items.Split:
                return new List<Effects>() { Effects.Damage, Effects.PushBack, Effects.Split };

            case Items.Wreck:
                return new List<Effects>() { Effects.Leap, Effects.Damage, Effects.PushBack };

            default:
                return new List<Effects>() { };
        }
    }

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

        itemDefinitions = new List<ItemDefinition>((int) Items.Count)
        {
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.LaneUp, GetEffects(Items.LaneUp)),
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.LaneDown, GetEffects(Items.LaneDown)),
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.Damage, GetEffects(Items.Damage)),
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.Split, GetEffects(Items.Split)),
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.Leap, GetEffects(Items.Leap)),
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.Part, GetEffects(Items.Part)),
            ScriptableObject.CreateInstance<ItemDefinition>().Initialize("Item", 2, 1, Items.Wreck, GetEffects(Items.Wreck))
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