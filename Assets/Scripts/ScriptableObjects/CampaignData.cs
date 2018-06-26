using System.Collections.Generic;
using UnityEngine;

public class CampaignData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => missionSets.Add( toAdd as MissionSet );
    public override void Remove( ScriptableObject toRemove ) => missionSets.Remove( toRemove as MissionSet );

    public List<MissionSet> missionSets = new List<MissionSet>();
}
