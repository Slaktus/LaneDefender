using System.Collections.Generic;
using UnityEngine;

public class CampaignSet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => missionDefinitions.Add( toAdd as CampaignDefinition );
    public override void Remove( ScriptableObject toRemove ) => missionDefinitions.Remove( toRemove as CampaignDefinition );

    public List<CampaignDefinition> missionDefinitions = new List<CampaignDefinition>();
}