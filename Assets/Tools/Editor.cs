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

    GameObject container { get; }
    WaveEditor waveEditor { get; }
    Button waveEditorButton { get; }
    bool showingWaveEditor { get; set; }

    public Editor()
    {
        int fontSize = 20;
        float buttonWidth = 3;
        float buttonHeight = 1;
        container = new GameObject( "Editor" );

        waveEditorButton = new Button( "WaveEditor" , "Wave Editor" , buttonWidth , buttonHeight , container ,
            fontSize: fontSize ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    if ( !showingWaveEditor )
                    {
                        waveEditor.Show( waveEditorButton.localPosition + ( Vector3.back * waveEditorButton.height ) );
                        showingWaveEditor = true;
                    }
                    else
                    {
                        waveEditor.Hide();
                        showingWaveEditor = false;
                    }
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) );

        waveEditorButton.SetViewportPosition( new Vector2( 0 , 1 ) );

        waveEditor = new WaveEditor( container );
    }
}
