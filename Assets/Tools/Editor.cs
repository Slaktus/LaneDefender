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

        stage?.Update();
        _waveEditor?.Update();
        waveEditorDropdown?.Update();
    }

    public Dropdown waveEditorDropdown { get; }
    public Stage stage { get; private set; }

    private Camera _camera { get; }
    private GameObject _container { get; }
    private WaveEditor _waveEditor { get; }

    public Editor()
    {
        _camera = Camera.main;
        _container = new GameObject( "Editor" );
        _waveEditor = new WaveEditor( this , _container );

        int fontSize = 20;
        float buttonWidth = 3;
        float buttonHeight = 1;

        waveEditorDropdown = new Dropdown( "WaveEditor" , "Wave Editor" , buttonWidth , buttonHeight , _container ,
            fontSize: fontSize ,
            Enter: ( Button button ) => button.SetColor( !_waveEditor.showingWaveSets ? Color.green : button.color ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) && !_waveEditor.showingWaveSets )
                {
                    button.SetColor( Color.yellow );
                    _waveEditor.Show( waveEditorDropdown.localPosition + ( Vector3.back * waveEditorDropdown.height ) );
                    waveEditorDropdown.AddLayout( _waveEditor.waveSetLayout );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( !_waveEditor.showingWaveSets ? Color.white : button.color ) ,
            Close: ( Button button ) => 
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    waveEditorDropdown.RemoveLayout( _waveEditor.waveDefinitionLayout );
                    waveEditorDropdown.RemoveLayout( _waveEditor.waveSetLayout );
                    _waveEditor.HideWaveDefinitions();
                    _waveEditor.HideWaveSets();

                    button.SetColor( Color.white );
                }
            } );

        waveEditorDropdown.SetViewportPosition( new Vector2( 0 , 1 ) );

        stage = new Stage(
            speed: 5 ,
            width: 25 ,
            height: 15 ,
            laneSpacing: 1 ,
            laneCount: 5 ,
            conveyor: null ,
            player: new Player() );
    }
}
