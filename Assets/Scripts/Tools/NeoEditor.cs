#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NeoEditor
{
    public void Update() {}

    public T Load<T>( string path ) where T : ScriptableObject => AssetDatabase.LoadAssetAtPath<T>( path + typeof( T ) + ".asset" );
    private T Create<T>( string path ) where T : ScriptableObject => ScriptableObjects.Create<T>( path + typeof( T ) + ".asset" );

    public CampaignMap campaignMap { get; private set; }
    public Stage stage { get; private set; }

    public CampaignData campaignData { get; }
    public StageData stageData { get; }
    public WaveData waveData { get; }

    private const string _campaignDataPath = "Assets/AssetBundleSource/Campaigns/";
    private const string _stageDataPath = "Assets/AssetBundleSource/Stages/";
    private const string _waveDataPath = "Assets/AssetBundleSource/Waves/";

    public NeoEditor()
    {
        waveData = Load<WaveData>( _waveDataPath );
        stageData = Load<StageData>( _stageDataPath );
        campaignData = Load<CampaignData>( _campaignDataPath );

        if ( waveData == null )
            waveData = Create<WaveData>( _waveDataPath );

        if ( stageData == null )
            stageData = Create<StageData>( _stageDataPath );

        if ( campaignData == null )
            campaignData = Create<CampaignData>( _campaignDataPath );

        campaignComposite = new CampaignLayout( this );
    }
}

#endif //UNITY_EDITOR