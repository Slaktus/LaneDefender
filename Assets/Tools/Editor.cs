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
        waveEditor = new WaveEditor( parent: container = new GameObject( "Editor" ) );
        waveEditorButton = new Button( "WaveEditor" , "Wave Editor" , 5 , 3 , container ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    if ( !showingWaveEditor )
                    {
                        waveEditor.Show();
                        showingWaveEditor = true;
                    }
                    else
                    {
                        waveEditor.Hide();
                        showingWaveEditor = false;
                    }
                }
            } );
    }
}
