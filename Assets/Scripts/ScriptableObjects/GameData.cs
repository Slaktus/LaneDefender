using System.Collections.Generic;
using UnityEngine;

public class GameData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd )
    {
        if ( toAdd is CampaignData )
            campaignData.Add( toAdd as CampaignData );
        else if ( toAdd is StageData )
            stageData.Add( toAdd as StageData );
        else if ( toAdd is WaveData )
            waveData.Add( toAdd as WaveData );
    }

    public override void Remove( ScriptableObject toRemove )
    {
        if ( toRemove is CampaignData )
            campaignData.Remove( toRemove as CampaignData );
        else if ( toRemove is StageData )
            stageData.Remove( toRemove as StageData );
        else if ( toRemove is WaveData )
            waveData.Remove( toRemove as WaveData );
    }

    public List<CampaignData> campaignData = new List<CampaignData>();
    public StageData stageData = new StageData();
    public WaveData waveData = new WaveData();
}
