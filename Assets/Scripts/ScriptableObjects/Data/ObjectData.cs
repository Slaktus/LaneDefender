using System.Collections.Generic;
using UnityEngine;

public class ObjectData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd )
    {
        if ( toAdd is ItemSet)
            itemSets.Add( toAdd as ItemSet);
        else if ( toAdd is HeroSet)
            heroSets.Add( toAdd as HeroSet);
        else if (toAdd is EnemySet)
            enemySets.Add(toAdd as EnemySet);
    }

    public override void Remove( ScriptableObject toRemove )
    {
        if (toRemove is ItemSet)
            itemSets.Remove(toRemove as ItemSet);
        else if (toRemove is HeroSet)
            heroSets.Remove(toRemove as HeroSet);
        else if (toRemove is EnemySet)
            enemySets.Remove(toRemove as EnemySet);
    }

    public List<ItemSet> itemSets = new List<ItemSet>();
    public List<HeroSet> heroSets = new List<HeroSet>();
    public List<EnemySet> enemySets = new List<EnemySet>();
}
