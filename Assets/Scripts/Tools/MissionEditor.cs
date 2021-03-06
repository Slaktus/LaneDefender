﻿#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class MissionEditor : Layout
{
    public void SetSelectedMissionSet(MissionSet selectedMissionSet) => this.selectedMissionSet = selectedMissionSet;
    public void SetSelectedMission(MissionDefinition selectedMission) => this.selectedMission = selectedMission;
    public void SetStageDefinition(StageDefinition stageDefinition) => selectedMission.stageDefinition = stageDefinition;

    private void ShowCampaignMap() => _editor.campaignMapEditor.ShowCampaignMap();
    private MissionSet GetMissionSet(int index) => _editor.campaignData.GetMissionSet(index);
    private MissionDefinition GetMission(int index) => selectedMissionSet.GetMission(index);

    public void ShowMissionSets( int index , Vector3 position )
    {
        HideMissionSets();
        int count = _editor.campaignData.missionSets.Count;
        Add(missionSets = new Layout("MissionSets", 4, count + 1, 0.25f, 0.1f, count + 1, container));
        missionSets.SetPosition( position + ( Vector3.right * missionSets.width * 0.5f ) + ( Vector3.back * missionSets.height * 0.5f ) );

        missionSets.Add(new List<RenameableDeletableButton>(RenameableDeletableButton.GetButtons(count, 
            (int capturedIndex) => new RenameableDeletableButton(_editor.campaignData.missionSets[ capturedIndex ].name, 3, 1, container,
            fontSize: 20,
            DeleteStay: (Button button) =>
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (button.selected)
                        HideMissions();

                    _editor.campaignData.Remove(GetMissionSet(capturedIndex));
                    ShowMissionSets(index, position);
                }
            },
            EndInput: (Field field) =>
            {
                GetMissionSet(capturedIndex).name = field.label.text;
                ShowMissionSets(index,position);
            },
            Enter: (Button button) => button.SetColor(selectedMissionSet == GetMissionSet( capturedIndex ) ? button.color : Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    button.SetColor(Color.yellow);
                    selectedMissionSet = GetMissionSet(capturedIndex);
                    ShowMissions(index, button.position + new Vector3(missionSets.width * 0.5f, 0, button.height * 0.5f));
                }
            },
            Exit: (Button button) => button.SetColor(selectedMissionSet == GetMissionSet(capturedIndex) ? button.color : Color.white),
            Close: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0) && selectedMissionSet == GetMissionSet(capturedIndex) && missions != null && !missions.containsMouse)
                {
                    HideMissions();
                    selectedMissionSet = null;
                    button.SetColor(Color.white);
                }
            }))));

        missionSets.Add(new Button("New Set", 4, 1, container, "NewMissionSet",
                fontSize: 20,
                Enter: (Button button) => button.SetColor(Color.green),
                Stay: (Button button) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        HideMissions();
                        ScriptableObjects.Add(ScriptableObject.CreateInstance<MissionSet>(), _editor.campaignData);
                        ShowMissionSets(index, position);
                    }
                },
                Exit: (Button button) => button.SetColor(Color.white)), true);
    }

    public void HideMissionSets()
    {
        if (missionSets != null)
            Remove(missionSets);

        selectedMissionSet = null;
        missionSets?.Destroy();
        missionSets = null;
    }

    public void ShowMissions( int index , Vector3 position )
    {
        HideMissions();
        int count = selectedMissionSet.missionDefinitions.Count;
        Add(missions = new Layout("MissionLayout", 4, count + 1, 0.25f, 0.1f, count + 1, container));
        missions.SetPosition( position + ( Vector3.right * missions.width * 0.5f ) + ( Vector3.back * missions.height * 0.5f ) );

        missions.Add(new List<RenameableDeletableButton>(RenameableDeletableButton.GetButtons(count, 
            (int capturedIndex) => new RenameableDeletableButton(GetMission(capturedIndex).name, 3, 1, container,
            fontSize: 20,
            DeleteStay: (Button button) =>
            {
                if (Input.GetMouseButtonUp(0))
                {
                    selectedMissionSet.Remove(GetMission(capturedIndex));
                    ShowMissions(index, position);
                }
            },
            EndInput: (Field field) =>
            {
                GetMission(capturedIndex).name = field.label.text;
                ShowMissions(index, position);
            },
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    selectedMission = GetMission(capturedIndex);
                    selectedCampaign.Add(selectedMission, index);
                    ShowCampaignMap();
                    HideMissionSets();
                    HideMissions();
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)))));

        missions.Add(new Button("New Mission", 4, 1, container, "NewMission",
                    fontSize: 20,
                    Enter: (Button button) => button.SetColor(Color.green),
                    Stay: (Button button) =>
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            ScriptableObjects.Add(MissionDefinition.Default(), selectedMissionSet);
                            ShowMissions(index, position);
                        }
                    },
                    Exit: (Button button) => button.SetColor(Color.white)), true);
    }

    public void HideMissions()
    {
        if (missions != null)
            Remove(missions);

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

        Add(_missionEditorLayout = new Layout( "StageEditor" , 4 , 1 , 0.25f , 0.1f , missionEditorButtons.Count / 2 , container ));
        _missionEditorLayout.Add( missionEditorButtons , true );
        _missionEditorLayout.SetPosition( ( _editor.stageEditor.stageEditorLayout != null ? _editor.stageEditor.stageEditorLayout.position : _editor.stageEditor.stageSets.position ) + ( Vector3.back * ( _missionEditorLayout.height + ( _editor.stageEditor.stageEditorLayout != null ? _editor.stageEditor.stageEditorLayout.height : _editor.stageEditor.stageSets.height ) ) * 0.5f ) );
    }

    public void HideMissionEditor()
    {
        if (_missionEditorLayout != null)
            Remove(_missionEditorLayout);

        _missionEditorLayout?.Destroy();
        _missionEditorLayout = null;
    }

    //NOTE: Find everyone who calls this and make them hide TimelineEditor too
    public override void Hide()
    {
        HideMissions();
        HideMissionSets();
    }


    public MissionSet selectedMissionSet { get; private set; }
    public MissionDefinition selectedMission { get; private set; }
    public Layout missionSets { get; private set; }
    public Layout missions { get; private set; }

    private CampaignDefinition selectedCampaign => _editor.campaignEditor.selectedCampaign;

    private Editor _editor { get; }
    private Layout _missionEditorLayout { get; set; }

    public MissionEditor(Editor editor, GameObject parent = null) : base(typeof(MissionEditor).Name, parent)
    {
        _editor = editor;
    }
}
#endif //UNITY_EDITOR