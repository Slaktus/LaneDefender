#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class StageEditor
{
    public void Update()
    {
        stageSets?.Update();
        stageEditorLayout?.Update();
        _stages?.Update();
    }

    public void Show()
    {
        ShowStageSets();

        if ( selectedStageDefinition != null )
            ShowStageEditor();
    }

    private void ShowStageSets( float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        HideStageSets();
        int count = _editor.stageData.stageSets.Count;
        stageSets = new Layout( "StageSetButtons" , width , height * (count + 2), padding , spacing , count + 2, _container );
        stageSets.SetViewportPosition(new Vector2(0, 1));
        stageSets.SetPosition(stageSets.position + Vector3.up);
        stageSets.Add(new List<Button>(Button.GetButtons(count, (int index) => new Button("StageSet", "Stage Set", width, height, _container,
        fontSize: 20,
        Enter: (Button button) => button.SetColor(_stages != null && _selectedStageSet == _editor.stageData.stageSets[ index ] ? button.color : Color.green),
        Stay: (Button button) =>
        {
            if (Input.GetMouseButtonDown(0))
            {

                button.Select();
                button.SetColor(Color.yellow);
                _selectedStageSet = _editor.stageData.stageSets[ index ];

                HideStageDefinitions();
                ShowStageDefinitions(button.position);
            }
        },
        Exit: (Button button) => button.SetColor(_stages != null && _selectedStageSet == _editor.stageData.stageSets[ index ] ? button.color : Color.white),
        Close: (Button button) =>
        {
            if (Input.GetMouseButtonDown(0) && (stageSets == null || !stageSets.containsMouse) && (_stages == null || !_stages.containsMouse))
            {
                button.Deselect();
                button.SetColor(Color.white);
                _selectedStageSet = null;
                HideStageDefinitions();
            }
        })))
        {
            new Button( "AddStageSet" , "Add Stage Set" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<StageSet>() , _editor.stageData );

                    ShowStageSets();
                    ShowStageEditor();
                    _editor.missionEditor.ShowMissionEditor();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) ,

            new Button( "BackToCampaign" , "Back to Campaign" , width , height , _container , fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    _editor.waveEditor.Hide();
                    _editor.stageEditor.Hide();
                    _editor.missionEditor.Hide();
                    _editor.timelineEditor.Hide();
                    _editor.missionEditor.HideMissionEditor();
                    _editor.timelineEditor.HideMissionTimeline();

                    _editor.campaignEditor.ShowCampaignSets();
                    _editor.campaignEditor.ShowCampaignEditor();
                    _editor.ShowCampaignMap();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) )
        }, true );
    }

    private void ShowStageDefinitions(Vector3 position, float width = 3, float height = 1, float padding = 0.25f, float spacing = 0.1f)
    {
        HideStageDefinitions();
        int count = _selectedStageSet.stageDefinitions.Count;
        _stages = new Layout("StageDefinitionButtons", width, height * (count + 1), padding, spacing, count + 1, _container);
        _stages.SetPosition(position + (Vector3.right * width) + (Vector3.back * ((height * (count ) * 0.5f) + (padding * 0.5f))));
        _stages.Add(new List<Button>(Button.GetButtons(count, (int index) => new Button("StageDefinition", "Stage Definition", width, height, _container,
                 fontSize: 20,
                 Enter: (Button button) => button.SetColor(Color.green),
                 Stay: (Button button) =>
                 {
                     if (_selectedStageSet != null && Input.GetMouseButtonDown(0))
                     {
                         selectedStageDefinition = _selectedStageSet.stageDefinitions[ index ];

                         HideStageSets();
                         HideStageDefinitions();
                         _editor.HideStage();

                         _editor.missionEditor.selectedMission.stageDefinition = selectedStageDefinition;
                         _editor.ShowStage(selectedStageDefinition);

                         ShowStageSets();
                         ShowStageEditor();
                         _editor.timelineEditor.ShowMissionTimeline();
                         _editor.missionEditor.ShowMissionEditor();

                     }
                 },
                 Exit: (Button button) => button.SetColor(Color.white))))
        {
            new Button( "AddStageDefinition" , "Add Stage\nDefinition" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( _selectedStageSet != null && Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( StageDefinition.Default() , _selectedStageSet );
                    HideStageDefinitions();
                    ShowStageDefinitions( position );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) )
        }, true);
    }

    public void ShowStageEditor()
    {
        HideStageEditor();
        stageEditorLayout = new Layout( "StageEditor" , 3 , 4 , 0.25f , 0.1f , 5 , _container );
        stageEditorLayout.SetPosition(stageSets.position + (Vector3.back * (stageSets.height + stageEditorLayout.height) * 0.5f));
        stageEditorLayout.Add(new List<Element>()
        {
            new Label( "Lanes:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Lanes" , selectedStageDefinition.laneCount.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                int.TryParse( field.label.text , out selectedStageDefinition.laneCount );
                Refresh();
            } ) ,

            new Label( "Width:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Width" , selectedStageDefinition.width.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.width );
                Refresh();
            } ) ,

            new Label( "Height:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Height" , selectedStageDefinition.height.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.height );
                Refresh();
            } ) ,

            new Label( "Spacing:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Spacing" , selectedStageDefinition.laneSpacing.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.laneSpacing );
                Refresh();
            } ) ,

            new Label( "Speed:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Speed" , selectedStageDefinition.speed.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.speed );
                Refresh();
            } )
        }, true );
    }

    public void HideStageEditor()
    {
        stageEditorLayout?.Destroy();
        stageEditorLayout = null;
    }

    public void HideStageDefinitions()
    {
        _stages?.Destroy();
        _stages = null;
    }

    public void HideStageSets()
    {
        stageSets?.Destroy();
    }

    public void Hide()
    {
        HideStageSets();
        HideStageEditor();
        HideStageDefinitions();
        _selectedStageSet = null;

        if ( selectedStageDefinition != null )
            _editor.HideStage();
    }

    public void SetSelectedStageDefinition( StageDefinition selectedStageDefinition ) => this.selectedStageDefinition = selectedStageDefinition;

    public void Refresh()
    {
        Hide();
        Show();
    }

    public StageDefinition selectedStageDefinition { get; private set; }
    public Layout stageEditorLayout { get; private set; }
    public Layout stageSets { get; private set; }

    private Editor _editor { get; }
    private GameObject _container { get; }
    private StageSet _selectedStageSet { get; set; }
    private Layout _stages { get; set; }

    public StageEditor( Editor editor )
    {
        _editor = editor;
        _container = new GameObject( "StageEditor" );
        _container.transform.SetParent( editor.container.transform );
    }
}
#endif //UNITY_EDITOR