using System.Collections.Generic;
using UnityEngine;

public static class Definitions
{
    public static void Initialize(ObjectData objectData)
    {
        Definitions.objectData = objectData;
        initialized = true;
    }

    public static HeroDefinition Hero(Heroes hero, Assets.ObjectDataSets set = Assets.ObjectDataSets.Default) => objectData.heroSets[ (int) set ].heroDefinitions[ (int) hero ];

    public static EnemyDefinition Enemy(Enemies enemy, Assets.ObjectDataSets set = Assets.ObjectDataSets.Default) => objectData.enemySets[ (int) set ].enemyDefinitions[ (int) enemy ];

    public static ItemDefinition Item(Items item, Assets.ObjectDataSets set = Assets.ObjectDataSets.Default) => objectData.itemSets[ (int) set ].itemDefinitions[ (int) item ];

    public static bool initialized { get; private set; }
    private static ObjectData objectData { get; set; }

    public enum Heroes
    {
        Default = 0,
        Count = 1
    }

    public enum Enemies
    {
        Default = 0,
        Test = 1,
        Count = 2
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
        PushBack = 0,
        LaneDown = 1,
        LaneUp = 2,
        Damage = 3,
        Split = 4,
        Leap = 5,
        Count = 6
    }
}