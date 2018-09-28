#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class Editor : Layout
{
    public override void Update()
    {
        session.Update(stage?.enemies > 0 || 1 >= level?.progress );
        HandleLaneHover();
        base.Update();
    }

    private void HandleLaneHover()
    {
        if (timelineEditor.heldWave == null && stage != null && (conveyor == null || !conveyor.showing) && waveEditor.selectedWaveDefinition != null && waveEditor.waveSets == null && waveEditor.waveEventEditor == null)
        {
            Lane hoveredLane = stage.GetHoveredLane(mousePos);
            stage.SetLaneColor(Color.black);

            if (hoveredLane != null)
            {
                hoveredLane.color = Color.yellow;
                int laneIndex = stage.IndexOf(hoveredLane);

                if (Input.GetMouseButtonDown(0) && laneIndex >= 0 && waveEditor.waveEventLayouts.Count > laneIndex && !waveEditor.waveEventLayouts[ laneIndex ].containsMouse)
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

    public void ShowStage(StageDefinition stageDefinition) => session.SetStage( new Stage(
        stageDefinition, 
        new Player(),
        new Conveyor(
            speed: 6,
            width: 4,
            height: stageDefinition.height + (stageDefinition.laneSpacing * (stageDefinition.laneCount - 1)),
            itemInterval: 2,
            itemLimit: 8,
            itemWidthPadding: 1,
            itemSpacing: 0.1f,
            hide: true)));

    public void HideStage()
    {
        stage?.Destroy();
        session.SetStage(null);
    }

    private void ShowCampaignEditor()
    {
        if ( missionEditor.selectedMission != null)
        {
            if (missionEditor.selectedMission.stageDefinition != null)
            {
                stageEditor.SetSelectedStageDefinition(missionEditor.selectedMission.stageDefinition);
                ShowStage(stageEditor.selectedStageDefinition);
                testButton.Enable();
                testButton.Show();
            }

            stageEditor.Show();
            missionEditor.ShowMissionEditor();
            timelineEditor.ShowMissionTimeline();

            campaignEditor.Hide();
            timelineEditor.Hide();
            missionEditor.Hide();

        }
        else
        {
            if (campaignEditor.selectedCampaign == null && campaignData.campaignSets.Count > 0 && campaignData.campaignSets[ 0 ].campaignDefinitions.Count > 0)
                campaignEditor.SetSelectedCampaign(campaignData.campaignSets[ 0 ].campaignDefinitions[ 0 ]);

            if (campaignEditor.selectedCampaign != null)
                campaignMapEditor.ShowCampaignMap();

            campaignEditor.ShowCampaignSets();
            campaignEditor.ShowCampaignEditor();
        }

        HideObjectsEditor();

        campaignButton.Select();
        campaignButton.SetColor(Color.yellow);
    }

    private void HideCampaignEditor()
    {
        campaignButton.Deselect();
        campaignButton.SetColor(Color.white);

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
        testButton.Hide();
        testButton.Disable();

        HideCampaignEditor();

        objectsButton.Select();
        objectsButton.SetColor(Color.yellow);

        enemyEditor.ShowEnemies();
        heroEditor.ShowHeroes();
        itemEditor.ShowItems();
    }

    private void HideObjectsEditor()
    {
        objectsButton.Deselect();
        objectsButton.SetColor(Color.white);

        enemyEditor.Hide();
        heroEditor.Hide();
        itemEditor.Hide();
    }

    public override void Refresh()
    {
        if (campaignButton.selected)
            ShowCampaignEditor();
        else
            ShowObjectEditors();
    }

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
    public Button campaignButton { get; }
    public Button objectsButton { get; }
    public Button testButton { get; }
    public Button saveButton { get; }

    public Stage stage => session.stage;
    public Level level => session.level;
    public Conveyor conveyor => session.stage?.conveyor;

    private Session session { get; }
    private HeldItem _heldItem { get; set; }
    private float _itemTime { get; set; }

    public Editor(Player player, GameObject parent) : base("Editor", parent)
    {
        session = new Session(new Player());
        session.Hide();

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
            objectData = Assets.Create<ObjectData>(Assets.objectDataPath);

        if (objectData.enemySets.Count == 0 )
            ScriptableObjects.Add(ScriptableObject.CreateInstance<EnemySet>(), objectData);

        if (objectData.itemSets.Count == 0)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<ItemSet>(), objectData);

        if (objectData.heroSets.Count == 0)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<HeroSet>(), objectData);

        for (int i = objectData.enemySets[ (int) Assets.ObjectDataSets.Default ].enemyDefinitions.Count; (int) Definitions.Enemies.Count > i; i++)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<EnemyDefinition>().Initialize(((Definitions.Enemies) i).ToString(), 2, 1, (Definitions.Enemies) i), objectData.enemySets[ (int) Assets.ObjectDataSets.Default ]);

        for (int i = objectData.heroSets[ (int) Assets.ObjectDataSets.Default ].heroDefinitions.Count; (int) Definitions.Heroes.Count > i; i++)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<HeroDefinition>().Initialize(((Definitions.Heroes) i).ToString(), 2, 1, (Definitions.Heroes) i), objectData.heroSets[ (int) Assets.ObjectDataSets.Default ]);

        for (int i = objectData.itemSets[ (int) Assets.ObjectDataSets.Default ].itemDefinitions.Count; (int) Definitions.Items.Count > i; i++)
            ScriptableObjects.Add(ScriptableObject.CreateInstance<ItemDefinition>().Initialize(((Definitions.Items) i).ToString(), 2, 1, (Definitions.Items) i), objectData.itemSets[ (int) Assets.ObjectDataSets.Default ]);

        Definitions.Initialize(objectData);

        Add(testButton = new Button("Test", 1.5f, 0.5f, container, "Test",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(stage != null && missionEditor.selectedMission != null ? Color.green : button.color),
            Stay: (Button button) =>
            {
                if (stage != null && missionEditor.selectedMission != null && Input.GetMouseButtonDown(0))
                {
                    if (level == null)
                    {
                        stage.conveyor.Show();
                        session.SetLevel(new Level(missionEditor.selectedMission.duration, showProgress: false));

                        for (int i = 0; missionEditor.selectedMission.waveDefinitions.Count > i; i++)
                        {
                            Wave wave = new Wave(missionEditor.selectedMission.waveTimes[ i ] * missionEditor.selectedMission.duration, stage);
                            level.Add(wave);

                            for (int j = 0; missionEditor.selectedMission.waveDefinitions[ i ].waveEvents.Count > j; j++)
                                switch ((WaveEvent.Type) missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ].type)
                                {
                                    case WaveEvent.Type.SpawnEnemy:
                                        wave.Add(new SpawnEnemyEvent(Definitions.Enemy((Definitions.Enemies) missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ].subType), missionEditor.selectedMission.waveDefinitions[ i ].waveEvents[ j ]));
                                        break;
                                }
                        }

                        button.SetLabel("Stop");
                        button.SetPosition(saveButton.position);

                        saveButton.Hide();
                        saveButton.Disable();
                        objectsButton.Hide();
                        campaignButton.Hide();
                        stageEditor.HideStageSets();
                        stageEditor.HideStageEditor();
                        stageEditor.HideStageDefinitions();
                        missionEditor.HideMissionEditor();
                        waveEditor.HideWaveEventButtons();
                        session.Start();
                    }
                    else
                    {
                        session.Stop();
                        stage.ClearLanes();
                        conveyor.Hide();
                        conveyor.Clear();
                        button.SetLabel("Test");
                        level.DestroyProgress();
                        session.SetLevel(null);
                        _heldItem = null;
                        _itemTime = 0;

                        saveButton.Show();
                        saveButton.Enable();
                        objectsButton.Show();
                        campaignButton.Show();
                        stageEditor.ShowStageSets();
                        stageEditor.ShowStageEditor();
                        missionEditor.ShowMissionEditor();
                        testButton.SetPosition(saveButton.position + Vector3.right * (saveButton.width));

                        if (waveEditor.selectedWaveDefinition != null)
                            waveEditor.ShowWaveEventButtons();
                    }
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)));


        Add(saveButton = new Button("Save", 1.5f, 0.5f, container, "Save",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(Color.green),
            Stay: (Button button) =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    EditorUtility.SetDirty(campaignData);
                    EditorUtility.SetDirty(objectData);
                    EditorUtility.SetDirty(stageData);
                    EditorUtility.SetDirty(waveData);
                    ScriptableObjects.Save();
                }
            },
            Exit: (Button button) => button.SetColor(Color.white)));

        saveButton.SetViewportPosition(new Vector2(0, 1));
        saveButton.SetPosition(saveButton.position + Vector3.up);
        testButton.SetPosition(saveButton.position + Vector3.right * (saveButton.width));
        testButton.Disable();
        testButton.Hide();
        
        Add(campaignButton = new Button("Campaigns", 2, 0.5f, container, "CampaignsButton",
            fontSize: 20,
            Enter: (Button button) => button.SetColor( button.selected ? button.color : Color.green),
            Stay: (Button button) =>
            {
                if (!button.selected && Input.GetMouseButtonDown(0))
                    ShowCampaignEditor();
            },
            Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white)));

        campaignButton.SetViewportPosition(new Vector2(1, 1));
        campaignButton.SetPosition(campaignButton.position + (Vector3.left * campaignButton.width) + Vector3.up);

        Add(objectsButton = new Button("Objects", 2, 0.5f, container, "ObjectsButton",
            fontSize: 20,
            Enter: (Button button) => button.SetColor(button.selected ? button.color : Color.green),
            Stay: (Button button) => 
            {
                if ( !button.selected && Input.GetMouseButtonDown(0))
                    ShowObjectEditors();
            },
            Exit: (Button button) => button.SetColor(button.selected ? button.color : Color.white)));

        objectsButton.SetPosition(campaignButton.position + Vector3.left * (objectsButton.width));
        
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