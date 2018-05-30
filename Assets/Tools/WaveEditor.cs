using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class WaveEditor
{
    public void Update()
    {
        waveSetLayout?.Update();
        waveDefinitionLayout?.Update();
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
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveSet" , "Add Wave Set" , 10 , 3 , waveSetContainer ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    waveData.AddWaveSet();
                    HideWaveSets();
                    ShowWaveSets();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( waveData.waveSets != null )
            for ( int i = 0 ; waveData.waveSets.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveSet" , "Wave Set" , 10 , 3 , waveSetContainer ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            currentWaveSet = waveData.waveSets[ index ];
                            HideWaveDefinitions();
                            ShowWaveDefinitions( currentWaveSet.waveDefinitions );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveSetLayout = new Layout( "WaveSetButtons" , 10 , 3 * buttons.Count , 1 , 0.1f , buttons.Count );
        waveSetLayout.SetLocalPosition( Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ) + new Vector3( 5 , 0 , -buttons.Count * 3 * 0.5f ) );
        waveSetLayout.Add( buttons , true );
    }

    private void HideWaveSets() => waveSetLayout?.Destroy();

    private void ShowWaveDefinitions( List<WaveDefinition> waveDefinitions )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveDefinition" , "Add Wave\nDefinition" , 10 , 3 , waveDefinitionContainer ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( currentWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                {
                    currentWaveSet.AddWaveDefinition();
                    HideWaveDefinitions();
                    ShowWaveDefinitions( currentWaveSet.waveDefinitions );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( currentWaveSet.waveDefinitions != null )
            for ( int i = 0 ; currentWaveSet.waveDefinitions.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveDefinition" , "Wave Definition" , 10 , 3 , waveDefinitionContainer ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( currentWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                            currentWaveDefinition = currentWaveSet.waveDefinitions[ index ];
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , 10 , 3 * buttons.Count , 1 , 0.1f , buttons.Count );
        waveDefinitionLayout.SetLocalPosition( Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ) + new Vector3( 15 , 0 , -buttons.Count * 3 * 0.5f ) );
        waveDefinitionLayout.Add( buttons , true );
    }

    private void HideWaveDefinitions() => waveDefinitionLayout?.Destroy();

    public GameObject container { get; }
    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public Layout waveDefinitionLayout { get; private set; }

    public WaveSet currentWaveSet { get; private set; }
    public WaveDefinition currentWaveDefinition { get; private set; }
    public GameObject waveSetContainer { get; private set; }
    public GameObject waveDefinitionContainer { get; private set; }

    private const string waveDataPath = "Assets/Data/Waves/";

    public WaveEditor()
    {
        container = new GameObject( "Container" );
        waveSetContainer = new GameObject( "WaveSetContainer" );
        waveDefinitionContainer = new GameObject( "WaveDefinitionContainer" );
        waveSetContainer.transform.SetParent( container.transform );
        waveDefinitionContainer.transform.SetParent( container.transform );

        Load();

        if ( waveData == null )
            Create();

        ShowWaveSets();
    }
}