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
    public MissionDefinition GetMissionDefinition( int index ) => Has( index ) ? missionDefinitions[ missionIndices.IndexOf( index ) ] : null;
    public int GetMissionIndex(MissionDefinition missionDefinition ) => missionIndices[ missionDefinitions.IndexOf(missionDefinition) ];
    public Connection GetConnection(int index) => connections[ index ];

    public void Add( ScriptableObject toAdd , int index )
    {
        Add( toAdd as MissionDefinition );
        missionIndices.Add( index );
    }

    public override void Add(ScriptableObject toAdd)
    {
        if (toAdd is MissionDefinition)
            missionDefinitions.Add(toAdd as MissionDefinition);
        else
            connections.Add(toAdd as Connection);
    }

    public void RemoveMissionDefinitionAt(int index)
    {

        missionDefinitions.RemoveAt(missionIndices.IndexOf(index));
        missionIndices.Remove(index);
    }

    public override void Remove( ScriptableObject toRemove )
    {
        if ( toRemove is MissionDefinition)
        {
            MissionDefinition missionDefinition = toRemove as MissionDefinition;
            missionIndices.RemoveAt(missionDefinitions.IndexOf(missionDefinition));
            missionDefinitions.Remove(missionDefinition);
        }
        else
        {
            Connection connection = toRemove as Connection;
            connections.Remove(connection);
        }
    }

    public void SetFirstMissionIndex(int firstMissionIndex) => this.firstMissionIndex = firstMissionIndex;
    public void AddFinalMissionIndex(int finalMissionIndex) => finalMissionIndices.Add(finalMissionIndex);
    public bool HasFinalMissionIndex(int finalMissionIndex) => finalMissionIndices.Contains(finalMissionIndex);
    public void RemoveFinalMissionIndex(int finalMissionIndex) => finalMissionIndices.Remove(finalMissionIndex);

    public List<MissionDefinition> missionDefinitions = new List<MissionDefinition>();
    public List<Connection> connections = new List<Connection>();
    public List<int> missionIndices = new List<int>();
    public List<int> finalMissionIndices = new List<int>();
    public int firstMissionIndex;
    public float height;
    public float width;
    public int columns;
    public int rows;
}
