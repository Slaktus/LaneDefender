﻿#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionEditor
{
    public void Update()
    {
        _missions?.Update();
        _missionSets?.Update();
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
        _missionSets?.Destroy();
        int count = _editor.campaignData.missionSets.Count + 1;
        _missionSets = new Layout( "MissionSets" , 4 , count , 0.25f , 0.1f , count , container );
        _missionSets.SetPosition( position + ( Vector3.right * _missionSets.width * 0.5f ) + ( Vector3.back * _missionSets.height * 0.5f ) );

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

        _missionSets.Add( buttons );
        _missionSets.Refresh();
    }

    public void HideMissionSets()
    {
        _missionSets?.Destroy();
        _missionSets = null;
    }

    public void ShowMissions( int index , Vector3 position )
    {
        _missions?.Destroy();
        int count = _selectedMissionSet.missionDefinitions.Count + 1;
        _missions = new Layout( "MissionLayout" , 4 , count , 0.25f , 0.1f , count , container );
        _missions.SetPosition( position + ( Vector3.right * _missions.width * 0.5f ) + ( Vector3.back * _missions.height * 0.5f ) );

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
                        ScriptableObjects.Add( missionDefinition , _selectedMissionSet );
                        //_campaignMap.Add( index , missionDefinition );
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
                        _selectedMission = _selectedMissionSet.missionDefinitions[ capturedIndex ];
                        //actually no, this should instead assign/associate the mission to the grid tile in question
                        //should also change the label of the dropdown to indicate what mission is currently loaded
                        //then we need to handle the layout that allows the player to hop into the stage/wave/mission editor
                        //goal line is right around the bend!
                        HideMissionSets();
                        HideMissions();
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) ) );
        }

        _missions.Add( buttons );
        _missions.Refresh();
    }

    public void HideMissions()
    {
        _missions?.Destroy();
        _missions = null;
    }

    private void ShowIndicator() => _indicator.enabled = true;
    private void HideIndicator() => _indicator.enabled = false;

    public Button missionTimeline { get; private set; }
    public Stage stage { get; private set; }
    public GameObject container { get; }

    private NeoEditor _editor { get; }
    private MeshRenderer _indicator { get; }
    private Layout _missions { get; set; }
    private Layout _missionSets { get; set; }
    private MissionSet _selectedMissionSet { get; set; }
    private MissionDefinition _selectedMission { get; set; }

    public MissionEditor( NeoEditor editor )
    {
        _editor = editor;
        _indicator = GameObject.CreatePrimitive( PrimitiveType.Sphere ).GetComponent<MeshRenderer>();
        container = new GameObject( typeof( MissionEditor ).Name );
        ShowMissionTimeline();
        HideIndicator();
    }
}
#endif //UNITY_EDITOR

