using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class WaveEditor
{
    public void Update()
    {
        waveSetLayout.Update();
    }

    public void Load() => waveData = AssetDatabase.LoadAssetAtPath<WaveData>( waveDataPath + "WaveData.asset" );
    public void Save() => AssetDatabase.SaveAssets();

    private void Create()
    {
        waveData = ScriptableObject.CreateInstance<WaveData>();
        AssetDatabase.CreateAsset( waveData , waveDataPath + "WaveData.asset" );
        AssetDatabase.SaveAssets();
    }

    private void ShowWaveSets()
    {
        //make a panel
        //make an ADD button
        //shove buttons in for each existing asset

        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveSet" , "Add Wave Set" , 10 , 3 , waveSetContainer ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    waveData.Add();
                    HideWaveSets();
                    ShowWaveSets();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( waveData.waveSets != null )
            for ( int i = 0 ; waveData.waveSets.Count > i ; i++ )
                buttons.Add( new Button( "WaveSet" , "Wave Set" , 10 , 3 , waveSetContainer ) );

        waveSetLayout = new Layout( "WaveSetButtons" , 10 , 3 * buttons.Count , 1 , 0.1f , 1 );
        waveSetLayout.Add( buttons );
    }

    private void HideWaveSets() => waveSetLayout.Destroy();

    private void ShowWaveDefinitions( List<WaveDefinition> waveDefinitions )
    {
        //make a panel
        //make an ADD button
        //shove buttons in etc

        for ( int i = 0 ; waveDefinitions.Count > i ; i++ )
        {
            
        }
    }

    public GameObject container { get; }
    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public GameObject waveSetContainer { get; private set; }
    private const string waveDataPath = "Assets/Data/Waves/";

    public WaveEditor()
    {
        container = new GameObject( "Container" );
        waveSetContainer = new GameObject( "WaveSetContainer" );
        waveSetContainer.transform.SetParent( container.transform );

        Load();

        if ( waveData == null )
            Create();

        ShowWaveSets();
    }
}