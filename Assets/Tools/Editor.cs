using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor
{
    public void Update()
    {
        _waveEditor?.Update();
        waveEditorDropdown?.Update();
    }

    public Dropdown waveEditorDropdown { get; }

    private GameObject _container { get; }
    private WaveEditor _waveEditor { get; }

    public Editor()
    {
        int fontSize = 20;
        float buttonWidth = 3;
        float buttonHeight = 1;
        _container = new GameObject( "Editor" );
        _waveEditor = new WaveEditor( this , _container );

        waveEditorDropdown = new Dropdown( "WaveEditor" , "Wave Editor" , buttonWidth , buttonHeight , _container ,
            fontSize: fontSize ,
            Enter: ( Button button ) => button.SetColor( !_waveEditor.showingWaveSets ? Color.green : button.color ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) && !_waveEditor.showingWaveSets )
                {
                    button.SetColor( Color.yellow );
                    _waveEditor.Show( waveEditorDropdown.localPosition + ( Vector3.back * waveEditorDropdown.height ) );
                    ( button as Dropdown ).AddLayout( _waveEditor.waveSetLayout );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( !_waveEditor.showingWaveSets ? Color.white : button.color ) ,
            Close: ( Button button ) => 
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ( button as Dropdown ).RemoveLayout( _waveEditor.waveSetLayout );
                    ( button as Dropdown ).RemoveLayout( _waveEditor.waveDefinitionLayout );
                    _waveEditor.HideWaveDefinitions();
                    _waveEditor.HideWaveSets();

                    button.SetColor( Color.white );
                }
            } );

        waveEditorDropdown.SetViewportPosition( new Vector2( 0 , 1 ) );
    }
}
