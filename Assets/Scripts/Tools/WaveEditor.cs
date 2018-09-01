#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaveEditor : Layout
{
    public override void Show() => ShowWaveSets();

    private void ShowWaveSets( float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        HideWaveSets();
        int count = _editor.waveData.waveSets.Count;
        Add(waveSets = new Layout("WaveSetButtons", width, height * (count + 1), padding, spacing, count + 1, container));
        waveSets.SetLocalPosition(_editor.timelineEditor.indicatorPosition + (Vector3.back * (height * count * 0.5f)) + Vector3.up);
        waveSets.Add(new List<Button>(Button.GetButtons(count, (int index) => new Button("Wave Set", width, height, container, "WaveSet",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(selectedWaveSet == _editor.waveData.waveSets[ index ] ? button.color : Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    button.Select();
                    button.SetColor(Color.yellow);
                    selectedWaveSet = _editor.waveData.waveSets[ index ];

                    HideWaveDefinitions();
                    ShowWaveDefinitions(button.position);
                }
            },
            Exit: (Button button) => button.SetColor(selectedWaveSet == _editor.waveData.waveSets[ index ] ? button.color : Color.white))))
        {
            new Button( "Add Wave Set" , width , height , container , "AddWaveSet" ,
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
                    button.SetColor( Color.white );
                    HideWaveDefinitions();
                    HideWaveSets();
                }
            } )
        }, true);
    }

    public void HideWaveSets()
    {
        if (waveSets != null)
            Remove(waveSets);

        waveSets?.Destroy();
        waveSets = null;
    }

    private void ShowWaveDefinitions( Vector3 position , float width = 3 , float height = 1 , float padding = 0.25f , float spacing = 0.1f )
    {
        HideWaveDefinitions();
        int count = selectedWaveSet.waveDefinitions.Count;
        Add(_waveDefinitionLayout = new Layout("WaveDefinitionButtons", width, height * (count + 1), padding, spacing, count + 1, container));
        _waveDefinitionLayout.SetPosition(position + (Vector3.left * width) + (Vector3.back * ((height * count * 0.5f) + (padding * 0.5f))));
        _waveDefinitionLayout.Add(new List<Button>(Button.GetButtons(count, (int index) => new Button("Wave Definition", width, height, container, "WaveDefinition",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (selectedWaveSet != null && Input.GetMouseButtonDown(0))
                {
                    selectedWaveDefinition = selectedWaveSet.waveDefinitions[ index ];
                    _editor.timelineEditor.AddWaveToTimeline(selectedWaveDefinition);
                    ShowWaveEventButtons();
                    HideWaveDefinitions();
                    HideWaveSets();
                }
            },
            Exit: (Button button) => button.SetColor(Color.white))))
        {
            new Button( "Add Wave\nDefinition" , width , height , container , "AddWaveDefinition" ,
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
            Exit: ( Button button ) => button.SetColor( Color.white ) )
        }, true);
    }

    public void HideWaveDefinitions()
    {
        if (_waveDefinitionLayout != null)
            Remove(_waveDefinitionLayout);

        _waveDefinitionLayout?.Destroy();
        _waveDefinitionLayout = null;
    }

    public void ShowWaveEventButtons()
    {
        HideWaveEventButtons();
        List<List<Button>> waveEventButtons = new List<List<Button>>();

        for ( int i = 0 ; _editor.stage.lanes > i ; i++ )
        {
            waveEventButtons.Add( new List<Button>() );
            Layout layout = new Layout( "WaveEvent" + i.ToString() + "Layout" , waveEventButtons[ i ].Count , 1 , 0.25f , 0.1f , 1 , container , false );
            layout.SetLocalPosition( _editor.stage.LaneBy( i ).start + ( Vector3.left * layout.width * 0.5f ) );
            waveEventLayouts.Add( layout );
            Add(layout);
        }

        for ( int i = 0 ; selectedWaveDefinition.waveEvents.Count > i ; i++ )
        {
            int index = i;
            int laneIndex = selectedWaveDefinition.waveEvents[ index ].lane;
            WaveEventDefinition waveEvent = selectedWaveDefinition.waveEvents[ index ];
            Lane lane = _editor.stage.LaneBy( laneIndex );

            if ( waveEventButtons.Count > laneIndex )
            {
                Button button = new Button(index.ToString(), 1, 1, container, "WaveEvent" + index.ToString(),
                    Enter: (Button butt) => butt.SetColor(Color.green),
                    Stay: (Button butt) =>
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            butt.Select();
                            List<Element> waveEventEditorButtons = new List<Element>()
                            {
                                new Label("Type:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Button( WaveEvent.Type.SpawnEnemy.ToString() , 2 , 0.5f , container , "Type" ,
                                    fontSize: 20 ,
                                    Enter: ( Button b ) => b.SetColor( Color.green ) ,
                                    Stay: ( Button b ) => { } ,
                                    Exit: ( Button b ) => b.SetColor( Color.white ) ) ,

                                new Label( "Delay:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Field( "Delay" , waveEvent.delay.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers  , EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].delay ) ) ,

                                new Label("Entry:" , Color.black , 1.25f , 0.5f , container , fontSize: 20 , anchor: TextAnchor.MiddleCenter ) ,
                                new Field( "Entry" , waveEvent.entryPoint.ToString() , 2 , 0.5f , 20 , container , Field.ContentMode.Numbers , StartInput: ( Field field ) => Debug.Log("hello"), EndInput: ( Field field ) => float.TryParse( field.label.text , out selectedWaveDefinition.waveEvents[ index ].entryPoint ) )
                            };

                            Add( waveEventEditor = new Layout("WaveEventEditor", 3.5f, 3, 0.25f, 0.1f, waveEventEditorButtons.Count / 2, container) );
                            waveEventEditor.Add(waveEventEditorButtons, true);
                            waveEventEditor.SetPosition(butt.position + Vector3.up);

                            for (int j = 0; waveEventButtons[ laneIndex ].Count > j; j++)
                                waveEventButtons[ laneIndex ][ j ].Hide();
                        }

                        if (Input.GetMouseButtonDown(1))
                        {
                            selectedWaveDefinition.Remove(waveEvent);
                            ShowWaveEventButtons();
                        }
                    },
                    Exit: (Button butt) =>
                    {
                        if ( _editor.timelineEditor.heldWave == null && heldWaveEvent == null && Input.GetMouseButton(0))
                        {
                            heldWaveEvent = new HeldEvent(butt.rect.position, waveEvent, laneIndex);
                            heldWaveEvent.SetText(index.ToString());
                        }

                        butt.SetColor(Color.white);
                    },
                    Close: (Button butt) =>
                    {
                        if (Input.GetMouseButtonDown(0) && butt.selected && (waveEventEditor == null || !waveEventEditor.containsMouse))
                        {
                            Remove(waveEventEditor);
                            waveEventEditor?.Destroy();
                            waveEventEditor = null;
                            butt.Deselect();

                            for (int j = 0; waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ].Count > j; j++)
                                waveEventButtons[ selectedWaveDefinition.waveEvents[ index ].lane ][ j ].Show();
                        }
                    });

                waveEventButtons[ laneIndex ].Add( button );
                waveEventLayouts[ laneIndex ].Add( button );
                button.SetLocalPosition( new Vector3( waveEvent.entryPoint * lane.width , 1 , 0 ) );
            }
        }
    }

    public void HideWaveEventButtons()
    {
        for ( int i = 0 ; waveEventLayouts.Count > i ; i++)
        {
            Remove(waveEventLayouts[ i ]);
            waveEventLayouts[ i ].Destroy();
        }

        waveEventLayouts.Clear();
    }

    public override void Hide()
    {
        HideWaveEventButtons();
        HideWaveDefinitions();
        HideWaveSets();
    }

    public override void Refresh()
    {
        Hide();
        Show();
    }

    public void SetSelectedWaveDefinition( WaveDefinition selectedWaveDefinition ) => this.selectedWaveDefinition = selectedWaveDefinition;

    public HeldEvent heldWaveEvent { get; set; }
    public Layout waveSets { get; private set; }
    public List<Layout> waveEventLayouts { get; }
    public Layout waveEventEditor { get; private set; }
    public WaveSet selectedWaveSet { get; private set; }
    public WaveDefinition selectedWaveDefinition { get; private set; }

    private Layout _waveDefinitionLayout { get; set; }
    private Layout _waveEventEditorLayout { get; set; }
    private Editor _editor { get; }

    public WaveEditor(Editor editor, GameObject parent = null) : base(typeof(WaveEditor).Name, parent)
    {
        _editor = editor;
        waveEventLayouts = new List<Layout>();
    }
}
#endif //UNITY_EDITOR