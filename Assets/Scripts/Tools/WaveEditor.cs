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
        waveSets?.Update();
        waveEventEditor?.Update();
        _waveDefinitionLayout?.Update();

        for ( int i = 0 ; waveEventLayouts.Count > i ; i++ )
            waveEventLayouts[ i ].Update();
    }

    public void Show() => ShowWaveSets();

    private void ShowWaveSets( float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        HideWaveSets();
        int count = _editor.waveData.waveSets.Count + 1;
        List<Button> buttons = new List<Button>( count );
        buttons.Add( new Button( "AddWaveSet" , "Add Wave Set" , width , height , _container ,
            fontSize: 20 ,
            Enter: ( Button button ) => button.SetColor( Color.green ) ,
            Stay: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    ScriptableObjects.Add( ScriptableObject.CreateInstance<WaveSet>() , _editor.waveData );
                    Refresh();
                }
            } ,
            Exit: ( Button button ) => button.SetColor( Color.white ) ,
            Close: ( Button button ) =>
            {
                if ( Input.GetMouseButtonDown( 0 ) && !waveSets.containsMouse && ( _waveDefinitionLayout == null || !_waveDefinitionLayout.containsMouse ) )
                {
                    for ( int i = 0 ; buttons.Count > i ; i++ )
                    {
                        buttons[ i ].Deselect();
                        buttons[ i ].SetColor( Color.white );
                    }

                    button.SetColor( Color.white );
                    HideWaveDefinitions();
                    HideWaveSets();
                }
            } ) );

        if ( _editor.waveData.waveSets != null )
            for ( int i = 0 ; count - 1 > i ; i++ )
            {
                int index = i;
                buttons.Add( new Button( "WaveSet" , "Wave Set" , width , height , _container ,
                    fontSize: 20 ,
                    Enter: ( Button button ) => button.SetColor( selectedWaveSet == _editor.waveData.waveSets[ index ] ? button.color : Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            if ( selectedWaveSet != null )
                                buttons[ _editor.waveData.waveSets.IndexOf( selectedWaveSet ) + 1 ].SetColor( Color.white );

                            button.Select();
                            button.SetColor( Color.yellow );
                            selectedWaveSet = _editor.waveData.waveSets[ index ];

                            HideWaveDefinitions();
                            ShowWaveDefinitions( button.position );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( selectedWaveSet == _editor.waveData.waveSets[ index ] ? button.color : Color.white ) ) );
            }

        waveSets = new Layout( "WaveSetButtons" , width , height * buttons.Count , padding , spacing , buttons.Count , _container );
        waveSets.SetLocalPosition( _editor.missionEditor.indicatorPosition + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) ) ) + Vector3.up );
        waveSets.Add( buttons , true );
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
                            _editor.missionEditor.AddWaveToTimeline( selectedWaveDefinition );
                            ShowWaveEventButtons();
                            HideWaveDefinitions();
                            HideWaveSets();
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) );
            }

        _waveDefinitionLayout = new Layout( "WaveDefinitionButtons" , width , height * buttons.Count , padding , spacing , buttons.Count );
        _waveDefinitionLayout.SetParent( _container );
        _waveDefinitionLayout.SetPosition( position + ( Vector3.left * width ) + ( Vector3.back * ( ( height * ( buttons.Count - 1 ) * 0.5f ) + ( padding * 0.5f ) ) ) );
        _waveDefinitionLayout.Add( buttons , true );
        _showingWaveDefinitions = true;
    }

    public void ShowWaveEventButtons()
    {
        HideWaveEventButtons();
        List<List<Button>> waveEventButtons = new List<List<Button>>();

        for ( int i = 0 ; _editor.stage.lanes > i ; i++ )
        {
            waveEventButtons.Add( new List<Button>() );
            Layout layout = new Layout( "WaveEvent" + i.ToString() + "Layout" , waveEventButtons[ i ].Count , 1 , 0.25f , 0.1f , 1 , _container , false );
            layout.SetLocalPosition( _editor.stage.LaneBy( i ).start + ( Vector3.left * layout.width * 0.5f ) );
            waveEventLayouts.Add( layout );
        }

        for ( int i = 0 ; selectedWaveDefinition.waveEvents.Count > i ; i++ )
        {
            int index = i;
            int laneIndex = selectedWaveDefinition.waveEvents[ index ].lane;
            WaveEventDefinition waveEvent = selectedWaveDefinition.waveEvents[ index ];
            Lane lane = _editor.stage.LaneBy( laneIndex );

            if ( waveEventButtons.Count > laneIndex )
            {
                Button button = new Button( "WaveEvent" + index.ToString() , index.ToString() , 1 , 1 , _container ,
                    Enter: ( Button butt ) => butt.SetColor( Color.red ) ,
                    Stay: ( Button butt ) =>
                    {
                        if ( Input.GetMouseButtonUp( 0 ) )
                        {
                            butt.Select();
                            List<Element> waveEventEditorButtons = new List<Element>()
                            {
                                new Label("Type:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Button( "Type" , WaveEvent.Type.SpawnEnemy.ToString() , 2 , 0.5f , _container ,
                                    fontSize: 20 ,
                                    Enter: ( Button b ) => b.SetColor( Color.green ) ,
                                    Stay: ( Button b ) => { } ,
                                    Exit: ( Button b ) => b.SetColor( Color.white ) ) ,

                                new Label( "Delay:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Field( "Delay" , waveEvent.delay.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers  , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].delay ) ) ,

                                new Label("Entry:" , Color.black , 1.25f , 0.5f , _container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Field( "Entry" , waveEvent.entryPoint.ToString() , 2 , 0.5f , 20 , _container , Field.ContentMode.Numbers , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].entryPoint ) )
                            };

                            waveEventEditor = new Layout( "WaveEventEditor" , 3.5f , 3 , 0.25f , 0.1f , waveEventEditorButtons.Count / 2 , _container );
                            waveEventEditor.Add( waveEventEditorButtons , true );
                            waveEventEditor.SetPosition( butt.position );

                            for ( int j = 0 ; waveEventButtons[ laneIndex ].Count > j ; j++ )
                                waveEventButtons[ laneIndex ][ j ].Hide();
                        }

                        if ( Input.GetMouseButtonDown( 1 ) )
                        {
                            selectedWaveDefinition.Remove( waveEvent );
                            ShowWaveEventButtons();
                        }
                    } ,
                    Exit: ( Button butt ) =>
                    {
                        if ( heldWaveEvent == null && Input.GetMouseButton( 0 ) )
                        {
                            heldWaveEvent = new HeldEvent( butt.rect.position , waveEvent , laneIndex );
                            heldWaveEvent.SetText( index.ToString() );
                        }

                        butt.SetColor( Color.white );
                    } ,
                    Close: ( Button butt ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) && butt.selected && ( waveEventEditor == null || !waveEventEditor.containsMouse ) )
                        {
                            waveEventEditor?.Destroy();
                            waveEventEditor = null;
                            butt.Deselect();

                            for ( int j = 0 ; waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Count > j ; j++ )
                                waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ][ j ].Show();
                        }
                    } );

                waveEventButtons[ laneIndex ].Add( button );
                waveEventLayouts[ laneIndex ].Add( button );
                button.SetLocalPosition( new Vector3( waveEvent.entryPoint * lane.width , 1 , 0 ) );
            }
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
        _showingWaveDefinitions = false;
        _waveDefinitionLayout?.Destroy();
    }

    public void HideWaveSets()
    {
        waveSets?.Destroy();
        waveSets = null;
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

    public void SetSelectedWaveDefinition( WaveDefinition selectedWaveDefinition ) => this.selectedWaveDefinition = selectedWaveDefinition;

    public Vector3 position => waveSets.position;

    public HeldEvent heldWaveEvent { get; set; }
    public Layout waveSets { get; private set; }
    public List<Layout> waveEventLayouts { get; }
    public Layout waveEventEditor { get; private set; }
    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }

    private Layout _waveDefinitionLayout { get; set; }
    private Layout _waveEventEditorLayout { get; set; }
    private bool _showingWaveDefinitions { get; set; }
    private GameObject _container { get; }
    private Editor _editor { get; }

    public WaveEditor( Editor editor )
    {
        _editor = editor;
        waveEventLayouts = new List<Layout>();
        _container = new GameObject( "WaveEditor" );
        _container.transform.SetParent( editor.container.transform );
    }
}
#endif //UNITY_EDITOR