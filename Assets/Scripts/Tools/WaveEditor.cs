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

        for ( int i = 0 ; _waveEventLayouts.Count > i ; i++ )
            _waveEventLayouts[ i ].Update();
    }

    public void Load() => waveData = AssetDatabase.LoadAssetAtPath<WaveData>( _waveDataPath + "WaveData.asset" );
    private void Create() => waveData = ScriptableObjects.Create<WaveData>( _waveDataPath + "WaveData.asset" );

    public void Show()
    {
        ShowWaveSets();

        if ( selectedWaveDefinition != null )
            ShowWaveEventButtons();
    }

    private void ShowWaveSets( float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveSet" , "Add Wave Set" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<WaveSet>() , waveData );
                    _editor.Refresh();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( waveData.waveSets != null )
            for ( int i = 0 ; waveData.waveSets.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Dropdown( "WaveSet" , "Wave Set" , width , height , _container ,
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
                            ShowWaveDefinitions( button.position );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( selectedWaveSet == waveData.waveSets[ index ] ? button.color : Color.white ) ,
                    Close: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) && !waveSetLayout.containsMouse && ( waveDefinitionLayout == null || !waveDefinitionLayout.containsMouse )  )
                        {
                            button.SetColor( Color.white );
                            selectedWaveSet = null;
                            HideWaveDefinitions();
                        }
                    } ) );
            }

        waveSetLayout = new Layout( "WaveSetButtons" , width , height * buttons.Count , padding , spacing , buttons.Count , _container );
        waveSetLayout.SetLocalPosition( position + ( Vector3.back * height * ( buttons.Count - 1 ) * 0.5f ) );
        waveSetLayout.Add( buttons , true );
    }

    private void ShowWaveDefinitions( Vector3 position , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        List<Button> buttons = new List<Button>();

        buttons.Add( new Button( "AddWaveDefinition" , "Add Wave\nDefinition" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( selectedWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<WaveDefinition>() , selectedWaveSet );
                    HideWaveDefinitions();
                    ShowWaveDefinitions( position );
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ) );

        if ( selectedWaveSet.waveDefinitions != null )
            for ( int i = 0 ; selectedWaveSet.waveDefinitions.Count > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveDefinition" , "Wave Definition" , width , height , _container ,
                    fontSize: 20 ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( selectedWaveSet != null && Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedWaveDefinition != null )
                                buttons[ selectedWaveSet.waveDefinitions.IndexOf( selectedWaveDefinition ) + 1 ].SetColor( Color.white );

                            selectedWaveDefinition = selectedWaveSet.waveDefinitions[ index ];

                            Hide();
                            Show();
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        waveDefinitionLayout.SetParent( _container );
        waveDefinitionLayout.SetPosition( position + ( Vector3.left * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
        waveDefinitionLayout.Add( buttons , true );
        showingWaveDefinitions = true;
    }

    public void ShowWaveEventButtons()
    {
        List<List<Button>> waveEventButtons = new List<List<Button>>();

        for ( int i = 0 ; _editor.stage.lanes > i ; i++ )
            waveEventButtons.Add( new List<Button>() );

        for ( int i = 0 ; selectedWaveDefinition.waveEvents.Count > i ; i++ )
        {
            int index = i;

            if ( waveEventButtons.Count > selectedWaveDefinition.waveEvents[ index ].lane )
                waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Add( new Dropdown( "WaveEvent" + index.ToString() , index.ToString() , 1 , 1 , _container ,
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
                                new Label("Type:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Dropdown( "Type" , WaveEvent.Type.SpawnEnemy.ToString() , 2 , 0.5f , _container ,
                                    fontSize: 20 ,
                                    Enter: ( Button b ) => b.SetColor( Color.green ) ,
                                    Stay: ( Button b ) => { } ,
                                    Exit: ( Button b ) => b.SetColor( Color.white ) ) ,

                                new Label( "Delay:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Field( "Delay" , selectedWaveDefinition.waveEvents[ index ].delay.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].delay ) ) ,

                                new Label("Entry:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Field( "Entry" , selectedWaveDefinition.waveEvents[ index ].entryPoint.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].entryPoint ) )
                            };

                            waveEventEditor = new Layout( "WaveEventEditor" , 3.5f , 3 , 0.25f , 0.1f , waveEventEditorButtons.Count / 2 , _container );
                            waveEventEditor.Add( waveEventEditorButtons , true );
                            waveEventEditor.SetPosition( _editor.stage.LaneBy( selectedWaveDefinition.waveEvents[ index ].lane ).start + ( Vector3.left * waveEventEditor.width * 0.5f ) );
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
            Layout layout = new Layout( "WaveEvent" + i.ToString() + "Layout" , waveEventButtons[ i ].Count , 1 , 0.25f , 0.1f , 1 , _container );
            layout.SetLocalPosition( _editor.stage.LaneBy( i ).start + ( Vector3.left * layout.width * 0.5f ) );
            layout.Add( waveEventButtons[ i ] , true );
            _waveEventLayouts.Add( layout );
            layout.Refresh();
        }
    }

    public void HideWaveEventButtons()
    {
        for ( int i = 0 ; _waveEventLayouts.Count > i ; i++ )
            _waveEventLayouts[ i ].Destroy();

        _waveEventLayouts.Clear();
    }

    public void HideWaveDefinitions()
    {
        showingWaveDefinitions = false;
        waveDefinitionLayout?.Destroy();
    }

    public void HideWaveSets()
    {
        selectedWaveSet = null;
        waveSetLayout?.Destroy();
    }

    public void Hide()
    {
        HideWaveEventButtons();
        HideWaveDefinitions();
        HideWaveSets();
    }

    public void Refresh()
    {
        Hide();
        Show();
    }

    public Vector3 position => _editor.stageEditorPosition + ( Vector3.back * ( _editor.stageSetLayoutHeight + 0.5f ) );

    public WaveData waveData { get; private set; }
    public Layout waveSetLayout { get; private set; }
    public Layout waveEventEditor { get; private set; }
    public Layout waveDefinitionLayout { get; private set; }
    public Layout waveEventEditorLayout { get; private set; }
    public bool showingWaveDefinitions { get; private set; }

    public HeldEvent heldWaveEvent { get; set; }
    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }

    private const string _waveDataPath = "Assets/AssetBundleSource/Waves/";
    private List<Layout> _waveEventLayouts { get; }
    private GameObject _container { get; }
    private Editor _editor { get; }

    public WaveEditor( Editor editor , GameObject parent = null )
    {
        _editor = editor;
        _container = new GameObject( "WaveEditor" );

        if ( parent != null )
            _container.transform.SetParent( parent.transform );

        _waveEventLayouts = new List<Layout>();
        Load();

        if ( waveData == null )
            Create();
    }
}
#endif //UNITY_EDITOR