using System.Collections.Generic;
using UnityEngine;

public class MissionSet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => missionDefinitions.Add( toAdd as MissionDefinition );
    public override void Remove( ScriptableObject toRemove ) => missionDefinitions.Remove( toRemove as MissionDefinition );

    public List<MissionDefinition> missionDefinitions = new List<MissionDefinition>();
}