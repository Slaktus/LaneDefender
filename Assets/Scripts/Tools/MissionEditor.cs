#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionEditor
{
    public void Update()
    {
        missions?.Update();
        missionSets?.Update();
        _missionTimeline?.Update();
        _indicator.transform.position = _editor.mousePosition;

        //should be replaced with a proper layout
        //layouts can ignore constraints now after all
        for ( int i = 0 ; _dropdowns.Count > i ; i++ )
            _dropdowns[ i ].Update();
    }

    public void AddMissionToTimeline( WaveSet waveSet )
    {
        selectedMission.Add( waveSet , timelinePosition );

        Dropdown dropdown = new Dropdown( "Wave" , "Wave" , 2 , 1 , container ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Exit: ( Button button ) => button.SetColor( Color.white ) );

        dropdown.SetPosition( new Vector3( _missionTimeline.rect.xMin + ( timelinePosition * _missionTimeline.rect.xMax ) , 0 , _missionTimeline.rect.yMin + 0.5f ) );
        _dropdowns.Add( dropdown );
    }

    public void ShowMissionTimeline()
    {
        _missionTimeline?.Destroy();

        _missionTimeline = new Button( "MissionTimeline" , string.Empty , _editor.stage.width , 1 , container ,
            Enter: ( Button button ) => ShowIndicator() ,
            Stay: ( Button button ) => 
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    _editor.waveEditor.Show( new Vector3( _indicator.transform.position.x , 0 , button.rect.yMin ) );
                    timelinePosition = Helpers.Normalize( _indicator.transform.position.x , button.rect.xMax , button.rect.xMin );
                }
            } ,
            Exit: ( Button button ) => HideIndicator() );

        _missionTimeline.SetPosition( new Vector3( _editor.stage.start + ( _editor.stage.width * 0.5f ) , 0 , Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ).z ) + ( Vector3.back * _missionTimeline.height * 0.5f ) );
    }

    public void ShowMissionSets( int index , Vector3 position )
    {
        missionSets?.Destroy();
        int count = _editor.campaignData.missionSets.Count + 1;
        missionSets = new Layout( "MissionSets" , 4 , count , 0.25f , 0.1f , count , container );
        missionSets.SetPosition( position + ( Vector3.right * missionSets.width * 0.5f ) + ( Vector3.back * missionSets.height * 0.5f ) );
        _editor.GetDropdown( index ).AddLayout( missionSets );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewMissionSet" , "New Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideMissionSets();
                        ScriptableObjects.Add( ScriptableObject.CreateInstance<MissionSet>() , _editor.campaignData );
                        ShowMissionSets( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "MissionSet" , "Mission Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        _selectedMissionSet = _editor.campaignData.missionSets[ capturedIndex ];
                        ShowMissions( index , button.position + new Vector3( button.width * 0.5f , 0 , button.height * 0.5f ) );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        missionSets.Add( buttons );
        missionSets.Refresh();
    }

    public void HideMissionSets()
    {
        missionSets?.Destroy();
        missionSets = null;
    }

    public void ShowMissions( int index , Vector3 position )
    {
        missions?.Destroy();
        int count = _selectedMissionSet.missionDefinitions.Count + 1;
        missions = new Layout( "MissionLayout" , 4 , count , 0.25f , 0.1f , count , container );
        missions.SetPosition( position + ( Vector3.right * missions.width * 0.5f ) + ( Vector3.back * missions.height * 0.5f ) );
        _editor.GetDropdown( index ).AddLayout( missions );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "NewMission" , "New Mission" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideMissions();
                        ScriptableObjects.Add( MissionDefinition.Default() , _selectedMissionSet );
                        ShowMissions( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int capturedIndex = i;
            buttons.Add( new Button( "Mission" , "Mission" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        selectedMission = _selectedMissionSet.missionDefinitions[ capturedIndex ];
                        _editor.campaignEditor.selectedCampaign.Add( selectedMission , index );
                        _editor.GetDropdown( index ).SetLabel( selectedMission.name );
                        HideMissionSets();
                        HideMissions();
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        missions.Add( buttons );
        missions.Refresh();
    }

    public void HideMissions()
    {
        missions?.Destroy();
        missions = null;
    }

    public void Hide()
    {
        HideMissions();
        HideMissionSets();
        HideIndicator();
    }

    private void ShowIndicator() => _indicator.enabled = true;
    private void HideIndicator() => _indicator.enabled = false;

    public MissionDefinition selectedMission { get; private set; }
    public float timelinePosition { get; private set; }
    public Layout missionSets { get; private set; }
    public Layout missions { get; private set; }
    public GameObject container { get; }

    private Editor _editor { get; }
    private MeshRenderer _indicator { get; }
    private Button _missionTimeline { get; set; }
    private List<Dropdown> _dropdowns { get; set; }
    private MissionSet _selectedMissionSet { get; set; }

    public MissionEditor( Editor editor )
    {
        _editor = editor;
        _dropdowns = new List<Dropdown>();
        _indicator = GameObject.CreatePrimitive( PrimitiveType.Sphere ).GetComponent<MeshRenderer>();
        container = new GameObject( typeof( MissionEditor ).Name );
        container.transform.SetParent( editor.container.transform );
        _indicator.transform.SetParent( container.transform );
        _indicator.material.color = Color.yellow;
        HideIndicator();
    }
}
#endif //UNITY_EDITOR

