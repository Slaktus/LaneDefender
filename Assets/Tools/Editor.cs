using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor
{
    public void Update()
    {
        Ray mouseRay = _camera.ScreenPointToRay( Input.mousePosition );
        RaycastHit[] hits = Physics.RaycastAll( mouseRay.origin , mouseRay.direction , float.PositiveInfinity );
        Lane hoveredLane = null;

        if ( stage != null && _waveEditor.waveEventEditor == null && !_waveEditor.showingWaveSets && !_waveEditor.showingWaveDefinitions && _waveEditor.selectedWaveDefinition != null && hits.Length > 0 )
        {
            Vector3 mousePosition = hits[ 0 ].point;
            hoveredLane = stage.GetHoveredLane( mousePosition );
            stage.SetLaneColor( Color.black );

            if ( hoveredLane != null )
            {
                hoveredLane.color = Color.yellow;

                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    int index = stage.IndexOf( hoveredLane );
                    WaveEventDefinition waveEventDefinition = ScriptableObject.CreateInstance<WaveEventDefinition>();
                    waveEventDefinition.Initialize( 0 , index , WaveEvent.Type.SpawnEnemy );
                    ScriptableObjects.Add( waveEventDefinition , _waveEditor.selectedWaveDefinition );
                    _waveEditor.HideWaveEventButtons();
                    _waveEditor.ShowWaveEventButtons();
                }
            }

            if ( _waveEditor.heldWaveEvent != null )
            {
                _waveEditor.heldWaveEvent.SetPosition( mousePosition );

                if ( Input.GetMouseButtonUp( 0 ) )
                {
                    if ( hoveredLane != null )
                    {
                        _waveEditor.heldWaveEvent.waveEventDefinition.SetLane( stage.IndexOf( hoveredLane ) );
                        _waveEditor.HideWaveEventButtons();
                        _waveEditor.ShowWaveEventButtons();
                    }

                    _waveEditor.heldWaveEvent.Destroy();
                    _waveEditor.heldWaveEvent = null;
                }
            }
        }

        level?.Update();
        stage?.Update();
        _testButton?.Update();
        _saveButton?.Update();
        _waveEditor?.Update();
        _stageEditor?.Update();
    }
    
    public Stage stage => _stageEditor.stage;
    public Level level { get; private set; }

    private Camera _camera { get; }
    private Button _testButton { get; }
    private Button _saveButton { get; }
    private GameObject _container { get; }
    private WaveEditor _waveEditor { get; }
    private StageEditor _stageEditor { get; }

    public Editor()
    {
        _camera = Camera.main;
        _container = new GameObject( "Editor" );
        _waveEditor = new WaveEditor( this , _container );
        _stageEditor = new StageEditor( this , _container );

        _waveEditor.Show();

        _testButton = new Button( "Test" , "Test" , 1.5f , 0.5f , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( _waveEditor.selectedWaveDefinition != null ? Color.green : button.color ) ,
            Stay: ( Button button ) =>
            {
                if ( _waveEditor.selectedWaveDefinition != null && Input.GetMouseButtonDown( 0 ) )
                {
                    if ( level == null )
                    {
                        level = new Level( 10 , showProgress: false );
                        Wave wave = new Wave( 1 , stage );

                        for ( int i = 0 ; _waveEditor.selectedWaveDefinition.waveEvents.Count > i ; i++ )
                        {
                            switch ( ( WaveEvent.Type ) _waveEditor.selectedWaveDefinition.waveEvents[ i ].type )
                            {
                                case WaveEvent.Type.SpawnEnemy:
                                    wave.Add( new SpawnEnemyEvent( Definitions.Enemy( Definitions.Enemies.Default ) , _waveEditor.selectedWaveDefinition.waveEvents[ i ] ) );
                                    break;
                            }
                        }

                        level.Add( wave );
                        button.SetLabel( "Stop" );
                    }
                    else
                    {
                        button.SetLabel( "Test" );
                        stage.ClearLanes();
                        level.DestroyProgress();
                        level = null;
                    }
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) );

        _testButton.SetViewportPosition( new Vector2( 1 , 1 ) );
        _testButton.SetPosition( _testButton.position + Vector3.left * _testButton.width );

        _saveButton = new Button( "Save" , "Save" , 1.5f , 0.5f , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                    ScriptableObjects.Save();
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) );

        _saveButton.SetPosition( _testButton.position + Vector3.left * ( _saveButton.width ) );
    }
}
