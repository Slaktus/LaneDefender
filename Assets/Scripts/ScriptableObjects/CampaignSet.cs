using System.Collections.Generic;
using UnityEngine;

public class CampaignSet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => campaignDefinitions.Add( toAdd as CampaignDefinition );
    public override void Remove( ScriptableObject toRemove ) => campaignDefinitions.Remove( toRemove as CampaignDefinition );

    public List<CampaignDefinition> campaignDefinitions = new List<CampaignDefinition>();
}