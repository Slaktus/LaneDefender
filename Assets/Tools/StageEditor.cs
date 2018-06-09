#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StageEditor
{
    public void Update() { }

    public void Load() => stageData = AssetDatabase.LoadAssetAtPath<StageData>( stageDataPath + "StageData.asset" );
    private void Create() => stageData = ScriptableObjects.Create<StageData>( stageDataPath + "StageData.asset" );

    public Stage stage { get; private set; }

    private Editor _editor { get; }
    private GameObject _container { get; }
    private StageData stageData { get; set; }
    private const string stageDataPath = "Assets/Data/Stages/";

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

        if ( stageData == null )
            Create();
    }
}

#endif //UNITY_EDITOR