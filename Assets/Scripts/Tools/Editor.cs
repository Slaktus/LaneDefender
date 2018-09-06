#if UNITY_EDITOR
using UnityEngine;

public class Editor : Layout
{
    public override void Update()
    {
        HandleLaneHover();

        _level?.Update();
        stage?.Update();
        stage?.conveyor?.Update();

        if (stage != null && stage.conveyor != null && stage.conveyor.showing)
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(mouseRay.origin, mouseRay.direction, float.PositiveInfinity);

            Lane hoveredLane = null;
            ConveyorItem hoveredItem = null;

            if (hits.Length > 0)
            {
                Vector3 mousePosition = hits[ 0 ].point;
                hoveredLane = stage.GetHoveredLane(mousePosition);

                if (stage.conveyor.Contains(mousePosition))
                {
                    hoveredItem = stage.conveyor.GetHoveredItem(mousePosition);

                    if (hoveredItem != null && _heldItem == null)
                    {
                        if (_heldItem == null && Input.GetMouseButtonDown(0))
                            _heldItem = new HeldItem(hoveredItem);
                        else
                            hoveredItem.color = Color.yellow;
                    }
                }

                stage.SetLaneColor(Color.black);

                if (_heldItem != null && hoveredLane != null)
                {
                    hoveredLane.color = Color.yellow;

                    if (!Input.GetMouseButton(0))
                    {
                        hoveredLane.Add(new LaneItem(_heldItem, hoveredLane));
                        _heldItem.conveyorItem.Destroy();
                        _heldItem.Destroy();
                        _heldItem = null;
                    }
                }

                if (_heldItem != null)
                {
                    _heldItem.SetPosition(mousePosition);

                    if (!Input.GetMouseButton(0) || Input.GetMouseButtonDown(1))
                    {
                        _heldItem.conveyorItem.color = Color.white;
                        _heldItem.Destroy();
                        _heldItem = null;
                    }
                }
            }

            stage.conveyor.SetItemColor(Color.white, _heldItem != null ? _heldItem.conveyorItem : hoveredItem);

            if (Time.time > _itemTime && (1 > _level.progress || stage.enemies > 0))
                _itemTime = stage.conveyor.AddItemToConveyor(new Inventory());
        }

        base.Update();
    }

    private void HandleLaneHover()
    {
        if (timelineEditor.heldWave == null && stage != null && (stage.conveyor == null || !stage.conveyor.showing) && waveEditor.selectedWaveDefinition != null && waveEditor.waveSets == null && waveEditor.waveEventEditor == null)
        {
            Lane hoveredLane = stage.GetHoveredLane(mousePos);
            stage.SetLaneColor(Color.black);

            if (hoveredLane != null)
            {
                hoveredLane.color = Color.yellow;

                if (Input.GetMouseButtonDown(0) && !waveEditor.waveEventLayouts[ stage.IndexOf(hoveredLane) ].containsMouse)
                {
                    int index = stage.IndexOf(hoveredLane);
                    WaveEventDefinition waveEventDefinition = ScriptableObject.CreateInstance<WaveEventDefinition>();
                    waveEventDefinition.Initialize(0, index, WaveEvent.Type.SpawnEnemy, 0 , Helpers.Normalize(mousePos.x, hoveredLane.width, hoveredLane.start.x));
                    ScriptableObjects.Add(waveEventDefinition, waveEditor.selectedWaveDefinition);
                    waveEditor.HideWaveEventButtons();
                    waveEditor.ShowWaveEventButtons();
                }
            }

            if (waveEditor.heldWaveEvent != null)
            {
                waveEditor.heldWaveEvent.SetPosition(mousePos);

                if (Input.GetMouseButtonUp(0))
                {
                    if (hoveredLane != null)
                    {
                        waveEditor.heldWaveEvent.waveEventDefinition.SetLane(stage.IndexOf(hoveredLane));
                        waveEditor.heldWaveEvent.waveEventDefinition.entryPoint = Helpers.Normalize(mousePos.x, hoveredLane.end.x, hoveredLane.start.x);
                        waveEditor.HideWaveEventButtons();
                        waveEditor.ShowWaveEventButtons();
                    }

                    waveEditor.heldWaveEvent.Destroy();
                    waveEditor.heldWaveEvent = null;
                }
            }
        }
    }

    public void ShowStage(StageDefinition stageDefinition) => stage = new Stage(
        stageDefinition, 
        new Player(),
        new Conveyor(
            speed: 6,
            width: 5,
            height: stageDefinition.height + (stageDefinition.laneSpacing * (stageDefinition.laneCount - 1)),
            itemInterval: 2,
            itemLimit: 8,
            itemWidthPadding: 2,
            itemSpacing: 0.1f,
            hide: true));

    public void HideStage()
    {
        stage?.Destroy();
        stage = null;
    }

    private void ShowCampaignEditor()
    {
        HideObjectsEditor();

        _campaignButton.Select();
        _campaignButton.SetColor(Color.yellow);

        campaignEditor.ShowCampaignSets();
        campaignEditor.ShowCampaignEditor();
    }

    private void HideCampaignEditor()
    {
        _campaignButton.Deselect();
        _campaignButton.SetColor(Color.white);

        stageEditor.Hide();
        missionEditor.Hide();
        campaignEditor.Hide();
        timelineEditor.Hide();
        waveEditor.HideWaveEventButtons();
        missionEditor.HideMissionEditor();
        campaignMapEditor.HideCampaignMap();
        timelineEditor.HideMissionTimeline();
    }

    private void ShowObjectEditors()
    {
        HideCampaignEditor();

        _objectsButton.Select();
        _objectsButton.SetColor(Color.yellow);

        enemyEditor.ShowEnemies();
        heroEditor.ShowHeroes();
        itemEditor.ShowItems();
    }

    private void HideObjectsEditor()
    {
        _objectsButton.Deselect();
        _objectsButton.SetColor(Color.white);

        enemyEditor.Hide();
        heroEditor.Hide();
        itemEditor.Hide();
    }

    public override void Refresh()
    {
        if (_campaignButton.selected)
            ShowCampaignEditor();
        else
            ShowObjectEditors();
    }

    public Stage stage { get; private set; }

    public CampaignData campaignData { get; }
    public ObjectData objectData { get; }
    public StageData stageData { get; }
    public WaveData waveData { get; }

    public CampaignMapEditor campaignMapEditor { get; }
    public TimelineEditor timelineEditor { get; }
    public CampaignEditor campaignEditor { get; }
    public MissionEditor missionEditor { get; }
    public StageEditor stageEditor { get; }
    public EnemyEditor enemyEditor { get; }
    public HeroEditor heroEditor { get; }
    public ItemEditor itemEditor { get; }
    public WaveEditor waveEditor { get; }

    private HeldItem _heldItem { get; set; }
    private Button _campaignButton { get; }
    private Button _objectsButton { get; }
    private float _itemTime { get; set; }
    private Button _testButton { get; }
    private Button _saveButton { get; }
    private Level _level { get; set; }

    public Editor(GameObject parent) : base("Editor", parent)
    {
        waveData = Assets.Get(Assets.WaveDataSets.Default);
        stageData = Assets.Get(Assets.StageDataSets.Default);
        objectData = Assets.Get(Assets.ObjectDataSets.Default);
        campaignData = Assets.Get(Assets.CampaignDataSets.Default);

        if (waveData == null)
            waveData = Assets.Create<WaveData>(Assets.waveDataPath);

        if (stageData == null)
            stageData = Assets.Create<StageData>(Assets.stageDataPath);

        if (campaignData == null)
            campaignData = Assets.Create<CampaignData>(Assets.campaignDataPath);

        if (objectData == null)
        {
            objectData = Assets.Create<ObjectData>(Assets.objectDataPath);
            ScriptableObjects.Add(ScriptableObject.CreateInstance<EnemySet>(), objectData);
            ScriptableObjects.Add(ScriptableObject.CreateInstance<ItemSet>(), objectData);
            ScriptableObjects.Add(ScriptableObject.CreateInstance<HeroSet>(), objectData);
        }

        for (int i = objectData.enemySets[ (int) Assets.ObjectDataSets.Default ].enemyDefinitions.Count; (int) Definitions.Enemies.Count > i; i++)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<EnemyDefinition>().Initialize(((Definitions.Enemies) i).ToString(), 2, 1, (Definitions.Enemies) i), objectData.enemySets[ (int) Assets.ObjectDataSets.Default ]);

        for (int i = objectData.heroSets[ (int) Assets.ObjectDataSets.Default ].heroDefinitions.Count; (int) Definitions.Heroes.Count > i; i++)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<HeroDefinition>().Initialize(((Definitions.Heroes) i).ToString(), 2, 1, (Definitions.Heroes) i), objectData.heroSets[ (int) Assets.ObjectDataSets.Default ]);

        for (int i = objectData.itemSets[ (int) Assets.ObjectDataSets.Default ].itemDefinitions.Count; (int) Definitions.Items.Count > i; i++)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<ItemDefinition>().Initialize(((Definitions.Items) i).ToString(), 2, 1, (Definitions.Items) i), objectData.itemSets[ (int) Assets.ObjectDataSets.Default ]);

        Definitions.Initialize(objectData);

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
                                        wave.Add(new SpawnEnemyEvent(Definitions.Enemy((Definitions.Enemies) missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ].subType), missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ]));
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

        _testButton.SetViewportPosition(new Vector2(1, 1));
        _testButton.SetPosition(_testButton.position + (Vector3.left * _testButton.width) + Vector3.up);

        Add(_saveButton = new Button("Save", 1.5f, 0.5f, container, "Save",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                    ScriptableObjects.Save();
            },
            Exit: (Button button) => button.SetColor(Color.white)));

        _saveButton.SetPosition(_testButton.position + Vector3.left * (_saveButton.width));

        Add(_campaignButton = new Button("Campaigns", 2, 0.5f, container, "CampaignsButton",
            fontSize: 20,
            Enter: (Button button) => button.SetColor( button.selected ? button.color : Color.green),
            Stay: (Button button) =>
            {
                if (!button.selected && Input.GetMouseButtonDown(0))
                    ShowCampaignEditor();
            },
            Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white)));

        _campaignButton.SetViewportPosition(new Vector2(0, 1));
        _campaignButton.SetPosition(_campaignButton.position + Vector3.up);

        Add(_objectsButton = new Button("Objects", 2, 0.5f, container, "ObjectsButton",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
            Stay: (Button button) => 
            {
                if ( !button.selected && Input.GetMouseButtonDown(0))
                    ShowObjectEditors();
            },
            Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white)));

        _objectsButton.SetPosition(_campaignButton.position + Vector3.right * (_objectsButton.width));
        Add(campaignMapEditor = new CampaignMapEditor(this, container));
        Add(campaignEditor = new CampaignEditor(this, Vector3.zero, container));
        Add(timelineEditor = new TimelineEditor(this, container));
        Add(missionEditor = new MissionEditor(this, container));
        Add(stageEditor = new StageEditor(this, container));
        Add(enemyEditor = new EnemyEditor(this, container));
        Add(waveEditor = new WaveEditor(this, container));
        Add(itemEditor = new ItemEditor(this, container));
        Add(heroEditor = new HeroEditor(this, container));
        ShowCampaignEditor();
    }
}
#endif //UNITY_EDITOR