using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEditor
{
    public void Update() { }

    public Stage stage { get; private set; }

    private Editor _editor { get; }
    private GameObject _container { get; }

    public StageEditor( Editor editor , GameObject parent )
    {
        _editor = editor;
        _container = new GameObject( "StageEditor" );
        _container.transform.SetParent( parent.transform );
        stage = new Stage(
            speed: 5 ,
            width: 25 ,
            height: 15 ,
            laneSpacing: 1 ,
            laneCount: 5 ,
            conveyor: null ,
            player: new Player() );
    }
}
