using System.Collections.Generic;
using UnityEngine;

public class EnemySet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => enemyDefinitions.Add( toAdd as EnemyDefinition );
    public override void Remove( ScriptableObject toRemove ) => enemyDefinitions.Remove( toRemove as EnemyDefinition);

    public List<EnemyDefinition> enemyDefinitions = new List<EnemyDefinition>();
}