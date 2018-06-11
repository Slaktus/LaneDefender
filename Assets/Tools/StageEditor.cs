#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StageEditor
{
    public void Update()
    {
        if ( selectedStageDefinition != null && stageEditorLayout == null )
            ShowStageEditor();

        stageSetLayout?.Update();
        stageEditorLayout?.Update();
        stageDefinitionLayout?.Update();
    }

    public void Load() => _stageData = AssetDatabase.LoadAssetAtPath<StageData>( _stageDataPath + "StageData.asset" );
    private void Create() => _stageData = ScriptableObjects.Create<StageData>( _stageDataPath + "StageData.asset" );

    public void Show() => ShowStageSets();

    private void ShowStageSets( float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddStageSet" , "Add Stage Set" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<StageSet>() , _stageData );
                    _editor.Refresh();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( _stageData.stageSets != null )
            for ( int i = 0 ; _stageData.stageSets.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Dropdown( "StageSet" , "Stage Set" , width , height , _container ,
                    fontSize: 20 ,
                    Enter: ( Button button ) => button.SetColor( selectedStageSet == _stageData.stageSets[ index ] ? button.color : Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedStageSet != null )
                                buttons[ _stageData.stageSets.IndexOf( selectedStageSet ) + 1 ].SetColor( Color.white );

                            button.SetColor( Color.yellow );
                            selectedStageSet = _stageData.stageSets[ index ];

                            HideStageDefinitions();
                            ShowStageDefinitions( button.position );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( selectedStageSet == _stageData.stageSets[ index ] ? button.color : Color.white ) ,
                    Close: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) && !stageSetLayout.containsMouse &&( stageDefinitionLayout == null || !stageDefinitionLayout.containsMouse ) )
                        {
                            button.SetColor( Color.white );
                            selectedStageSet = null;
                            HideStageDefinitions();
                        }
                    } ) );
            }

        stageSetLayout = new Layout( "StageSetButtons" , width , height * buttons.Count , padding , spacing , buttons.Count , _container );
        stageSetLayout.SetLocalPosition( position + ( Vector3.back * height * ( buttons.Count - 1 ) * 0.5f ) );
        stageSetLayout.Add( buttons , true );
    }

    private void ShowStageDefinitions( Vector3 position , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddStageDefinition" , "Add Stage\nDefinition" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( selectedStageSet != null && Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<StageDefinition>() , selectedStageSet );
                    HideStageDefinitions();
                    ShowStageDefinitions( position );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( selectedStageSet != null && selectedStageSet.stageDefinitions != null )
            for ( int i = 0 ; selectedStageSet.stageDefinitions.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "StageDefinition" , "Stage Definition" , width , height , _container ,
                    fontSize: 20 ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( selectedStageSet != null && Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedStageDefinition != null )
                                buttons[ selectedStageSet.stageDefinitions.IndexOf( selectedStageDefinition ) + 1 ].SetColor( Color.white );

                            selectedStageDefinition = selectedStageSet.stageDefinitions[ index ];
                            stage?.Destroy();
                            stage = new Stage( selectedStageDefinition , null , new Player() );
                            Refresh( refreshAll: true );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        stageDefinitionLayout = new Layout( "StageDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        stageDefinitionLayout.SetParent( _container );
        stageDefinitionLayout.SetPosition( position + ( Vector3.left * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
        stageDefinitionLayout.Add( buttons , true );
        showingStageDefinitions = true;
    }

    public void ShowStageEditor()
    {
        List<Element> stageEditorButtons = new List<Element>()
        {
            new Label( "Lanes:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Lanes" , selectedStageDefinition.laneCount.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                int.TryParse( field.label.text , out selectedStageDefinition.laneCount );
                Refresh( refreshAll: true );
            } ) ,

            new Label( "Width:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Width" , selectedStageDefinition.width.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.width );
                Refresh( refreshAll: true );
            } ) ,

            new Label( "Height:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Height" , selectedStageDefinition.height.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.height );
                Refresh( refreshAll: true );
            } ) ,

            new Label( "Spacing:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Spacing" , selectedStageDefinition.laneSpacing.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.laneSpacing );
                Refresh( refreshAll: true );
            } ) ,

            new Label( "Speed:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
            new Field( "Speed" , selectedStageDefinition.speed.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) =>
            {
                float.TryParse( field.label.text , out selectedStageDefinition.speed );
                Refresh( refreshAll: true );
            } )
        };

        stageEditorLayout = new Layout( "StageEditor" , 3 , 4 , 0.25f , 0.1f , stageEditorButtons.Count / 2 , _container );
        stageEditorLayout.Add( stageEditorButtons , true );
        stageEditorLayout.SetPosition( _editor.waveEditorPosition + ( Vector3.back * ( _editor.waveSetLayoutHeight + ( stageEditorLayout.height * 0.5f ) ) ) );
    }

    public void HideStageEditor()
    {
        stageEditorLayout?.Destroy();
        stageEditorLayout = null;
    }

    Layout stageEditorLayout { get; set; }

    public void HideStageDefinitions()
    {
        stageDefinitionLayout?.Destroy();
        showingStageDefinitions = false;
    }

    public void HideStageSets()
    {
        stageSetLayout?.Destroy();
    }

    public void Hide()
    {
        HideStageSets();
        HideStageEditor();
        HideStageDefinitions();
        selectedStageSet = null;

        if ( selectedStageDefinition != null )
        {
            stage.Destroy();
            stage = new Stage( selectedStageDefinition , null , new Player() );
        }
    }

    public void Refresh( bool refreshAll = false )
    {
        if ( refreshAll )
            _editor.Refresh();
        else
        {
            Hide();
            Show();
        }
    }

    public Vector3 position => _editor.testButtonPosition + ( Vector3.left * stageSetLayout.width * 0.5f ) + ( Vector3.right * _editor.testButtonWidth * 0.5f ) + ( Vector3.back * ( ( _editor.testButtonWidth * 0.5f ) + 0.5f ) );

    public Stage stage { get; private set; }
    public Layout stageSetLayout { get; private set; }
    public StageSet selectedStageSet { get; private set; }
    public Layout stageDefinitionLayout { get; private set; }
    public StageDefinition selectedStageDefinition { get; private set; }
    public bool showingStageDefinitions { get; private set; }

    private Editor _editor { get; }
    private GameObject _container { get; }
    private StageData _stageData { get; set; }
    private const string _stageDataPath = "Assets/Data/Stages/";

    public StageEditor( Editor editor , GameObject parent )
    {
        _editor = editor;
        _container = new GameObject( "StageEditor" );
        _container.transform.SetParent( parent.transform );
        Load();

        stage = new Stage(
            speed: 5 ,
            width: 25 ,
            height: 15 ,
            laneSpacing: 1 ,
            laneCount: 5 ,
            conveyor: null ,
            player: new Player() );

        if ( _stageData == null )
            Create();
    }
}
#endif //UNITY_EDITOR