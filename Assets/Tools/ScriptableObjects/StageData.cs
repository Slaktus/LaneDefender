using System.Collections.Generic;
using UnityEngine;

public class StageData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => stageDefinitions.Add( toAdd as StageSet );
    public override void Remove( ScriptableObject toRemove ) => stageDefinitions.Remove( toRemove as StageSet );

    public List<StageSet> stageDefinitions = new List<StageSet>();
}