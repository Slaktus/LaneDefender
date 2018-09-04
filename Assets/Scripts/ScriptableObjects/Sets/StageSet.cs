using System.Collections.Generic;
using UnityEngine;

public class StageSet : DefinitionBase
{
    public StageDefinition GetStage(int index) => stageDefinitions[ index ];
    public override void Add( ScriptableObject toAdd ) => stageDefinitions.Add( toAdd as StageDefinition );
    public override void Remove( ScriptableObject toRemove ) => stageDefinitions.Remove( toRemove as StageDefinition );

    public List<StageDefinition> stageDefinitions = new List<StageDefinition>();
}
