using System.Collections.Generic;
using UnityEngine;

public class StageData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => stageSets.Add( toAdd as StageSet );
    public override void Remove( ScriptableObject toRemove ) => stageSets.Remove( toRemove as StageSet );

    public List<StageSet> stageSets = new List<StageSet>();
}