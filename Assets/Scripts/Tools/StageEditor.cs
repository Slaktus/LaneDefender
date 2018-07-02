#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class StageEditor
{
    public void Update()
    {
        _stageSets?.Update();
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
        _stageSets?.Destroy();
        int count = _editor.stageData.stageSets.Count + 1;
        _stageSets = new Layout( "StageSetButtons" , width , height * count , padding , spacing , count , _container );

        List<Button> buttons = new List<Button>( count )
        {
            new Button( "AddStageSet" , "Add Stage Set" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<StageSet>() , _editor.stageData );
                    Refresh();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) )
        };

        for ( int i = 0 ; buttons.Capacity - 1 > i ; i++ )
        {
            int index = i;
            buttons.Add( new Button( "StageSet" , "Stage Set" , width , height , _container ,
                fontSize: 20 ,
                Enter: ( Button button ) => button.SetColor( _stages != null && _selectedStageSet == _editor.stageData.stageSets[ index ] ? button.color : Color.green ) ,
                Stay: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        if ( _selectedStageSet != null )
                            buttons[ _editor.stageData.stageSets.IndexOf( _selectedStageSet ) + 1 ].SetColor( Color.white );

                        button.Select();
                        button.SetColor( Color.yellow );
                        _selectedStageSet = _editor.stageData.stageSets[ index ];

                        HideStageDefinitions();
                        ShowStageDefinitions( button.position );
                    }
                } ,
                Exit: ( Button button ) => button.SetColor( _stages != null && _selectedStageSet == _editor.stageData.stageSets[ index ] ? button.color : Color.white ) ,
                Close: ( Button button ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) && ( _stageSets == null || !_stageSets.containsMouse ) && ( _stages == null || !_stages.containsMouse ) )
                    {
                        button.Deselect();
                        button.SetColor( Color.white );
                        _selectedStageSet = null;
                        HideStageDefinitions();
                    }
                } ) );
        }

        _stageSets.Add( buttons , true );
        _stageSets.SetViewportPosition( new Vector2( 0 , 1 ) );
    }

    private void ShowStageDefinitions( Vector3 position , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddStageDefinition" , "Add Stage\nDefinition" , width , height , _container ,
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
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( _selectedStageSet != null && _selectedStageSet.stageDefinitions != null )
            for ( int i = 0 ; _selectedStageSet.stageDefinitions.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "StageDefinition" , "Stage Definition" , width , height , _container ,
                    fontSize: 20 ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( _selectedStageSet != null && Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedStageDefinition != null )
                                buttons[ _selectedStageSet.stageDefinitions.IndexOf( selectedStageDefinition ) + 1 ].SetColor( Color.white );

                            selectedStageDefinition = _selectedStageSet.stageDefinitions[ index ];

                            HideStageSets();
                            HideStageDefinitions();
                            _editor.HideStage();

                            _editor.missionEditor.selectedMission.stageDefinition = selectedStageDefinition;
                            _editor.ShowStage( selectedStageDefinition );
                            _editor.missionEditor.ShowMissionTimeline();
                            ShowStageSets();
                            ShowStageEditor();
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        _stages = new Layout( "StageDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        _stages.SetParent( _container );
        _stages.SetPosition( position + ( Vector3.right * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
        _stages.Add( buttons , true );
        _showingStageDefinitions = true;
    }

    public void ShowStageEditor()
    {
        List<Element> stageEditorButtons = new List<Element>()
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
        };

        stageEditorLayout = new Layout( "StageEditor" , 3 , 4 , 0.25f , 0.1f , stageEditorButtons.Count / 2 , _container );
        stageEditorLayout.Add( stageEditorButtons , true );
        stageEditorLayout.SetPosition( _stageSets.position + ( Vector3.back * ( _stageSets.height + stageEditorLayout.height ) * 0.5f ) );
    }

    public void HideStageEditor()
    {
        stageEditorLayout?.Destroy();
        stageEditorLayout = null;
    }

    Layout stageEditorLayout { get; set; }

    public void HideStageDefinitions()
    {
        _stages?.Destroy();
        _showingStageDefinitions = false;
    }

    public void HideStageSets()
    {
        _stageSets?.Destroy();
    }

    public void Hide()
    {
        HideStageSets();
        HideStageEditor();
        HideStageDefinitions();
        _selectedStageSet = null;

        if ( selectedStageDefinition != null )
        {
            _editor.HideStage();
            _editor.ShowStage( selectedStageDefinition );
        }
    }

    public void Refresh()
    {
        Hide();
        Show();
    }

    public StageDefinition selectedStageDefinition { get; private set; }

    private Editor _editor { get; }
    private GameObject _container { get; }
    private Layout _stageSets { get; set; }
    private StageSet _selectedStageSet { get; set; }
    private Layout _stages { get; set; }
    private bool _showingStageDefinitions { get; set; }

    public StageEditor( Editor editor )
    {
        _editor = editor;
        _container = new GameObject( "StageEditor" );
        _container.transform.SetParent( editor.container.transform );
    }
}
#endif //UNITY_EDITOR