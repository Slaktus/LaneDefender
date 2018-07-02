#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Editor
{
    public void Update()
    {
        HandleLaneHover();

        for ( int i = 0 ; _campaignMapDropdowns.Count > i ; i++ )
            _campaignMapDropdowns[ i ].Update();

        campaignEditor.Update();
        missionEditor.Update();
        stageEditor.Update();
        waveEditor.Update();
        stage?.Update();
    }

    private void HandleLaneHover()
    {
        if ( stage != null && waveEditor.selectedWaveDefinition != null )
        {
            Lane hoveredLane = stage.GetHoveredLane( mousePosition );
            stage.SetLaneColor( Color.black );

            if ( hoveredLane != null )
            {
                hoveredLane.color = Color.yellow;

                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    int index = stage.IndexOf( hoveredLane );
                    WaveEventDefinition waveEventDefinition = ScriptableObject.CreateInstance<WaveEventDefinition>();
                    waveEventDefinition.Initialize( 0 , index , WaveEvent.Type.SpawnEnemy );
                    ScriptableObjects.Add( waveEventDefinition , waveEditor.selectedWaveDefinition );
                    waveEditor.HideWaveEventButtons();
                    waveEditor.ShowWaveEventButtons();
                }
            }

            if ( waveEditor.heldWaveEvent != null )
            {
                waveEditor.heldWaveEvent.SetPosition( mousePosition );

                if ( Input.GetMouseButtonUp( 0 ) )
                {
                    if ( hoveredLane != null )
                    {
                        waveEditor.heldWaveEvent.waveEventDefinition.SetLane( stage.IndexOf( hoveredLane ) );
                        waveEditor.HideWaveEventButtons();
                        waveEditor.ShowWaveEventButtons();
                    }

                    waveEditor.heldWaveEvent.Destroy();
                    waveEditor.heldWaveEvent = null;
                }
            }
        }
    }

    public void ShowStage( StageDefinition stageDefinition ) => stage = new Stage( stageDefinition , null , new Player() );

    public void HideStage() => stage?.Destroy();

    public void ShowCampaignMap()
    {
        HideCampaignMap();
        campaignMap = new CampaignMap( campaignEditor.selectedCampaign.width , campaignEditor.selectedCampaign.height , campaignEditor.selectedCampaign.columns , campaignEditor.selectedCampaign.rows );

        for ( int i = 0 ; campaignMap.tileMap.count > i ; i++ )
        {
            int index = i;
            Dropdown dropdown = new Dropdown( "CampaignMap" + index , index.ToString() , campaignMap.tileMap.tileWidth - 1 , campaignMap.tileMap.tileHeight * 0.5f , campaignEditor.container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) && button.containsMouse )
                    {
                        if ( missionEditor.selectedMission != null && campaignEditor.selectedCampaign.Get( index ) == missionEditor.selectedMission )
                        {
                            //actually, we probably want to do something else here
                            //we really want to be able to set the stage here, at least
                            ShowStage( missionEditor.selectedMission.stageDefinition );
                            missionEditor.ShowMissionTimeline();
                            stageEditor.Show();

                            campaignEditor.Hide();
                            missionEditor.Hide();
                            HideCampaignMap();
                        }
                        else if ( !( button as Dropdown ).HasLayout( missionEditor.missions ) )
                            missionEditor.ShowMissionSets( index , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ,
                Close: ( Button button ) =>
                {
                    Dropdown d = button as Dropdown;

                    if ( ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) ) && ( d.HasLayout( missionEditor.missions ) || d.HasLayout( missionEditor.missionSets ) ) )
                    {
                        missionEditor.HideMissions();
                        missionEditor.HideMissionSets();
                        ( button as Dropdown ).RemoveLayouts();
                    }
                } );

            dropdown.SetPosition( campaignMap.tileMap.PositionOf( index ) );
            _campaignMapDropdowns.Add( dropdown );
        }
    }

    public void HideCampaignMap()
    {
        for ( int i = 0 ; _campaignMapDropdowns.Count > i ; i++ )
            _campaignMapDropdowns[ i ].Destroy();

        _campaignMapDropdowns.Clear();
    }

    public void Refresh()
    {
        ShowCampaignMap();
    }

    public Dropdown GetDropdown( int index ) => _campaignMapDropdowns[ index ];

    public T Load<T>( string path ) where T : ScriptableObject => AssetDatabase.LoadAssetAtPath<T>( path + typeof( T ) + ".asset" );
    private T Create<T>( string path ) where T : ScriptableObject => ScriptableObjects.Create<T>( path + typeof( T ) + ".asset" );

    public Vector3 mousePosition => Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );
    public CampaignMap campaignMap { get; private set; }
    public Stage stage { get; private set; }
    public GameObject container { get; }

    public CampaignData campaignData { get; }
    public StageData stageData { get; }
    public WaveData waveData { get; }

    public CampaignEditor campaignEditor { get; }
    public MissionEditor missionEditor { get; }
    public StageEditor stageEditor { get; }
    public WaveEditor waveEditor { get; }

    private List<Dropdown> _campaignMapDropdowns { get; }
    private Button _testButton { get; }
    private Button _saveButton { get; }
    private Level _level { get; set; }


    private const string _campaignDataPath = "Assets/AssetBundleSource/Campaigns/";
    private const string _stageDataPath = "Assets/AssetBundleSource/Stages/";
    private const string _waveDataPath = "Assets/AssetBundleSource/Waves/";

    public Editor()
    {
        waveData = Load<WaveData>( _waveDataPath );
        stageData = Load<StageData>( _stageDataPath );
        campaignData = Load<CampaignData>( _campaignDataPath );

        if ( waveData == null )
            waveData = Create<WaveData>( _waveDataPath );

        if ( stageData == null )
            stageData = Create<StageData>( _stageDataPath );

        if ( campaignData == null )
            campaignData = Create<CampaignData>( _campaignDataPath );

        _campaignMapDropdowns = new List<Dropdown>();
        container = new GameObject( "Editor" );

        _testButton = new Button( "Test" , "Test" , 1.5f , 0.5f , container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( waveEditor.selectedWaveDefinition != null ? Color.green : button.color ) ,
            Stay: ( Button button ) =>
            {
                if ( waveEditor.selectedWaveDefinition != null && Input.GetMouseButtonDown( 0 ) )
                {
                    if ( _level == null )
                    {
                        _level = new Level( 10 , showProgress: false );
                        Wave wave = new Wave( 1 , stage );

                        for ( int i = 0 ; waveEditor.selectedWaveDefinition.waveEvents.Count > i ; i++ )
                        {
                            switch ( ( WaveEvent.Type ) waveEditor.selectedWaveDefinition.waveEvents[ i ].type )
                            {
                                case WaveEvent.Type.SpawnEnemy:
                                    wave.Add( new SpawnEnemyEvent( Definitions.Enemy( Definitions.Enemies.Default ) , waveEditor.selectedWaveDefinition.waveEvents[ i ] ) );
                                    break;
                            }
                        }

                        _level.Add( wave );
                        button.SetLabel( "Stop" );
                    }
                    else
                    {
                        button.SetLabel( "Test" );
                        stage.ClearLanes();
                        _level.DestroyProgress();
                        _level = null;
                    }
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) );

        _testButton.SetViewportPosition( new Vector2( 1 , 1 ) );
        _testButton.SetPosition( _testButton.position + Vector3.left * _testButton.width );

        _saveButton = new Button( "Save" , "Save" , 1.5f , 0.5f , container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                    ScriptableObjects.Save();
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) );

        _saveButton.SetPosition( _testButton.position + Vector3.left * ( _saveButton.width ) );


        campaignEditor = new CampaignEditor( this , Vector3.zero );
        missionEditor = new MissionEditor( this );
        stageEditor = new StageEditor( this );
        waveEditor = new WaveEditor( this );
    }
}

#endif //UNITY_EDITOR