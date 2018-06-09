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
        waveSetLayout?.Update();
        waveEventEditor?.Update();
        waveDefinitionLayout?.Update();

        for ( int i = 0 ; waveEventLayouts.Count > i ; i++ )
            waveEventLayouts[ i ].Update();
    }

    public void Load() => waveData = AssetDatabase.LoadAssetAtPath<WaveData>( waveDataPath + "WaveData.asset" );
    private void Create() => waveData = ScriptableObjects.Create<WaveData>( waveDataPath + "WaveData.asset" );

    public void Show( Vector3 localPosition ) => ShowWaveSets( localPosition );

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
                buttons.Add( new Dropdown( "WaveSet" , "Wave Set" , width , height , waveSetContainer ,
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

    private void ShowWaveDefinitions( Vector3 position , List<WaveDefinition> waveDefinitions , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
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
                                buttons[ selectedWaveSet.waveDefinitions.IndexOf( selectedWaveDefinition ) + 1 ].SetColor( Color.white );

                            selectedWaveDefinition = selectedWaveSet.waveDefinitions[ index ];

                            HideWaveSets();
                            HideWaveDefinitions();
                            HideWaveEventButtons();
                            ShowWaveEventButtons();
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        waveDefinitionLayout.SetParent( waveDefinitionContainer );
        waveDefinitionLayout.SetPosition( position + ( Vector3.right * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
        waveDefinitionLayout.Add( buttons , true );
        showingWaveDefinitions = true;
    }

    public void ShowWaveEventButtons()
    {
        List<List<Button>> waveEventButtons = new List<List<Button>>();

        for ( int i = 0 ; editor.stage.lanes > i ; i++ )
            waveEventButtons.Add( new List<Button>() );

        for ( int i = 0 ; selectedWaveDefinition.waveEvents.Count > i ; i++ )
        {
            int index = i;
            waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Add( new Dropdown( "WaveEvent" + index.ToString() , index.ToString() , 1 , 1 , container ,
                Enter: ( Button butt ) => butt.SetColor( Color.red ) ,
                Stay: ( Button butt ) =>
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        if ( waveEventEditor != null )
                        {
                            for ( int j = 0 ; waveEventButtons.Count > j ; j++ )
                                for ( int k = 0 ; waveEventButtons[ j ].Count > k ; k++ )

                                if ( ( waveEventButtons[ j ][ k ] as Dropdown ).HasLayout( waveEventEditor ) )
                                {
                                    ( waveEventButtons[ j ][ k ] as Dropdown ).RemoveLayout( waveEventEditor );
                                    waveEventEditor.Destroy();
                                    waveEventEditor = null;
                                }
                        }

                        List<Element> waveEventEditorButtons = new List<Element>()
                        {
                            new Label("Type:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleRight ) ,
                            new Dropdown( "Type" , WaveEvent.Type.SpawnEnemy.ToString() , 2 , 0.5f , container ,
                                fontSize: 20 ,
                                Enter: ( Button b ) => b.SetColor( Color.green ) ,
                                Stay: ( Button b ) => { } ,
                                Exit: ( Button b ) => b.SetColor( Color.white ) ) ,

                            new Label( "Delay:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleRight ) ,
                            new Field( "Delay" , selectedWaveDefinition.waveEvents[ index ].delay.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].delay ) ) ,

                            new Label("Entry:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleRight ) ,
                            new Field( "Entry" , selectedWaveDefinition.waveEvents[ index ].entryPoint.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].entryPoint ) )
                        };

                        waveEventEditor = new Layout( "WaveEventEditor" , 3.5f , 3 , 0.25f , 0.1f , waveEventEditorButtons.Count / 2 , container );
                        waveEventEditor.Add( waveEventEditorButtons , true );
                        waveEventEditor.SetPosition( editor.stage.LaneBy( selectedWaveDefinition.waveEvents[ index ].lane ).start + ( Vector3.left * waveEventEditor.width * 0.5f ) );
                        ( butt as Dropdown ).AddLayout( waveEventEditor );

                        for ( int j = 0 ; waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Count > j ; j++ )
                            waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ][ j ].Hide();
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
                } , 
                Close: ( Button butt ) => 
                {
                    if ( Input.GetMouseButtonDown( 0 ) && ( butt as Dropdown ).HasLayout( waveEventEditor ) )
                    {
                        ( butt as Dropdown ).RemoveLayout( waveEventEditor );
                        waveEventEditor?.Destroy();
                        waveEventEditor = null;

                        for ( int j = 0 ; waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Count > j ; j++ )
                            waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ][ j ].Show();
                    }
                } ) );
        }

        for ( int i = 0 ; waveEventButtons.Count > i ; i++ )
        {
            Layout layout = new Layout( "WaveEventLayout" , waveEventButtons[ i ].Count , 1 , 0.25f , 0.1f , 1 , container );
            layout.SetLocalPosition( editor.stage.LaneBy( i ).start + ( Vector3.left * layout.width * 0.5f ) );
            layout.Add( waveEventButtons[ i ] , true );
            waveEventLayouts.Add( layout );
            layout.Refresh();
        }
    }

    public void HideWaveEventButtons()
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

    public void HideWaveSets()
    {
        selectedWaveSet = null;
        showingWaveSets = false;
        waveSetLayout?.Destroy();
    }

    public void Hide()
    {
        HideWaveEventButtons();
        HideWaveDefinitions();
        HideWaveSets();
    }

    public GameObject container { get; }
    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public Layout waveEventEditor { get; private set; }
    public Layout waveDefinitionLayout { get; private set; }
    public Layout waveEventEditorLayout { get; private set; }
    public bool showingWaveDefinitions { get; private set; }
    public bool showingWaveSets { get; private set; }

    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }

    public HeldEvent heldWaveEvent { get; set; }
    public GameObject waveSetContainer { get; private set; }
    public GameObject waveDefinitionContainer { get; private set; }

    private const string waveDataPath = "Assets/Data/Waves/";
    private List<Layout> waveEventLayouts { get; }
    private Editor editor { get; }

    public WaveEditor( Editor editor , GameObject parent = null )
    {
        this.editor = editor;
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