using System.Collections.Generic;

public class CampaignMap
{
    public bool Has( int index ) => missions.Count > index && missions[ index ] != null;
    public void Add( Mission mission ) => missions.Add( mission );
    public void Add( int index , MissionDefinition missionDefinition ) => missions.Add( new Mission( index , missionDefinition ) );
    public void Replace( int index , Mission mission ) => missions[ index ] = mission;
    public void Remove( Mission mission ) => missions.Add( mission );

    public TileMap tileMap { get; }
    public List<Mission> missions { get; }

    public CampaignMap( float width , float height , int columns , int rows )
    {
        tileMap = new TileMap( width , height , columns , rows );
        missions = new List<Mission>();
    }
}