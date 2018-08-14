#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Editor : Layout
{
    public override void Update()
    {
        HandleLaneHover();

        _level?.Update();
        stage?.Update();
        stage?.conveyor?.Update();

        if ( stage != null && stage.conveyor != null && stage.conveyor.showing )
        {
            Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );

            //The actual raycast returns an array with all the targets the ray passed through
            //Note that we don't pass in the ray itself -- that's because the method taking a ray as argument flat-out doesn't work
            //We don't bother constraining the raycast by layer mask just yet, since the ground plane is the only collider in the scene
            RaycastHit[] hits = Physics.RaycastAll( mouseRay.origin , mouseRay.direction , float.PositiveInfinity );

            //These references might be populated later
            Lane hoveredLane = null;
            ConveyorItem hoveredItem = null;

            //Proceed if we hit the ground plane
            if ( hits.Length > 0 )
            {
                //Get the mouse position on the ground plane
                Vector3 mousePosition = hits[ 0 ].point;

                //See if the mouse is hovering any lanes
                hoveredLane = stage.GetHoveredLane( mousePosition );

                //Proceed if the mouse is hovering the conveyor
                if ( stage.conveyor.Contains( mousePosition ) )
                {
                    //Try to get a hovered conveyor item
                    hoveredItem = stage.conveyor.GetHoveredItem( mousePosition );

                    //Proceed if an item is hovered and no item is held
                    if ( hoveredItem != null && _heldItem == null )
                    {
                        //Instantiate a new HeldItem if no item is held and the left mouse button is pressed
                        //Otherwise, change the color of the item to indicate hover
                        if ( _heldItem == null && Input.GetMouseButtonDown( 0 ) )
                            _heldItem = new HeldItem( hoveredItem );
                        else
                            hoveredItem.color = Color.yellow;
                    }
                }

                //Reset lane colors
                stage.SetLaneColor( Color.black );

                //Proceed if a lane is hovered and an item is held
                if ( _heldItem != null && hoveredLane != null )
                {
                    hoveredLane.color = Color.yellow;

                    //Proceed if the left mouse button is not held
                    //This will only happen if the left mouse button is released
                    if ( !Input.GetMouseButton( 0 ) )
                    {
                        hoveredLane.Add( new LaneItem( _heldItem , hoveredLane ) );
                        _heldItem.conveyorItem.Destroy();
                        _heldItem.Destroy();
                        _heldItem = null;
                    }
                }

                //Proceed if an item is held
                if ( _heldItem != null )
                {
                    //Position the held item at the world-space mouse position
                    _heldItem.SetPosition( mousePosition );

                    //Proceed if the left mouse button is released or the right mouse button is pressed
                    if ( !Input.GetMouseButton( 0 ) || Input.GetMouseButtonDown( 1 ) )
                    {
                        //Reset the held conveyor item's color and clean up the held item
                        _heldItem.conveyorItem.color = Color.white;
                        _heldItem.Destroy();
                        _heldItem = null;
                    }
                }
            }

            //Reset the color of any item not currently hovered
            stage.conveyor.SetItemColor( Color.white , _heldItem != null ? _heldItem.conveyorItem : hoveredItem );

            if ( Time.time > _itemTime && ( 1 > _level.progress || stage.enemies > 0 ) )
                _itemTime = stage.conveyor.AddItemToConveyor( new Inventory() );
        }

        base.Update();
    }

    private void HandleLaneHover()
    {
        if ( stage != null && ( stage.conveyor == null || !stage.conveyor.showing ) && waveEditor.selectedWaveDefinition != null && waveEditor.waveSets == null && waveEditor.waveEventEditor == null )
        {
            Lane hoveredLane = stage.GetHoveredLane( mousePosition );
            stage.SetLaneColor( Color.black );

            if ( hoveredLane != null )
            {
                hoveredLane.color = Color.yellow;

                if ( Input.GetMouseButtonDown( 0 ) && !waveEditor.waveEventLayouts[ stage.IndexOf( hoveredLane ) ].containsMouse )
                {
                    int index = stage.IndexOf( hoveredLane );
                    WaveEventDefinition waveEventDefinition = ScriptableObject.CreateInstance<WaveEventDefinition>();
                    waveEventDefinition.Initialize( 0 , index , WaveEvent.Type.SpawnEnemy , Helpers.Normalize( mousePosition.x , hoveredLane.width , hoveredLane.start.x ) );
                    ScriptableObjects.Add( waveEventDefinition , waveEditor.selectedWaveDefinition );
                    waveEditor.HideWaveEventButtons();
                    waveEditor.ShowWaveEventButtons();
                }
            }

            if ( waveEditor.heldWaveEvent != null )
            {
                waveEditor.heldWaveEvent.SetPosition( mousePosition );

                if ( Input.GetMouseButtonUp( 0 ) )
                {
                    if ( hoveredLane != null )
                    {
                        waveEditor.heldWaveEvent.waveEventDefinition.SetLane( stage.IndexOf( hoveredLane ) );
                        waveEditor.heldWaveEvent.waveEventDefinition.entryPoint = Helpers.Normalize( mousePosition.x , hoveredLane.end.x , hoveredLane.start.x );
                        waveEditor.HideWaveEventButtons();
                        waveEditor.ShowWaveEventButtons();
                    }

                    waveEditor.heldWaveEvent.Destroy();
                    waveEditor.heldWaveEvent = null;
                }
            }
        }
    }

    public void ShowStage( StageDefinition stageDefinition ) => stage = new Stage( stageDefinition , new Player() ,
        new Conveyor(
            speed: 5 ,
            width: 5 ,
            height: stageDefinition.height + ( stageDefinition.laneSpacing * ( stageDefinition.laneCount - 1 ) ) ,
            itemInterval: 3 ,
            itemLimit: 8 ,
            itemWidthPadding: 2 ,
            itemSpacing: 0.1f , 
            hide: true ) );

    public void HideStage()
    {
        stage?.Destroy();
        stage = null;
    }

    public void ShowCampaignMap()
    {
        HideCampaignMap();
        campaignMap = new CampaignMap( campaignEditor.selectedCampaign.width , campaignEditor.selectedCampaign.height , campaignEditor.selectedCampaign.columns , campaignEditor.selectedCampaign.rows );

        for ( int i = 0 ; campaignMap.tileMap.count > i ; i++ )
        {
            int index = i;
            Button button = new Button(campaignEditor.selectedCampaign.Has(index) ? campaignEditor.selectedCampaign.Get(index).name : index.ToString(), campaignMap.tileMap.tileWidth - 1, campaignMap.tileMap.tileHeight * 0.5f, container, "CampaignMap" + index,
                Enter: (Button b) => b.SetColor(b.selected ? b.color : Color.green),
                Stay: (Button b) =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (campaignEditor.selectedCampaign.Has(index))
                        {
                            missionEditor.SetSelectedMission(campaignEditor.selectedCampaign.Get(index));

                            if (missionEditor.selectedMission.stageDefinition != null)
                            {
                                stageEditor.SetSelectedStageDefinition(missionEditor.selectedMission.stageDefinition);
                                ShowStage(missionEditor.selectedMission.stageDefinition);
                            }

                            stageEditor.Show();
                            missionEditor.ShowMissionEditor();
                            timelineEditor.ShowMissionTimeline();

                            campaignEditor.Hide();
                            timelineEditor.Hide();
                            missionEditor.Hide();
                            HideCampaignMap();
                        }
                        else
                        {
                            missionEditor.ShowMissionSets(index, b.position + new Vector3(b.width * 0.5f, 0, b.height * 0.5f));
                            b.SetColor(Color.yellow);
                            b.Select();
                        }
                    }

                    if (Input.GetMouseButtonDown(1) && campaignEditor.selectedCampaign.Has(index))
                    {
                        Debug.Log(index);
                        //ok, so the problem is that it's the same damn instance!
                        //that's ... uh, gonna be a bit of a bitch to work around
                        //some kind of double bookkeeping required that I'm too tired to handle now
                        //but at least hey, that's it -- when looking up the index and removing the instance, it finds the other identical instance, because it's the same in both
                        //duh
                        campaignEditor.selectedCampaign.Remove(campaignEditor.selectedCampaign.Get(index));
                        ShowCampaignMap();
                    }
                },
                Exit: (Button b) => b.SetColor(b.selected ? b.color : Color.white),
                Close: (Button b) =>
                {
                    if (b.selected && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && (missionEditor.missionSets == null || !missionEditor.missionSets.containsMouse) && (missionEditor.missions == null || !missionEditor.missions.containsMouse))
                    {
                        b.Deselect();
                        b.SetColor(Color.white);
                        missionEditor.HideMissions();
                        missionEditor.HideMissionSets();
                    }
                });

            button.SetPosition( campaignMap.tileMap.PositionOf( index ) );
            _campaignMapButtons.Add( button );
            Add(button);
        }
    }

    public void HideCampaignMap()
    {
        for ( int i = 0 ; _campaignMapButtons.Count > i ; i++)
        {
            Remove(_campaignMapButtons[ i ]);
            _campaignMapButtons[ i ].Destroy();
        }

        _campaignMapButtons.Clear();
    }

    public override void Refresh() => ShowCampaignMap();

    public Button GetMapButton( int index ) => _campaignMapButtons[ index ];

    public T Load<T>( string path ) where T : ScriptableObject => AssetDatabase.LoadAssetAtPath<T>( path + typeof( T ) + ".asset" );
    private T Create<T>( string path ) where T : ScriptableObject => ScriptableObjects.Create<T>( path + typeof( T ) + ".asset" );

    public Vector3 mousePosition => Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );
    public CampaignMap campaignMap { get; private set; }
    public Stage stage { get; private set; }

    public CampaignData campaignData { get; }
    public ObjectData objectData { get; }
    public StageData stageData { get; }
    public WaveData waveData { get; }

    public TimelineEditor timelineEditor { get; }
    public CampaignEditor campaignEditor { get; }
    public MissionEditor missionEditor { get; }
    public StageEditor stageEditor { get; }
    public WaveEditor waveEditor { get; }
    public ItemEditor itemEditor { get; }

    private List<Button> _campaignMapButtons { get; }
    private HeldItem _heldItem { get; set; }
    private float _itemTime { get; set; }
    private Button _testButton { get; }
    private Button _saveButton { get; }
    private Level _level { get; set; }

    private const string _campaignDataPath = "Assets/AssetBundleSource/Campaigns/";
    private const string _objectDataPath = "Assets/AssetBundleSource/Objects/";
    private const string _stageDataPath = "Assets/AssetBundleSource/Stages/";
    private const string _waveDataPath = "Assets/AssetBundleSource/Waves/";

    public Editor(GameObject parent) : base("Editor", parent)
    {
        waveData = Load<WaveData>( _waveDataPath );
        stageData = Load<StageData>( _stageDataPath );
        objectData = Load<ObjectData>(_objectDataPath);
        campaignData = Load<CampaignData>( _campaignDataPath );

        if ( waveData == null )
            waveData = Create<WaveData>( _waveDataPath );

        if ( stageData == null )
            stageData = Create<StageData>( _stageDataPath );

        if (objectData == null)
        {
            objectData = Create<ObjectData>(_objectDataPath);
            ScriptableObjects.Add(ScriptableObject.CreateInstance<EnemySet>(), objectData);
            ScriptableObjects.Add(ScriptableObject.CreateInstance<ItemSet>(), objectData);
            ScriptableObjects.Add(ScriptableObject.CreateInstance<HeroSet>(), objectData);

            for (int i = 0; (int) Definitions.Enemies.Count > i; i++)
                ScriptableObjects.Add(ScriptableObject.CreateInstance<EnemyDefinition>().Initialize(((Definitions.Enemies) i).ToString(), 2, 1, 3, (Definitions.Enemies) i), objectData.enemySets[ (int) Assets.ObjectDataSets.Default ]);

            for (int i = 0; (int) Definitions.Heroes.Count > i; i++)
                ScriptableObjects.Add(ScriptableObject.CreateInstance<HeroDefinition>().Initialize(((Definitions.Heroes) i).ToString(), 2, 1, (Definitions.Heroes) i), objectData.heroSets[ (int) Assets.ObjectDataSets.Default ]);

            for (int i = 0; (int) Definitions.Items.Count > i; i++)
                ScriptableObjects.Add(ScriptableObject.CreateInstance<ItemDefinition>().Initialize(((Definitions.Items) i).ToString(), 2, 1, (Definitions.Items) i, Definitions.GetEffects((Definitions.Items) i)), objectData.itemSets[ (int) Assets.ObjectDataSets.Default ]);
        }

        if ( campaignData == null )
            campaignData = Create<CampaignData>( _campaignDataPath );

        _campaignMapButtons = new List<Button>();

        Add(_testButton = new Button("Test", 1.5f, 0.5f, container, "Test",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(stage != null && missionEditor.selectedMission != null ? Color.green : button.color),
            Stay: (Button button) =>
            {
                if (stage != null && missionEditor.selectedMission != null && Input.GetMouseButtonDown(0))
                {
                    if (_level == null)
                    {
                        stage.conveyor.Show();
                        _level = new Level(missionEditor.selectedMission.duration, showProgress: false);

                        for (int i = 0; missionEditor.selectedMission.waveDefinitions.Count > i; i++)
                        {
                            Wave wave = new Wave(missionEditor.selectedMission.waveTimes[ i ] * missionEditor.selectedMission.duration, stage);
                            _level.Add(wave);

                            for (int j = 0; missionEditor.selectedMission.waveDefinitions[ i ].waveEvents.Count > j; j++)
                                switch ((WaveEvent.Type) missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ].type)
                                {
                                    case WaveEvent.Type.SpawnEnemy:
                                        wave.Add(new SpawnEnemyEvent(Definitions.Enemy(Definitions.Enemies.Default), missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ]));
                                        break;
                                }
                        }

                        button.SetLabel("Stop");
                        waveEditor.HideWaveEventButtons();
                    }
                    else
                    {
                        stage.ClearLanes();
                        stage.conveyor.Hide();
                        stage.conveyor.Clear();
                        button.SetLabel("Test");
                        _level.DestroyProgress();
                        _heldItem = null;
                        _level = null;
                        _itemTime = 0;

                        if (waveEditor.selectedWaveDefinition != null)
                            waveEditor.ShowWaveEventButtons();
                    }
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)));

        _testButton.SetViewportPosition( new Vector2( 1 , 1 ) );
        _testButton.SetPosition( _testButton.position + ( Vector3.left * _testButton.width ) + Vector3.up );

        Add( _saveButton = new Button("Save", 1.5f, 0.5f, container, "Save",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                    ScriptableObjects.Save();
            },
            Exit: (Button button) => button.SetColor(Color.white)));

        _saveButton.SetPosition( _testButton.position + Vector3.left * ( _saveButton.width ) );

        Add(campaignEditor = new CampaignEditor( this , Vector3.zero , container));
        Add(timelineEditor = new TimelineEditor(this, container));
        Add(missionEditor = new MissionEditor( this, container));
        Add(stageEditor = new StageEditor( this, container));
        Add(waveEditor = new WaveEditor( this, container));
        Add(itemEditor = new ItemEditor(this, container));

        itemEditor.ShowItems();
        //campaignEditor.ShowCampaignSets();
        //campaignEditor.ShowCampaignEditor();
    }
}

#endif //UNITY_EDITOR