using System.Collections.Generic;
using UnityEngine;

public class CampaignData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd )
    {
        if ( toAdd is MissionSet )
            missionSets.Add( toAdd as MissionSet );
        else if ( toAdd is CampaignSet )
            campaignSets.Add( toAdd as CampaignSet );
    }

    public override void Remove( ScriptableObject toRemove )
    {
        if ( toRemove is MissionSet )
            missionSets.Remove( toRemove as MissionSet );
        else if ( toRemove is CampaignSet )
            campaignSets.Remove( toRemove as CampaignSet );
    }

    public List<MissionSet> missionSets = new List<MissionSet>();
    public List<CampaignSet> campaignSets = new List<CampaignSet>();
}
