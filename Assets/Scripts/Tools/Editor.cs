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

        for ( int i = 0 ; _campaignMapButtons.Count > i ; i++ )
            _campaignMapButtons[ i ].Update();

        _level?.Update();
        stage?.Update();
        _saveButton.Update();
        _testButton.Update();
        campaignEditor.Update();
        waveEditor.Update();
        missionEditor.Update();
        stageEditor.Update();
    }

    private void HandleLaneHover()
    {
        if ( stage != null && waveEditor.selectedWaveDefinition != null && waveEditor.selectedWaveSet == null )
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
            Button button = new Button( "CampaignMap" + index , campaignEditor.selectedCampaign.Has( index ) ? campaignEditor.selectedCampaign.Get( index ).name : index.ToString() , campaignMap.tileMap.tileWidth - 1 , campaignMap.tileMap.tileHeight * 0.5f , campaignEditor.container ,
                Enter: ( Button b ) => b.SetColor( b.selected ? b.color : Color.green ) ,
                Stay: ( Button b ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        if ( campaignEditor.selectedCampaign.Has( index ) )
                        {
                            missionEditor.SetSelectedMission( campaignEditor.selectedCampaign.Get( index ) );

                            if ( missionEditor.selectedMission.stageDefinition != null )
                            {
                                stageEditor.SetSelectedStageDefinition( missionEditor.selectedMission.stageDefinition );
                                ShowStage( missionEditor.selectedMission.stageDefinition );
                            }

                            stageEditor.Show();
                            missionEditor.ShowMissionEditor();
                            missionEditor.ShowMissionTimeline();

                            campaignEditor.Hide();
                            missionEditor.Hide();
                            HideCampaignMap();
                        }
                        else
                        {
                            missionEditor.ShowMissionSets( index , b.position + new Vector3( b.width * 0.5f , 0 , b.height * 0.5f ) );
                            b.SetColor( Color.yellow );
                            b.Select();
                        }
                    }
                } ,
                Exit: ( Button b ) => b.SetColor( b.selected ? b.color : Color.white ) ,
                Close: ( Button b ) =>
                {
                    if ( b.selected && ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) ) && ( missionEditor.missionSets == null || !missionEditor.missionSets.containsMouse ) && ( missionEditor.missions == null || !missionEditor.missions.containsMouse ) )
                    {
                        b.Deselect();
                        b.SetColor( Color.white );
                        missionEditor.HideMissions();
                        missionEditor.HideMissionSets();
                    }
                } );

            button.SetPosition( campaignMap.tileMap.PositionOf( index ) );
            _campaignMapButtons.Add( button );
        }
    }

    public void HideCampaignMap()
    {
        for ( int i = 0 ; _campaignMapButtons.Count > i ; i++ )
            _campaignMapButtons[ i ].Destroy();

        _campaignMapButtons.Clear();
    }

    public void Refresh()
    {
        ShowCampaignMap();
    }

    public Button GetMapButton( int index ) => _campaignMapButtons[ index ];

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

    private List<Button> _campaignMapButtons { get; }
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

        _campaignMapButtons = new List<Button>();
        container = new GameObject( "Editor" );

        _testButton = new Button( "Test" , "Test" , 1.5f , 0.5f , container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( stage != null && missionEditor.selectedMission != null ? Color.green : button.color ) ,
            Stay: ( Button button ) =>
            {
                if ( stage != null && missionEditor.selectedMission != null && Input.GetMouseButtonDown( 0 ) )
                {
                    if ( _level == null )
                    {
                        _level = new Level( missionEditor.selectedMission.duration , showProgress: false );

                        for ( int i = 0 ; missionEditor.selectedMission.waveDefinitions.Count > i ; i++ )
                        {
                            Wave wave = new Wave( missionEditor.selectedMission.waveTimes[ i ] * missionEditor.selectedMission.duration , stage );
                            _level.Add( wave );

                            for ( int j = 0 ; missionEditor.selectedMission.waveDefinitions[ i ].waveEvents.Count > j ; j++ )
                                switch ( ( WaveEvent.Type ) missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ].type )
                                {
                                    case WaveEvent.Type.SpawnEnemy:
                                        wave.Add( new SpawnEnemyEvent( Definitions.Enemy( Definitions.Enemies.Default ) , missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ] ) );
                                        break;
                                }
                        }

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

        campaignEditor.ShowCampaignSets();
        campaignEditor.ShowCampaignEditor();
    }
}

#endif //UNITY_EDITOR