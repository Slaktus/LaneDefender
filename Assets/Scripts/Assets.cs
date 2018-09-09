using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Assets
{
    public static void Initialize( MonoBehaviour client , Action Callback ) => client.StartCoroutine( LoadAssetBundles(Callback) );
    public static CampaignData Get( CampaignDataSets data ) => _campaignData.Count > 0 ? _campaignData[ ( int ) data ] : null;
    public static ObjectData Get(ObjectDataSets data) => _objectData.Count > 0 ? _objectData[ (int) data ] : null;
    public static StageData Get( StageDataSets data ) => _stageData.Count > 0 ? _stageData[ ( int ) data ] : null;
    public static WaveData Get( WaveDataSets data ) => _waveData.Count > 0 ? _waveData[ ( int ) data ] : null;

    #if UNITY_EDITOR
    private static void Add<T>( string[] paths, List<T> target ) where T : DefinitionBase
    {
        T[] assets = new T[ paths.Length ];

        for (int i = 0; paths.Length > i; i++)
            assets[ i ] = AssetDatabase.LoadAssetAtPath<T>(paths[ i ]);

        target.AddRange(assets);
    }

    public static T Create<T>(string path) where T : DefinitionBase => ScriptableObjects.Create<T>(path + typeof(T) + ".asset");
    #endif

    public const string campaignDataPath = "Assets/AssetBundleSource/Campaigns/";
    public const string objectDataPath = "Assets/AssetBundleSource/Objects/";
    public const string stageDataPath = "Assets/AssetBundleSource/Stages/";
    public const string waveDataPath = "Assets/AssetBundleSource/Waves/";

    private static IEnumerator LoadAssetBundles( Action Callback )
    {
        #if UNITY_EDITOR

        Add(System.IO.Directory.GetFiles(campaignDataPath), _campaignData);
        Add(System.IO.Directory.GetFiles(objectDataPath), _objectData);
        Add(System.IO.Directory.GetFiles(stageDataPath), _stageData);
        Add(System.IO.Directory.GetFiles(waveDataPath), _waveData);
        yield return null;

        #else
        
        string assetBundles = System.IO.Path.Combine( Application.streamingAssetsPath , "AssetBundles" );
        string platform = string.Empty;
        Debug.Log("Start");

        #if UNITY_STANDALONE_WIN
            Debug.Log("Windows");
            platform = System.IO.Path.Combine( assetBundles , "PC" );
        #elif UNITY_WEBGL
            platform = System.IO.Path.Combine( assetBundles , "WebGL" );
        #elif UNITY_SWITCH
            platform = System.IO.Path.Combine( assetBundles , "Switch" );
        #endif

        //STAGE DATA
        AssetBundleCreateRequest stageData = AssetBundle.LoadFromFileAsync( System.IO.Path.Combine( platform , "stages" ) );

        Debug.Log("StageData");
        yield return stageData;

        string[] stageDataNames = stageData.assetBundle.GetAllAssetNames();

        for ( int i = 0 ; stageDataNames.Length > i ; i++ )
            _stageData.Add( stageData.assetBundle.LoadAsset<StageData>( stageDataNames[ i ] ) );

        //WAVE DATA
        AssetBundleCreateRequest waveData = AssetBundle.LoadFromFileAsync( System.IO.Path.Combine( platform , "waves" ) );

        Debug.Log("WaveData");
        yield return waveData;

        string[] waveDataNames = waveData.assetBundle.GetAllAssetNames();

        for ( int i = 0 ; waveDataNames.Length > i ; i++ )
            _waveData.Add( waveData.assetBundle.LoadAsset<WaveData>( waveDataNames[ i ] ) );

        //CAMPAIGN DATA
        AssetBundleCreateRequest campaignData = AssetBundle.LoadFromFileAsync( System.IO.Path.Combine( platform , "campaigns" ) );

        Debug.Log("CampaignData");
        yield return campaignData;

        string[] campaignDataNames = campaignData.assetBundle.GetAllAssetNames();

        for ( int i = 0 ; campaignDataNames.Length > i ; i++ )
            _campaignData.Add( campaignData.assetBundle.LoadAsset<CampaignData>( campaignDataNames[ i ] ) );

        //OBJECT DATA
        AssetBundleCreateRequest objectData = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(platform, "objects"));

        Debug.Log("ObjectData");
        yield return objectData;

        string[] objectDataNames = objectData.assetBundle.GetAllAssetNames();

        for (int i = 0; objectDataNames.Length > i; i++)
            _objectData.Add(objectData.assetBundle.LoadAsset<ObjectData>(objectDataNames[ i ]));

        loaded = true;

        #endif //UNITY_EDITOR

        Callback?.Invoke();
    }

    public static bool loaded { get; private set; }

    private static List<CampaignData> _campaignData { get; set; }
    private static List<ObjectData> _objectData { get; set; }
    private static List<StageData> _stageData { get; set; }
    private static List<WaveData> _waveData { get; set; }

    static Assets()
    {
        _campaignData = new List<CampaignData>();
        _objectData = new List<ObjectData>();
        _stageData = new List<StageData>();
        _waveData = new List<WaveData>();
        loaded = false;
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

    public enum CampaignDataSets
    {
        Default = 0,
        Count = 1
    }

    public enum ObjectDataSets
    {
        Default = 0,
        Count = 1
    }
}