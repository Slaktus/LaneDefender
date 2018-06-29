#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NeoEditor
{
    public void Update()
    {
        for ( int i = 0 ; _campaignMapDropdowns.Count > i ; i++ )
            _campaignMapDropdowns[ i ].Update();

        campaignEditor.Update();
        missionEditor.Update();
    }

    public void ShowStage() { }
    public void HideStage() { }

    public void ShowCampaignMap()
    {
        HideCampaignMap();
        campaignMap = new CampaignMap( campaignEditor.selectedCampaign.width , campaignEditor.selectedCampaign.height , campaignEditor.selectedCampaign.columns , campaignEditor.selectedCampaign.rows );

        for ( int i = 0 ; campaignMap.tileMap.count > i ; i++ )
        {
            int index = i;
            Dropdown dropdown = new Dropdown( "CampaignMap" + index , index.ToString() , campaignMap.tileMap.tileWidth - 1 , campaignMap.tileMap.tileHeight * 0.5f , campaignEditor.container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) && button.containsMouse )
                    {
                        if ( campaignEditor.selectedCampaign.Get( index ) == missionEditor.selectedMission )
                            Debug.LogError( "strap it in" );
                        else if ( !( button as Dropdown ).HasLayout( missionEditor.missions ) )
                            missionEditor.ShowMissionSets( index , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ,
                Close: ( Button button ) =>
                {
                    Dropdown d = button as Dropdown;

                    if ( ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) ) && ( d.HasLayout( missionEditor.missions ) || d.HasLayout( missionEditor.missionSets ) ) )
                    {
                        missionEditor.HideMissions();
                        missionEditor.HideMissionSets();
                        ( button as Dropdown ).RemoveLayouts();
                    }
                } );

            dropdown.SetPosition( campaignMap.tileMap.PositionOf( index ) );
            _campaignMapDropdowns.Add( dropdown );
        }
    }

    public void HideCampaignMap()
    {
        for ( int i = 0 ; _campaignMapDropdowns.Count > i ; i++ )
            _campaignMapDropdowns[ i ].Destroy();

        _campaignMapDropdowns.Clear();
    }

    public void Refresh()
    {
        ShowCampaignMap();
    }

    public Dropdown GetDropdown( int index ) => _campaignMapDropdowns[ index ];

    public T Load<T>( string path ) where T : ScriptableObject => AssetDatabase.LoadAssetAtPath<T>( path + typeof( T ) + ".asset" );
    private T Create<T>( string path ) where T : ScriptableObject => ScriptableObjects.Create<T>( path + typeof( T ) + ".asset" );

    public Vector3 mousePosition => Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );
    public CampaignMap campaignMap { get; private set; }
    public CampaignEditor campaignEditor { get; }
    public MissionEditor missionEditor { get; }
    public Stage stage { get; private set; }
    public GameObject container { get; }

    public CampaignData campaignData { get; }
    public StageData stageData { get; }
    public WaveData waveData { get; }

    private List<Dropdown> _campaignMapDropdowns { get; }

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

        _campaignMapDropdowns = new List<Dropdown>();
        container = new GameObject( "Editor" );

        campaignEditor = new CampaignEditor( this , Vector3.zero );
        missionEditor = new MissionEditor( this );
    }
}

#endif //UNITY_EDITOR