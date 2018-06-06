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
                    Enter: ( Button button ) => button.SetColor( selectedWaveSet == waveData.waveSets[ index ] ? button.color : Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedWaveSet != null )
                                buttons[ waveData.waveSets.IndexOf( selectedWaveSet ) + 1 ].SetColor( Color.white );

                            button.SetColor( Color.yellow );
                            selectedWaveSet = waveData.waveSets[ index ];
                            HideWaveDefinitions();
                            ShowWaveDefinitions( button.position , selectedWaveSet.waveDefinitions );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( selectedWaveSet == waveData.waveSets[ index ] ? button.color : Color.white ) ) );
            }

        waveSetLayout = new Layout( "WaveSetButtons" , width , height * buttons.Count , padding , spacing , buttons.Count , waveSetContainer );
        waveSetLayout.SetLocalPosition( localPosition + ( Vector3.back * height * ( buttons.Count - 1 ) * 0.5f ) );
        waveSetLayout.Add( buttons , true );
        showingWaveSets = true;
    }

    public void HideWaveSets()
    {
        showingWaveSets = false;
        waveSetLayout?.Destroy();
    }

    private void ShowWaveDefinitions( Vector3 position , List<WaveDefinition> waveDefinitions , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        showingWaveDefinitions = true;
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveDefinition" , "Add Wave\nDefinition" , width , height , waveDefinitionContainer ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( selectedWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<WaveDefinition>() , selectedWaveSet );
                    HideWaveDefinitions();
                    ShowWaveDefinitions( position , selectedWaveSet.waveDefinitions );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( selectedWaveSet.waveDefinitions != null )
            for ( int i = 0 ; selectedWaveSet.waveDefinitions.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveDefinition" , "Wave Definition" , width , height , waveDefinitionContainer ,
                    fontSize: 20 ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( selectedWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedWaveDefinition != null )
                            {
                                Debug.Log( "hey" );
                                buttons[ selectedWaveSet.waveDefinitions.IndexOf( selectedWaveDefinition ) + 1 ].SetColor( Color.white );
                            }

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

                            HideWaveSets();
                            HideWaveDefinitions();
                            HideWaveEventButtons();
                            ShowWaveEventButtons();
                            editor.waveEditorButton.SetColor( Color.white );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        waveDefinitionLayout.SetParent( waveDefinitionContainer );
        waveDefinitionLayout.SetPosition( position + ( Vector3.right * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
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
            waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Add( new Button( "WaveEvent" + index.ToString() , index.ToString() , 1 , 1 , container ,
                Enter: ( Button butt ) => butt.SetColor( Color.red ) ,
                Stay: ( Button butt ) =>
                {
                    if ( Input.GetMouseButtonUp( 0 ) )
                    {
                        waveEventEditor?.Destroy();

                        List<Element> waveEventEditorButtons = new List<Element>()
                        {
                            new Label("Type:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleRight ) ,
                            new Button( "Type" , "Type" , 1.25f , 0.5f , container ,
                            fontSize: 20 ,
                            Enter: ( Button b ) => b.SetColor( Color.green ) ,
                            Stay: ( Button b ) => { } ,
                            Exit: ( Button b ) => b.SetColor( Color.white ) ) ,

                            new Label( "Delay:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleRight ) ,
                            new Field( "Delay" , selectedWaveDefinition.waveEvents[ index ].delay.ToString() , 3 , 1 , 20 , container , Field.Mode.Numbers  , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].delay ) ) ,

                            new Label("Entry:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleRight ) ,
                            new Field( "Entry" , selectedWaveDefinition.waveEvents[ index ].entryPoint.ToString() , 1.25f , 0.5f , 20 , container , Field.Mode.Numbers , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].entryPoint ) )
                        };

                        waveEventEditor = new Layout( "WaveEventEditor" , 3.5f , 3 , 0.25f , 0.1f , waveEventEditorButtons.Count / 2 , container );
                        waveEventEditor.Add( waveEventEditorButtons , true );
                        waveEventEditor.SetPosition( butt.position + ( Vector3.left * waveEventEditor.width * 0.5f ) );
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


    public void HideWaveDefinitions()
    {
        showingWaveDefinitions = false;
        waveDefinitionLayout?.Destroy();
    }

    public Camera camera { get; }
    public GameObject container { get; }
    public Stage stage { get; private set; }
    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public Layout waveDefinitionLayout { get; private set; }
    public Layout waveEventEditorLayout { get; private set; }
    public bool showingWaveDefinitions { get; private set; }
    public bool showingWaveSets { get; private set; }

    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }

    public GameObject waveSetContainer { get; private set; }
    public GameObject waveDefinitionContainer { get; private set; }

    private const string waveDataPath = "Assets/Data/Waves/";
    private List<Layout> waveEventLayouts { get; }
    private HeldEvent heldWaveEvent { get; set; }
    private Layout waveEventEditor { get; set; }
    private Editor editor { get; }

    public WaveEditor( Editor editor , GameObject parent = null )
    {
        this.editor = editor;
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