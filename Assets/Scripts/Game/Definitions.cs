using System.Collections.Generic;
using UnityEngine;

public static class Definitions
{
    public static void Initialize(ObjectData objectData) => Definitions.objectData = objectData;

    public static HeroDefinition Hero( Heroes hero, Assets.ObjectDataSets set = Assets.ObjectDataSets.Default) => objectData.heroSets[ (int) set ].heroDefinitions[ ( int ) hero ];

    public static EnemyDefinition Enemy( Enemies enemy, Assets.ObjectDataSets set = Assets.ObjectDataSets.Default) => objectData.enemySets[ (int) set ].enemyDefinitions[ ( int ) enemy ];

    public static ItemDefinition Item( Items item, Assets.ObjectDataSets set = Assets.ObjectDataSets.Default) => objectData.itemSets[ (int) set].itemDefinitions[ ( int ) item ];

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

    private static ObjectData objectData { get; set; }

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