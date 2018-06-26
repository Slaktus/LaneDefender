#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CampaignEditor
{
    public void Load() => _campaignData = AssetDatabase.LoadAssetAtPath<CampaignData>( _campaignDataPath + "CampaignData.asset" );
    private void Create() => _campaignData = ScriptableObjects.Create<CampaignData>( _campaignDataPath + "CampaignData.asset" );

    CampaignData _campaignData;
    MissionSet _selectedMissionSet;
    MissionDefinition _selectedMission;
    private const string _campaignDataPath = "Assets/AssetBundleSource/Campaigns/";

    public void Update()
    {
        for ( int i = 0 ; _dropdowns.Count > i ; i++ )
            _dropdowns[ i ].Update();

        _missions?.Update();
        _missionSets?.Update();
    }

    public void ShowMissionSets( int index , Vector3 position )
    {
        _missionSets?.Destroy();
        int count = _campaignData.missionSets.Count + 1;
        _missionSets = new Layout( "MissionSets" , 4 , count , 0.25f , 0.1f , count , _container );
        _missionSets.SetPosition( position + ( Vector3.right * _missionSets.width * 0.5f ) + ( Vector3.back * _missionSets.height * 0.5f ) );
        _dropdowns[ index ].AddLayout( _missionSets );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewMissionSet" , "New Set" , 4 , 1 , _container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideMissionSets();
                        ScriptableObjects.Add( ScriptableObject.CreateInstance<MissionSet>() , _campaignData );
                        ShowMissionSets( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "MissionSet" , "Mission Set" , 4 , 1 , _container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        _selectedMissionSet = _campaignData.missionSets[ capturedIndex ];
                        ShowMissions( index , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                        _dropdowns[ index ].AddLayout( _missionSets );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        _missionSets.Add( buttons );
        _missionSets.Refresh();
    }

    public void HideMissionSets()
    {
        for ( int i = 0 ; _dropdowns.Count > i ; i++ )
            _dropdowns[ i ].RemoveLayout( _missionSets );

        _missionSets?.Destroy();
        _missionSets = null;
    }

    public void ShowMissions( int index , Vector3 position )
    {
        _missions?.Destroy();
        int count = _selectedMissionSet.missionDefinitions.Count + 1;
        _missions = new Layout( "MissionLayout" , 4 , count , 0.25f , 0.1f , count , _container );
        _missions.SetPosition( position + ( Vector3.right * _missions.width * 0.5f ) + ( Vector3.back * _missions.height * 0.5f ) );
        _dropdowns[ index ].AddLayout( _missions );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewMission" , "New Mission" , 4 , 1 , _container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) => 
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideMissions();
                        MissionDefinition missionDefinition = ScriptableObject.CreateInstance<MissionDefinition>();
                        missionDefinition.Initialize( "MissionDefinition" , 120 );
                        ScriptableObjects.Add( missionDefinition , _selectedMissionSet );
                        //_campaignMap.Add( index , missionDefinition );
                        ShowMissions( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "Mission" , "Mission" , 4 , 1 , _container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        _selectedMission = _selectedMissionSet.missionDefinitions[ capturedIndex ];
                        HideMissionSets();
                        HideMissions();
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        _missions.Add( buttons );
        _missions.Refresh();
    }

    public void HideMissions()
    {
        for ( int i = 0 ; _dropdowns.Count > i ; i++ )
            _dropdowns[ i ].RemoveLayout( _missions );

        _missions?.Destroy();
        _missions = null;
    }

    public void ShowCampaignMap()
    {
        for ( int i = 0 ; _campaignMap.tileMap.count > i ; i++ )
        {
            int index = i;
            Dropdown dropdown = new Dropdown( "CampaignMap" + index , "" , 1 , 1 , _container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) && button.containsMouse && !( button as Dropdown ).HasLayout( _missions ) )
                        ShowMissionSets( index , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ,
                Close: ( Button button ) =>
                {
                    if ( ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) ) && ( ( button as Dropdown ).HasLayout( _missions ) || ( button as Dropdown ).HasLayout( _missionSets ) ) && ( _missions == null || !_missions.containsMouse ) )
                    {
                        HideMissions();
                        HideMissionSets();
                    }
                } );

            dropdown.SetPosition( _campaignMap.tileMap.PositionOf( index ) );
            _dropdowns.Add( dropdown );
        }
    }

    public void HideCampaignMap()
    {
        for ( int i = 0 ; _dropdowns.Count > i ; i++ )
            _dropdowns[ i ].Destroy();

        _dropdowns.Clear();
    }

    public void Hide()
    {
        HideCampaignMap();
        HideMissionSets();
        HideMissions();
    }

    public void Add( Mission mission ) => _campaignMap.Add( mission );
    public void Add( int index , MissionDefinition missionDefinition ) => _campaignMap.Add( index , missionDefinition );
    public void Replace( int index , Mission mission ) => _campaignMap.Replace( index , mission );

    private List<Dropdown> _dropdowns { get; }
    private Layout _missions { get; set; }
    private CampaignMap _campaignMap { get; }
    private Layout _missionSets { get; set; }
    private GameObject _container { get; }

    public CampaignEditor()
    {
        Load();

        if ( _campaignData == null )
            Create();

        _campaignMap = new CampaignMap( 20 , 15 , 5 , 5 );
        _container = new GameObject( "CampaignEditor" );
        _dropdowns = new List<Dropdown>();
        ShowCampaignMap();
    }
}

public class Mission
{
    public WaveSet Next( float time ) => waveSets.Count > 0 && waveSets[ 0 ].time > time ? waveSets[ 0 ] : null;
    public void SetDuration( float duration ) => _missionDefinition.SetDuration( duration );
    public void AddWaveSet( WaveSet waveSet ) => waveSets.Add( waveSet );
    public void SetIndex( int index ) => this.index = index;

    public List<WaveSet> waveSets => _missionDefinition.waveSets;
    public float duration => _missionDefinition.duration;
    public int index { get; private set; }

    private MissionDefinition _missionDefinition { get; }

    public Mission ( int index , MissionDefinition missionDefinition )
    {
        this.index = index;
        _missionDefinition = missionDefinition;
    }
}

public class CampaignMap
{
    public bool Has( int index ) => missions.Count > index && missions[ index ] != null;
    public void Add( Mission mission ) => missions.Add( mission );
    public void Add( int index , MissionDefinition missionDefinition ) => missions.Add( new Mission( index , missionDefinition ) );
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
#endif //UNITY_EDITOR