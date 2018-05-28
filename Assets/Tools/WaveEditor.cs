using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveEditor
{
    public void Create()
    {
        Debug.Log( "Created" );
        WaveData asset = ScriptableObject.CreateInstance<WaveData>();
        AssetDatabase.CreateAsset( asset , waveDataPath + "WaveData.asset" );
        AssetDatabase.SaveAssets();
    }

    public void Load() => waveData = AssetDatabase.LoadAssetAtPath<WaveData>( waveDataPath + "WaveData.asset" );
    public void Save()
    {

    }

    public WaveData waveData { get; private set; }
    private const string waveDataPath = "Assets/Data/Waves/";

    public WaveEditor()
    {
        Load();


        if ( waveData == null )
            Create();
        else
            Debug.Log( "Loaded" );
    }
}