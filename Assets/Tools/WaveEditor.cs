#if UNITY_EDITOR
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
        RaycastHit[] hits = Physics.RaycastAll( mouseRay.origin , mouseRay.direction , float.PositiveInfinity );
        Lane hoveredLane = null;

        if ( stage != null && hits.Length > 0 )
        {
            Vector3 mousePosition = hits[ 0 ].point;
            hoveredLane = stage.GetHoveredLane( mousePosition );
            stage.SetLaneColor( Color.black );

            if ( hoveredLane != null )
            {
                hoveredLane.color = Color.yellow;

                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    int index = stage.IndexOf( hoveredLane );
                    WaveEventDefinition waveEventDefinition = ScriptableObject.CreateInstance<WaveEventDefinition>();
                    waveEventDefinition.Initialize( 0 , index , WaveEvent.Type.SpawnEnemy );
                    ScriptableObjects.Add( waveEventDefinition , selectedWaveDefinition );
                    HideWaveEventButtons();
                    ShowWaveEventButtons();
                }
            }

            if ( heldWaveEvent != null )
            {
                heldWaveEvent.SetPosition( mousePosition );

                if ( Input.GetMouseButtonUp( 0 ) )
                {
                    if ( hoveredLane != null )
                    {
                        heldWaveEvent.waveEventDefinition.SetLane( stage.IndexOf( hoveredLane ) );
                        HideWaveEventButtons();
                        ShowWaveEventButtons();
                    }

                    heldWaveEvent.Destroy();
                    heldWaveEvent = null;
                }
            }
        }

        stage?.Update();
        waveSetLayout?.Update();
        waveEventEditor?.Update();
        waveDefinitionLayout?.Update();

        for ( int i = 0 ; waveEventLayouts.Count > i ; i++ )
            waveEventLayouts[ i ].Update();
    }

    public void Load() => waveData = AssetDatabase.LoadAssetAtPath<WaveData>( waveDataPath + "WaveData.asset" );
    public void Save() => AssetDatabase.SaveAssets();

    private void Create()
    {
        waveData = ScriptableObject.CreateInstance<WaveData>();
        AssetDatabase.CreateAsset( waveData , waveDataPath + "WaveData.asset" );
        AssetDatabase.SaveAssets();
    }

    public void Show( Vector3 localPosition ) => ShowWaveSets( localPosition );
    public void Hide()
    {
        stage?.Destroy();
        HideWaveEventButtons();
        HideWaveDefinitions();
        HideWaveSets();
    }

    private void ShowWaveSets( Vector3 localPosition , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveSet" , "Add Wave Set" , width , height , waveSetContainer , 
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<WaveSet>() , waveData );
                    HideWaveSets();
                    ShowWaveSets( localPosition );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( waveData.waveSets != null )
            for ( int i = 0 ; waveData.waveSets.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveSet" , "Wave Set" , width , height , waveSetContainer , 
                    fontSize: 20 ,
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

        waveSetLayout = new Layout( "WaveSetButtons" , width , height * buttons.Count , padding , spacing , buttons.Count , waveSetContainer );
        waveSetLayout.SetLocalPosition( localPosition + ( Vector3.back * height * ( buttons.Count - 1 ) * 0.5f ) );
        waveSetLayout.Add( buttons , true );        
    }

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
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<WaveDefinition>() , selectedWaveSet );
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
                                spacing: 1 ,
                                laneCount: 5 ,
                                conveyor: null ,
                                player: new Player() );

                            HideWaveEventButtons();
                            ShowWaveEventButtons();
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , 10 , 3 * buttons.Count , 1 , 0.1f , buttons.Count );
        waveDefinitionLayout.SetParent( waveDefinitionContainer );

        waveDefinitionLayout.SetLocalPosition( Camera.main.ViewportToWorldPoint( new Vector3( 0 , 1 , Camera.main.transform.position.y ) ) + new Vector3( 15 , 0 , -buttons.Count * 3 * 0.5f ) );
        waveDefinitionLayout.Add( buttons , true );
    }

    private void ShowWaveEventButtons()
    {
        List<List<Button>> waveEventButtons = new List<List<Button>>();

        for ( int i = 0 ; stage.lanes > i ; i++ )
            waveEventButtons.Add( new List<Button>() );

        for ( int i = 0 ; selectedWaveDefinition.waveEvents.Count > i ; i++ )
        {
            int index = i;
            waveEventButtons[ selectedWaveDefinition.waveEvents[ i ].lane ].Add( new Button( "WaveEvent" + i.ToString() , i.ToString() , 1 , 1 , container ,
                Enter: ( Button butt ) => butt.SetColor( Color.red ) ,
                Stay: ( Button butt ) =>
                {
                    if ( Input.GetMouseButtonUp( 0 ) )
                    {
                        waveEventEditor?.Destroy();

                        List<Element> waveEventEditorButtons = new List<Element>()
                        {
                            new Label("Type:" , Color.black , 1 , 1 , container) ,
                            new Button( "TypeButton" , "Type" , 5 , 3 , container ,
                            Enter: ( Button b ) => b.SetColor( Color.green ) ,
                            Stay: ( Button b ) => { } ,
                            Exit: ( Button b ) => b.SetColor( Color.white ) ) ,

                            new Label("Delay:" , Color.black , 1 , 1 , container) ,
                            new Field( "DelayButton" , selectedWaveDefinition.waveEvents[ index ].delay.ToString() , 5 , 3 , container , Field.Mode.Numbers  , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].delay ) ) ,

                            new Label("Entry Point:" , Color.black , 1 , 1 , container) ,
                            new Field( "EntryButton" , selectedWaveDefinition.waveEvents[ index ].entryPoint.ToString() , 5 , 3 , container , Field.Mode.Numbers , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].entryPoint ) )
                        };

                        waveEventEditor = new Layout( "WaveEventEditor" , 5 , waveEventEditorButtons.Count * 3 , 1 , 0.1f , waveEventEditorButtons.Count / 2 , container );
                        waveEventEditor.Add( waveEventEditorButtons , true );
                    }
                } ,
                Exit: ( Button butt ) =>
                {
                    if ( heldWaveEvent == null && Input.GetMouseButton( 0 ) )
                    {
                        heldWaveEvent = new HeldEvent( butt.rect.position , selectedWaveDefinition.waveEvents[ index ] , selectedWaveDefinition.waveEvents[ index ].lane );
                        heldWaveEvent.SetText( index.ToString() );
                    }

                    butt.SetColor( Color.white );
                } ) );
        }

        for ( int i = 0 ; waveEventButtons.Count > i ; i++ )
        {
            Layout layout = new Layout( "WaveEventLayout" , waveEventButtons[ i ].Count , 1 , 0 , 0.1f , 1 );
            layout.SetLocalPosition( stage.LaneBy( i ).start );
            layout.Add( waveEventButtons[ i ] , true );
            waveEventLayouts.Add( layout );
        }
    }

    
    private void HideWaveEventButtons()
    {
        for ( int i = 0 ; waveEventLayouts.Count > i ; i++ )
            waveEventLayouts[ i ].Destroy();

        waveEventLayouts.Clear();
    }


    private void HideWaveDefinitions() => waveDefinitionLayout?.Destroy();

    public Camera camera { get; }
    public GameObject container { get; }
    public Stage stage { get; private set; }
    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public Layout waveDefinitionLayout { get; private set; }
    public Layout waveEventEditorLayout { get; private set; }

    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }

    public GameObject waveSetContainer { get; private set; }
    public GameObject waveDefinitionContainer { get; private set; }

    private const string waveDataPath = "Assets/Data/Waves/";
    private List<Layout> waveEventLayouts { get; }
    private HeldEvent heldWaveEvent { get; set; }
    private Layout waveEventEditor { get; set; }

    public WaveEditor( GameObject parent = null )
    {
        camera = Camera.main;
        container = new GameObject( "WaveEditor" );
        waveSetContainer = new GameObject( "WaveSetContainer" );
        waveDefinitionContainer = new GameObject( "WaveDefinitionContainer" );
        waveSetContainer.transform.SetParent( container.transform );
        waveDefinitionContainer.transform.SetParent( container.transform );

        if ( parent != null )
            container.transform.SetParent( parent.transform );

        waveEventLayouts = new List<Layout>();
        Load();

        if ( waveData == null )
            Create();
    }
}
#endif //UNITY_EDITOR