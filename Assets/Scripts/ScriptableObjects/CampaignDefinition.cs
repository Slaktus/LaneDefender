using System.Collections.Generic;
using UnityEngine;

public class CampaignDefinition : DefinitionBase
{
    public CampaignDefinition Initialize( float width , float height , int columns , int rows )
    {
        this.rows = rows;
        this.width = width;
        this.height = height;
        this.columns = columns;
        return this;
    }

    public override void Add( ScriptableObject toAdd ) => missionDefinitions.Add( toAdd as MissionDefinition );
    public override void Remove( ScriptableObject toRemove ) => missionDefinitions.Remove( toRemove as MissionDefinition );

    public List<MissionDefinition> missionDefinitions = new List<MissionDefinition>();
    public float height;
    public float width;
    public int columns;
    public int rows;
}
