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
        _missionEditorLayout?.Update();
    }

    public void ShowMissionSets( int index , Vector3 position )
    {
        HideMissionSets();
        int count = _editor.campaignData.missionSets.Count;
        missionSets = new Layout( "MissionSets" , 4 , count + 1 , 0.25f , 0.1f , count + 1 , container );
        missionSets.SetPosition( position + ( Vector3.right * missionSets.width * 0.5f ) + ( Vector3.back * missionSets.height * 0.5f ) );

        missionSets.Add(new List<Button>(Button.GetButtons(count, (int capturedIndex) => new Button("MissionSet", "Mission Set", 4, 1, container,
            Enter: (Button button) => button.SetColor(selectedMissionSet == _editor.campaignData.missionSets[ capturedIndex ] ? button.color : Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    button.SetColor(Color.yellow);
                    selectedMissionSet = _editor.campaignData.missionSets[ capturedIndex ];
                    ShowMissions(index, button.position + new Vector3(button.width * 0.5f, 0, button.height * 0.5f));
                }
            },
            Exit: (Button button) => button.SetColor(selectedMissionSet == _editor.campaignData.missionSets[ capturedIndex ] ? button.color : Color.white),
            Close: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0) && selectedMissionSet == _editor.campaignData.missionSets[ capturedIndex ] && missions != null && !missions.containsMouse)
                {
                    HideMissions();
                    selectedMissionSet = null;
                    button.SetColor(Color.white);
                }
            })))
        {
            new Button( "NewMissionSet" , "New Set" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        HideMissions();
                        ScriptableObjects.Add( ScriptableObject.CreateInstance<MissionSet>() , _editor.campaignData );
                        ShowMissionSets( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        } , true );
    }

    public void HideMissionSets()
    {
        selectedMissionSet = null;
        missionSets?.Destroy();
        missionSets = null;
    }

    public void ShowMissions( int index , Vector3 position )
    {
        HideMissions();
        int count = selectedMissionSet.missionDefinitions.Count;
        missions = new Layout("MissionLayout", 4, count + 1, 0.25f, 0.1f, count + 1, container);
        missions.SetPosition( position + ( Vector3.right * missions.width * 0.5f ) + ( Vector3.back * missions.height * 0.5f ) );

        missions.Add(new List<Button>(Button.GetButtons(count, (int capturedIndex) => new Button("Mission", "Mission", 4, 1, container,
              Enter: (Button button) => button.SetColor(Color.green),
              Stay: (Button button) =>
              {
                  if (Input.GetMouseButtonDown(0))
                  {
                      selectedMission = selectedMissionSet.missionDefinitions[ capturedIndex ];
                      _editor.campaignEditor.selectedCampaign.Add(selectedMission, index);
                      Button mapButton = _editor.GetMapButton(index);
                      mapButton.SetLabel(selectedMission.name);
                      mapButton.SetColor(Color.white);
                      mapButton.Deselect();
                      HideMissionSets();
                      HideMissions();
                  }
              },
              Exit: (Button button) => button.SetColor(Color.white))))
        {
            new Button( "NewMission" , "New Mission" , 4 , 1 , container ,
                Enter: ( Button button ) => button.SetColor( Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        ScriptableObjects.Add( MissionDefinition.Default() , selectedMissionSet );
                        ShowMissions( index , position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( Color.white ) )
        });
        missions.Refresh();
    }

    public void HideMissions()
    {
        missions?.Destroy();
        missions = null;
    }

    public void ShowMissionEditor()
    {
        HideMissionEditor();
        List<Element> missionEditorButtons = new List<Element>( 2 )
        {
            new Label( "Duration:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Duration" , selectedMission.duration.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedMission.duration );
            } )
        };

        _missionEditorLayout = new Layout( "StageEditor" , 3 , 1 , 0.25f , 0.1f , missionEditorButtons.Count / 2 , container );
        _missionEditorLayout.Add( missionEditorButtons , true );
        _missionEditorLayout.SetPosition( ( _editor.stageEditor.stageEditorLayout != null ? _editor.stageEditor.stageEditorLayout.position : _editor.stageEditor.stageSets.position ) + ( Vector3.back * ( _missionEditorLayout.height + ( _editor.stageEditor.stageEditorLayout != null ? _editor.stageEditor.stageEditorLayout.height : _editor.stageEditor.stageSets.height ) ) * 0.5f ) );
    }

    public void HideMissionEditor()
    {
        _missionEditorLayout?.Destroy();
        _missionEditorLayout = null;
    }

    //NOTE: Find everyone who calls this and make them hide TimelineEditor too
    public void Hide()
    {
        HideMissions();
        HideMissionSets();
    }

    public void SetSelectedMission( MissionDefinition selectedMission ) => this.selectedMission = selectedMission;

    public MissionSet selectedMissionSet { get; private set; }
    public MissionDefinition selectedMission { get; private set; }
    public Layout missionSets { get; private set; }
    public Layout missions { get; private set; }
    public GameObject container { get; }

    private Editor _editor { get; }
    private Layout _missionEditorLayout { get; set; }

    public MissionEditor( Editor editor )
    {
        _editor = editor;
        container = new GameObject( typeof( MissionEditor ).Name );
        container.transform.SetParent( editor.container.transform );
    }
}
#endif //UNITY_EDITOR

