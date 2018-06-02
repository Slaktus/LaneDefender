using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class WaveEditor
{
    public void Update()
    {
        Ray mouseRay = camera.ScreenPointToRay( Input.mousePosition );

        //The actual raycast returns an array with all the targets the ray passed through
        //Note that we don't pass in the ray itself -- that's because the method taking a ray as argument flat-out doesn't work
        //We don't bother constraining the raycast by layer mask just yet, since the ground plane is the only collider in the scene
        RaycastHit[] hits = Physics.RaycastAll( mouseRay.origin , mouseRay.direction , float.PositiveInfinity );

        //These references might be populated later
        Lane hoveredLane = null;

        //Proceed if we hit the ground plane
        if ( stage != null && hits.Length > 0 )
        {            
            //Get the mouse position on the ground plane
            Vector3 mousePosition = hits[ 0 ].point;

            hoveredLane = stage.GetHoveredLane( mousePosition );
            stage.SetLaneColor( Color.black );

            if ( hoveredLane != null )
                hoveredLane.color = Color.yellow;
        }

        stage?.Update();
        waveSetLayout?.Update();
        waveDefinitionLayout?.Update();
    }

    public void Load() => waveData = AssetDatabase.LoadAssetAtPath<WaveData>( waveDataPath + "WaveData.asset" );
    public void Save() => AssetDatabase.SaveAssets();

    private void Create()
    {
        waveData = ScriptableObject.CreateInstance<WaveData>();
        AssetDatabase.CreateAsset( waveData , waveDataPath + "WaveData.asset" );
        AssetDatabase.SaveAssets();
    }

    private void ShowWaveSets()
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveSet" , "Add Wave Set" , 10 , 3 , waveSetContainer ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    waveData.AddWaveSet();
                    HideWaveSets();
                    ShowWaveSets();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( waveData.waveSets != null )
            for ( int i = 0 ; waveData.waveSets.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveSet" , "Wave Set" , 10 , 3 , waveSetContainer ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            selectedWaveSet = waveData.waveSets[ index ];
                            HideWaveDefinitions();
                            ShowWaveDefinitions( selectedWaveSet.waveDefinitions );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveSetLayout = new Layout( "WaveSetButtons" , 10 , 3 * buttons.Count , 1 , 0.1f , buttons.Count );
        waveSetLayout.SetLocalPosition( Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ) + new Vector3( 5 , 0 , -buttons.Count * 3 * 0.5f ) );
        waveSetLayout.Add( buttons , true );        
    }

    Session session { get; set; }

    private void HideWaveSets() => waveSetLayout?.Destroy();

    private void ShowWaveDefinitions( List<WaveDefinition> waveDefinitions )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveDefinition" , "Add Wave\nDefinition" , 10 , 3 , waveDefinitionContainer ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( selectedWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                {
                    selectedWaveSet.AddWaveDefinition();
                    HideWaveDefinitions();
                    ShowWaveDefinitions( selectedWaveSet.waveDefinitions );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( selectedWaveSet.waveDefinitions != null )
            for ( int i = 0 ; selectedWaveSet.waveDefinitions.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveDefinition" , "Wave Definition" , 10 , 3 , waveDefinitionContainer ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( selectedWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                        {
                            selectedWaveDefinition = selectedWaveSet.waveDefinitions[ index ];
                            stage?.Destroy();

                            stage = new Stage(
                                speed: 5 ,
                                width: 25 ,
                                height: 15 ,
                                laneSpacing: 1 ,
                                laneCount: 5 ,
                                conveyor: null ,
                                player: new Player() );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , 10 , 3 * buttons.Count , 1 , 0.1f , buttons.Count );
        waveDefinitionLayout.SetLocalPosition( Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ) + new Vector3( 15 , 0 , -buttons.Count * 3 * 0.5f ) );
        waveDefinitionLayout.Add( buttons , true );
    }


    private void HideWaveDefinitions() => waveDefinitionLayout?.Destroy();

    public Stage stage { get; private set; }
    public GameObject ground { get; }
    public GameObject container { get; }
    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public Layout waveDefinitionLayout { get; private set; }

    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }
    public GameObject waveSetContainer { get; private set; }
    public GameObject waveDefinitionContainer { get; private set; }
    public Camera camera { get; }

    private const string waveDataPath = "Assets/Data/Waves/";

    public WaveEditor()
    {
        camera = Camera.main;
        container = new GameObject( "Container" );
        waveSetContainer = new GameObject( "WaveSetContainer" );
        waveDefinitionContainer = new GameObject( "WaveDefinitionContainer" );

        waveSetContainer.transform.SetParent( container.transform );
        waveDefinitionContainer.transform.SetParent( container.transform );

        Load();

        if ( waveData == null )
            Create();

        ShowWaveSets();

        ground = GameObject.CreatePrimitive( PrimitiveType.Cube );

        //Project the corners of the screen to the ground plane to find out how large the ground plane needs to be to fill the camera's field of view
        Vector3 bottomLeft = camera.ScreenToWorldPoint( new Vector3( 0 , 0 , camera.transform.position.y ) );
        Vector3 topRight = camera.ScreenToWorldPoint( new Vector3( Screen.width , Screen.height , camera.transform.position.y ) );

        //Transforms give GameObjects' positions, rotations and scales
        ground.transform.localScale = new Vector3( topRight.x - bottomLeft.x , 1 , topRight.z - bottomLeft.z );
        ground.transform.position = new Vector3( 25 * 0.5f , -1 , ( -15 * 0.5f ) - 1 * 0.5f );
        ground.name = "Ground";

        //Disable the ground mesh renderer -- we don't want to see the cube
        //GetComponent lets us fetch references to components attached to GameObjects in a scene
        ground.GetComponent<MeshRenderer>().enabled = false;
    }
}