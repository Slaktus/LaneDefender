#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionEditor
{
    public void Update()
    {
        missionTimeline.Update();
        _indicator.transform.position = _mousePos;
    }

    public void ShowMissionTimeline()
    {
        missionTimeline?.Destroy();

        missionTimeline = new Button( "MissionTimeline" , string.Empty , stage.width , 1 , container ,
            Enter: ( Button button ) => ShowIndicator() ,
            Stay: ( Button button ) => 
            {
                if ( Input.GetMouseButton( 0 ) )
                {
                    //ok, so now we pretty much just need to insert wave sets here ...
                    //we don't have anything to do that do we
                    //don't think so no

                    //probably time to do the whole dataflow thing now ...
                    //lists of items in dropdowns is pretty solved
                    Debug.Log( Helpers.Normalize( _indicator.transform.position.x , button.rect.xMax , button.rect.xMin ) );
                }
            } ,
            Exit: ( Button button ) => HideIndicator() );

        missionTimeline.SetPosition( new Vector3( stage.start + ( stage.width * 0.5f ) , 0 , Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ).z ) + ( Vector3.back * missionTimeline.height * 0.5f ) );
    }

    private void ShowIndicator() => _indicator.enabled = true;
    private void HideIndicator() => _indicator.enabled = false;

    public Button missionTimeline { get; private set; }
    public Stage stage { get; private set; }
    public GameObject container { get; }

    private MeshRenderer _indicator;
    private Vector3 _mousePos => Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

    public MissionEditor()
    {
        container = new GameObject( "MissionEditor" );
        stage = new Stage(
            speed: 5 ,
            width: 25 ,
            height: 15 ,
            laneSpacing: 1 ,
            laneCount: 5 ,
            conveyor: null ,
            player: new Player() );

        _indicator = GameObject.CreatePrimitive( PrimitiveType.Sphere ).GetComponent<MeshRenderer>();
        ShowMissionTimeline();
        HideIndicator();
    }
}
#endif //UNITY_EDITOR

