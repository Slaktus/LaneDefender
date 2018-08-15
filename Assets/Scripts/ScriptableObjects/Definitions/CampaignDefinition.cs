using System.Collections.Generic;
using UnityEngine;

public class CampaignDefinition : DefinitionBase
{
    public static CampaignDefinition Default() => CreateInstance<CampaignDefinition>().Initialize( 20 , 15 , 5 , 5 );

    public CampaignDefinition Initialize( float width , float height , int columns , int rows )
    {
        this.rows = rows;
        this.width = width;
        this.height = height;
        this.columns = columns;
        return this;
    }

    public bool Has( int index ) => missionIndices.Contains( index );
    public MissionDefinition Get( int index ) => Has( index ) ? missionDefinitions[ missionIndices.IndexOf( index ) ] : null;

    public void Add( ScriptableObject toAdd , int index )
    {
        Add( toAdd as MissionDefinition );
        missionIndices.Add( index );
    }

    public override void Add( ScriptableObject toAdd ) => missionDefinitions.Add( toAdd as MissionDefinition );

    public override void Remove( ScriptableObject toRemove )
    {
        MissionDefinition missionDefinition = toRemove as MissionDefinition;
        missionIndices.RemoveAt( missionDefinitions.IndexOf( missionDefinition ) );
        missionDefinitions.Remove( missionDefinition );
    }

    public List<MissionDefinition> missionDefinitions = new List<MissionDefinition>();
    public List<int> missionIndices = new List<int>();
    public float height;
    public float width;
    public int columns;
    public int rows;
}
