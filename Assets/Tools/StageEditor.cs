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
                    Enter: ( Button button ) => button.SetColor( selectedWaveSet == _stageData.stageSets[ index ] ? button.color : Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedWaveSet != null )
                                buttons[ _stageData.stageSets.IndexOf( selectedWaveSet ) + 1 ].SetColor( Color.white );

                            button.SetColor( Color.yellow );
                            selectedWaveSet = _stageData.stageSets[ index ];
                            //HideWaveDefinitions();
                            //ShowWaveDefinitions( button.position , selectedWaveSet.waveDefinitions );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( selectedWaveSet == _stageData.stageSets[ index ] ? button.color : Color.white ) ,
                    Close: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) && !stageSetLayout.containsMouse /*&&( waveDefinitionLayout == null || !waveDefinitionLayout.containsMouse )*/ )
                        {
                            button.SetColor( Color.white );
                            selectedWaveSet = null;
                            //HideWaveDefinitions();
                        }
                    } ) );
            }

        stageSetLayout = new Layout( "StageSetButtons" , width , height * buttons.Count , padding , spacing , buttons.Count , _container );
        stageSetLayout.SetLocalPosition( position + ( Vector3.back * height * ( buttons.Count - 1 ) * 0.5f ) );
        stageSetLayout.Add( buttons , true );
    }

    public void HideStageSets()
    {
        selectedWaveSet = null;
        stageSetLayout?.Destroy();
    }

    public void Hide()
    {
        HideStageSets();
    }

    public void Refresh()
    {
        Hide();
        Show();
    }

    public Vector3 position => _editor.waveEditorPosition + ( Vector3.back * _editor.waveSetLayoutHeight );

    public Stage stage { get; private set; }
    public Layout stageSetLayout { get; private set; }
    public StageSet selectedWaveSet { get; private set; }

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