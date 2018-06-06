using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor
{
    public void Update()
    {
        waveEditor?.Update();
        waveEditorButton?.Update();
    }

    public Button waveEditorButton { get; }
    GameObject container { get; }
    WaveEditor waveEditor { get; }

    public Editor()
    {
        int fontSize = 20;
        float buttonWidth = 3;
        float buttonHeight = 1;
        container = new GameObject( "Editor" );

        waveEditorButton = new Button( "WaveEditor" , "Wave Editor" , buttonWidth , buttonHeight , container ,
            fontSize: fontSize ,
            Enter: ( Button button ) => button.SetColor( !waveEditor.showingWaveSets ? Color.green : button.color ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    if ( !waveEditor.showingWaveSets )
                    {
                        button.SetColor( Color.yellow );
                        waveEditor.Show( waveEditorButton.localPosition + ( Vector3.back * waveEditorButton.height ) );
                    }
                }
            } ,
            Exit: ( Button button ) => button.SetColor( !waveEditor.showingWaveSets ? Color.white : button.color ) );

        waveEditorButton.SetViewportPosition( new Vector2( 0 , 1 ) );
        waveEditor = new WaveEditor( this , container );
    }
}
