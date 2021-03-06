﻿#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class StageEditor : Layout
{
    private StageSet GetStageSet(int index) => _editor.stageData.stageSets[ index ];
    private StageDefinition GetStageDefinition(int index) => _selectedStageSet.GetStage(index);
    private void SetStageDefinition(StageDefinition stageDefinition) => _editor.missionEditor.SetStageDefinition(stageDefinition);
    private void ShowMissionTimeline() => _editor.timelineEditor.ShowMissionTimeline();
    private void ShowMissionEditor() => _editor.missionEditor.ShowMissionEditor();
    private void ShowStage() => _editor.ShowStage(selectedStageDefinition);
    private void HideStage() => _editor.HideStage();

    public override void Show()
    {
        ShowStageSets();

        if (selectedStageDefinition != null )
        {
            if ( _editor.stage == null)
            {
                _editor.ShowStage(selectedStageDefinition);
                _editor.testButton.Enable();
                _editor.testButton.Show();
            }

            ShowStageEditor();
        }
    }

    public void ShowStageSets()
    {
        HideStageSets();
        int count = _editor.stageData.stageSets.Count;
        Add(stageSets = new Layout( "StageSetButtons" , 4, (count + 2), 0.25f, 0.1f, count + 2, container ));

        stageSets.SetViewportPosition(new Vector2(1, 1));
        stageSets.SetPosition(stageSets.position + (Vector3.left * stageSets.width) + Vector3.up + (Vector3.back * 0.5f));

        stageSets.Add(new List<RenameableDeletableButton>(RenameableDeletableButton.GetButtons(count, (int index) => new RenameableDeletableButton(GetStageSet( index ).name, 4, 1, container, 
            fontSize: 20,
            DeleteStay: (Button b) =>
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (b.selected)
                        HideStageDefinitions();

                    _editor.stageData.Remove(GetStageSet(index));
                    ShowStageSets();
                }
            },
            EndInput: (Field field) =>
            {
                GetStageSet(index).name = field.label.text;
                field.SetColor(Color.white);
            },
            Enter: (Button button) => button.SetColor(_stages != null && _selectedStageSet == GetStageSet( index ) ? button.color : Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _selectedStageSet = GetStageSet(index);
                    ShowStageDefinitions(button.position);
                    button.SetColor(Color.yellow);
                    button.Select();
                }
            },
            Exit: (Button button) => button.SetColor(_stages != null && _selectedStageSet == GetStageSet( index ) ? button.color : Color.white),
            Close: (Button button) =>
            {
                if (button.selected && Input.GetMouseButtonDown(0) && (_stages == null || !_stages.containsMouse))
                {
                    HideStageDefinitions();
                    button.Deselect();
                    _selectedStageSet = null;
                    button.SetColor(Color.white);
                }
            }))));

        stageSets.Add(new Button("Add Stage Set", 4, 1, container, "AddStageSet",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ScriptableObjects.Add(ScriptableObject.CreateInstance<StageSet>(), _editor.stageData);

                    if (selectedStageDefinition != null)
                        ShowStageEditor();

                    ShowStageSets();
                    _editor.missionEditor.ShowMissionEditor();
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)));

        stageSets.Add(new Button("Back to Campaign", 4, 1, container, "BackToCampaign", fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _selectedStageSet = null;
                    selectedStageDefinition = null;
                    _editor.missionEditor.SetSelectedMission(null);
                    _editor.missionEditor.SetSelectedMissionSet(null);

                    _editor.testButton.Disable();
                    _editor.testButton.Hide();
                    _editor.waveEditor.Hide();
                    _editor.stageEditor.Hide();
                    _editor.missionEditor.Hide();
                    _editor.timelineEditor.Hide();
                    _editor.missionEditor.HideMissionEditor();
                    _editor.timelineEditor.HideMissionTimeline();

                    _editor.campaignEditor.ShowCampaignSets();
                    _editor.campaignEditor.ShowCampaignEditor();
                    _editor.campaignMapEditor.ShowCampaignMap();
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)), true);
    }

    public void HideStageSets()
    {
        if (stageSets != null)
            Remove(stageSets);

        stageSets?.Destroy();
        stageSets = null;
    }

    public void ShowStageDefinitions(Vector3 position)
    {
        HideStageDefinitions();
        int count = _selectedStageSet.stageDefinitions.Count;
        Add(_stages = new Layout("StageDefinitionButtons", 4, 1 * (count + 1), 0.25f, 0.1f, count + 1, container));
        _stages.SetPosition(position + (Vector3.left * (stageSets.width - 0.5f)) + (Vector3.back * ((count * 0.5f) + (0.25f * 0.5f))));
        _stages.Add(new List<RenameableDeletableButton>(RenameableDeletableButton.GetButtons(count, (int index) => new RenameableDeletableButton(GetStageDefinition(index).name, 4, 1, container,
            fontSize: 20,
            DeleteStay: (Button b) =>
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _selectedStageSet.Remove(GetStageDefinition(index));
                    ShowStageDefinitions(position);
                }
            },
            EndInput: (Field field) =>
            {
                GetStageDefinition(index).name = field.label.text;
                field.SetColor(Color.white);
            },
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (_selectedStageSet != null && Input.GetMouseButtonDown(0))
                {
                    selectedStageDefinition = GetStageDefinition( index );

                    HideStageSets();
                    HideStageDefinitions();
                    HideStage();

                    SetStageDefinition(selectedStageDefinition);
                    _editor.testButton.Enable();
                    _editor.testButton.Show();
                    ShowStage();

                    ShowStageSets();
                    ShowStageEditor();
                    ShowMissionTimeline();
                    ShowMissionEditor();
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)))));

        _stages.Add(new Button("Add Stage\nDefinition", 4, 1, container, "AddStageDefinition",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (_selectedStageSet != null && Input.GetMouseButtonDown(0))
                {
                    ScriptableObjects.Add(StageDefinition.Default(), _selectedStageSet);
                    ShowStageDefinitions(position);
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)), true);
    }

    public void HideStageDefinitions()
    {
        if (_stages != null)
            Remove(_stages);

        _stages?.Destroy();
        _stages = null;
    }

    public void ShowStageEditor()
    {
        HideStageEditor();
        Add(stageEditorLayout = new Layout( "StageEditor" , 4 , 4 , 0.25f , 0.1f , 5 , container ));
        stageEditorLayout.SetPosition(stageSets.position + (Vector3.back * (stageSets.height + stageEditorLayout.height) * 0.5f));
        stageEditorLayout.Add(new List<Element>()
        {
            new Label( "Lanes:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Lanes" , selectedStageDefinition.laneCount.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                int.TryParse( field.label.text , out selectedStageDefinition.laneCount );
                Refresh();
            } ) ,

            new Label( "Width:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Width" , selectedStageDefinition.width.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.width );
                Refresh();
            } ) ,

            new Label( "Height:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Height" , selectedStageDefinition.height.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.height );
                Refresh();
            } ) ,

            new Label( "Spacing:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Spacing" , selectedStageDefinition.laneSpacing.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.laneSpacing );
                Refresh();
            } ) ,

            new Label( "Speed:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Speed" , selectedStageDefinition.speed.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.speed );
                Refresh();
            } )
        }, true );
    }

    public void HideStageEditor()
    {
        if (stageEditorLayout != null)
            Remove(stageEditorLayout);

        stageEditorLayout?.Destroy();
        stageEditorLayout = null;
    }

    public override void Hide()
    {
        HideStageSets();
        HideStageEditor();
        HideStageDefinitions();
        _selectedStageSet = null;

        if ( _editor.stage != null )
            _editor.HideStage();
    }

    public void SetSelectedStageDefinition( StageDefinition selectedStageDefinition ) => this.selectedStageDefinition = selectedStageDefinition;

    public override void Refresh()
    {
        Hide();
        Show();
    }

    public StageDefinition selectedStageDefinition { get; private set; }
    public Layout stageEditorLayout { get; private set; }
    public Layout stageSets { get; private set; }

    private Editor _editor { get; }
    private StageSet _selectedStageSet { get; set; }
    private Layout _stages { get; set; }

    public StageEditor(Editor editor, GameObject parent = null) : base(typeof(StageEditor).Name, parent)
    {
        _editor = editor;
    }
}
#endif //UNITY_EDITOR