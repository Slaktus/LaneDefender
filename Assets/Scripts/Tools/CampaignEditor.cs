using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignEditor
{
    public void Update()
    {
        for ( int i = 0 ; _dropdowns.Count > i ; i++ )
            _dropdowns[ i ].Update();

        _missionList?.Update();
    }

    public void ShowMissionList( Vector3 position )
    {
        _missionList?.Destroy();
        _missionList = new Layout( "MissionLayout" , 4 , 5 , 0.25f , 0.1f , 5 , _container );
        _missionList.SetPosition( position + ( Vector3.right * _missionList.width * 0.5f ) + ( Vector3.back * _missionList.height * 0.5f ) );

        List<Button> buttons = new List<Button>( 5 )
        {
            new Button( "NewMission" , "New Mission" , 4 , 1 , _container ,
                Enter: ( Button button ) => { } ,
                Stay: ( Button button ) => { } ,
                Exit: ( Button button ) => { } )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
            buttons.Add( new Button( "Mission" , "Mission" , 4 , 1 , _container ) );

        _missionList.Add( buttons );
        _missionList.Refresh();
    }

    public void HideMissionList()
    {
        _missionList?.Destroy();
        _missionList = null;
    }

    public void Add( Mission mission ) => _campaignMap.Add( mission );
    public void Add( int index , float duration = 120 ) => _campaignMap.Add( index , duration );
    public void Replace( int index , Mission mission ) => _campaignMap.Replace( index , mission );

    private List<Dropdown> _dropdowns { get; }
    private Layout _missionList { get; set; }
    private CampaignMap _campaignMap { get; }
    private GameObject _container { get; }

    public CampaignEditor()
    {
        _campaignMap = new CampaignMap( 20 , 15 , 5 , 5 );
        _container = new GameObject( "CampaignEditor" );
        _dropdowns = new List<Dropdown>();
        
        for ( int i = 0 ; _campaignMap.tileMap.count > i ; i++ )
        {
            Dropdown dropdown = new Dropdown( "CampaignMap" + i , "" , 1 , 1 , _container , 
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) && button.containsMouse && !( button as Dropdown ).HasLayout( _missionList ) )
                    {
                        ShowMissionList( button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                        ( button as Dropdown ).AddLayout( _missionList );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) , 
                Close: ( Button button ) => 
                {
                    if ( ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) ) && ( button as Dropdown ).HasLayout( _missionList ) && ( _missionList == null || !_missionList.containsMouse ) )
                    {
                        ( button as Dropdown ).RemoveLayout( _missionList );
                        HideMissionList();
                    }
                } );

            dropdown.SetPosition( _campaignMap.tileMap.PositionOf( i ) );
            _dropdowns.Add( dropdown );
        }
    }
}

public class Mission
{
    public WaveSet Next( float time ) => _waveSets.Count > 0 && _waveSets[ 0 ].time > time ? _waveSets[ 0 ] : null;
    public void AddWaveSet( WaveSet waveSet ) => _waveSets.Add( waveSet );
    public void SetDuration( float duration ) => this.duration = duration;
    public void SetIndex( int index ) => this.index = index;

    public float duration { get; private set; }
    public int index { get; private set; }

    private List<WaveSet> _waveSets { get; }

    public Mission ( int index , float duration , List<WaveSet> waveSets = null )
    {
        this.index = index;
        this.duration = duration;
        _waveSets = waveSets != null ? waveSets : new List<WaveSet>();
    }
}

public class CampaignMap
{
    public void Add( Mission mission ) => missions.Add( mission );
    public void Add( int index , float duration ) => missions.Add( new Mission( index , duration ) );
    public void Replace( int index , Mission mission ) => missions[ index ] = mission;
    public void Remove( Mission mission ) => missions.Add( mission );

    public TileMap tileMap { get; }
    public List<Mission> missions { get; }

    public CampaignMap ( float width , float height , int columns , int rows )
    {
        tileMap = new TileMap( width , height , columns , rows );
        missions = new List<Mission>();
    }
}

public class TileMap
{
    public int XOf( Tile tile ) => tile.index % columns;
    public int YOf( Tile tile ) => Mathf.FloorToInt( tile.index / columns );
    public Vector3 PositionOf( Tile tile ) => PositionOf( _tiles.IndexOf( tile ) );
    public Vector3 PositionOf( int index ) => _tiles[ index ].position;
    public int IndexOf( Tile tile ) => _tiles.IndexOf( tile );
    public Tile TileAt( int index ) => _tiles[ index ];

    public int count => _tiles.Count;
    public float height { get; }
    public float width { get; }
    public int columns { get; }
    public int rows { get; }

    private List<Tile> _tiles { get; }

    public TileMap( float width , float height , int columns , int rows )
    {
        this.rows = rows;
        this.width = width;
        this.height = height;
        this.columns = columns;
        _tiles = new List<Tile>( columns * rows );

        for ( int i = 0 ; _tiles.Capacity > i ; i++ )
            _tiles.Add( new Tile( this ) );
    }
}

public class Tile
{
    public Vector2 planarPosition => new Vector2( x * width , -y * height );
    public Vector3 position => new Vector3( x * width , 0 , -y * height );
    public float width => map.width / map.columns;
    public float height => map.height / map.rows;
    public int index => map.IndexOf( this );
    public int x => map.XOf( this );
    public int y => map.YOf( this );

    private TileMap map { get; }

    public Tile( TileMap map )
    {
        this.map = map;
    }
}