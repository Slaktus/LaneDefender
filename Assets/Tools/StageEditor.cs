#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StageEditor
{
    public void Update()
    {
        stageSetLayout?.Update();
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
                            Refresh();
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        stageDefinitionLayout = new Layout( "StageDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        stageDefinitionLayout.SetParent( _container );
        stageDefinitionLayout.SetPosition( position + ( Vector3.right * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
        stageDefinitionLayout.Add( buttons , true );
        showingStageDefinitions = true;
    }

    public void ShowStageEditor()
    {

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
        HideStageDefinitions();
        selectedStageSet = null;
    }

    public void Refresh()
    {
        Hide();
        Show();
    }

    public Vector3 position => _editor.waveEditorPosition + ( Vector3.back * _editor.waveSetLayoutHeight );

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