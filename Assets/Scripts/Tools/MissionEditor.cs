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
        missionTimeline?.Update();
        _indicator.transform.position = _editor.mousePosition;
    }

    public void ShowMissionTimeline()
    {
        missionTimeline?.Destroy();

        missionTimeline = new Button( "MissionTimeline" , string.Empty , stage.width , 1 , container ,
            Enter: ( Button button ) => ShowIndicator() ,
            Stay: ( Button button ) => 
            {
                if ( Input.GetMouseButton( 0 ) )
                {
                    //ok, so now we pretty much just need to insert wave sets here ...
                    //we don't have anything to do that do we
                    //don't think so no

                    //probably time to do the whole dataflow thing now ...
                    //lists of items in dropdowns is pretty solved
                    Debug.Log( Helpers.Normalize( _indicator.transform.position.x , button.rect.xMax , button.rect.xMin ) );
                }
            } ,
            Exit: ( Button button ) => HideIndicator() );

        missionTimeline.SetPosition( new Vector3( stage.start + ( stage.width * 0.5f ) , 0 , Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ).z ) + ( Vector3.back * missionTimeline.height * 0.5f ) );
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
                        selectedMissionSet = _editor.campaignData.missionSets[ capturedIndex ];
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
        int count = selectedMissionSet.missionDefinitions.Count + 1;
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
                        MissionDefinition missionDefinition = ScriptableObject.CreateInstance<MissionDefinition>();
                        missionDefinition.Initialize( "MissionDefinition" , 120 );
                        ScriptableObjects.Add( missionDefinition , selectedMissionSet );
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
                        selectedMission = selectedMissionSet.missionDefinitions[ capturedIndex ];
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

    private void ShowIndicator() => _indicator.enabled = true;
    private void HideIndicator() => _indicator.enabled = false;

    public MissionDefinition selectedMission { get; private set; }
    public MissionSet selectedMissionSet { get; private set; }
    public Button missionTimeline { get; private set; }
    public Layout missionSets { get; private set; }
    public Layout missions { get; private set; }
    public Stage stage { get; private set; }
    public GameObject container { get; }

    private NeoEditor _editor { get; }
    private MeshRenderer _indicator { get; }

    public MissionEditor( NeoEditor editor )
    {
        _editor = editor;
        _indicator = GameObject.CreatePrimitive( PrimitiveType.Sphere ).GetComponent<MeshRenderer>();
        container = new GameObject( typeof( MissionEditor ).Name );
        container.transform.SetParent( editor.container.transform );
        _indicator.transform.SetParent( container.transform);
        HideIndicator();
    }
}
#endif //UNITY_EDITOR

