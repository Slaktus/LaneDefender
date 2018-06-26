﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Assets
{
    public static void Initialize( MonoBehaviour client ) => client.StartCoroutine( LoadAssetBundles() );
    public static StageData Get( StageDataSets data ) => _stageData[ ( int ) data ];
    public static WaveData Get( WaveDataSets data ) => _waveData[ ( int ) data ];

    private static IEnumerator LoadAssetBundles()
    {
        string assetBundles = System.IO.Path.Combine( Application.streamingAssetsPath , "AssetBundles" );
        string platform = string.Empty;

        #if UNITY_STANDALONE_WIN
            platform = System.IO.Path.Combine( assetBundles , "PC" );
        #endif

        #if UNITY_SWITCH
            platform = System.IO.Path.Combine( assetBundles , "Switch" );
        #endif

        //STAGE DATA
        AssetBundleCreateRequest stageData = AssetBundle.LoadFromFileAsync( System.IO.Path.Combine( platform , "stagedata" ) );

        yield return stageData;

        string[] stageDataNames = stageData.assetBundle.GetAllAssetNames();

        for ( int i = 0 ; stageDataNames.Length > i ; i++ )
            _stageData.Add( stageData.assetBundle.LoadAsset<StageData>( stageDataNames[ i ] ) );

        //WAVE DATA
        AssetBundleCreateRequest waveData = AssetBundle.LoadFromFileAsync( System.IO.Path.Combine( platform , "wavedata" ) );

        yield return waveData;

        string[] waveDataNames = waveData.assetBundle.GetAllAssetNames();

        for ( int i = 0 ; waveDataNames.Length > i ; i++ )
            _waveData.Add( waveData.assetBundle.LoadAsset<WaveData>( waveDataNames[ i ] ) );

        loaded = true;
    }

    public static bool loaded { get; private set; }

    private static List<StageData> _stageData { get; set; }
    private static List<WaveData> _waveData { get; set; }

    static Assets()
    {
        loaded = false;
        _stageData = new List<StageData>();
        _waveData = new List<WaveData>();
    }

    public enum StageDataSets
    {
        Default = 0,
        Count = 1
    }

    public enum WaveDataSets
    {
        Default = 0,
        Count = 1
    }
}