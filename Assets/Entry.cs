﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public Material unlitColor;

	void Awake()
    {
        string progress = PlayerPrefs.GetString("Player");

        instance = this;
        #if !UNITY_EDITOR
        //Assets.Initialize(this, () => StartSession(new Player()));
        #else
        Assets.Initialize(this, () => ShowTitleScreen(string.IsNullOrEmpty(progress) ? new Player() : new Player(JsonUtility.FromJson<Progress>(progress))));
        //Assets.Initialize(this, () => editor = new Editor(gameObject));
        #endif
    }

    #if UNITY_EDITOR
    Editor editor;

    private void Update() => editor?.Update();
    #endif

    void ShowTitleScreen(Player player) => StartCoroutine(TitleScreen(player));
    void StartSession( Player player , CampaignDefinition selectedCampaign ) => StartCoroutine( SessionHandler( new Session( player ), selectedCampaign) );

    public IEnumerator TitleScreen(Player player)
    {
        TitleScreen titleScreen = new TitleScreen(gameObject);
        titleScreen.ShowCampaigns();
        titleScreen.ShowTitle();

        while (titleScreen.selectedCampaign == null)
        {
            titleScreen.Update();
            yield return null;
        }

        titleScreen.HideCampaigns();
        titleScreen.HideTitle();
        titleScreen.Hide();

        if ( titleScreen.selectedCampaign != null )
            StartSession(player, titleScreen.selectedCampaign);
    }

    public IEnumerator SessionHandler( Session session , CampaignDefinition selectedCampaign )
    {
        if (!Definitions.initialized)
            Definitions.Initialize(Assets.Get(Assets.ObjectDataSets.Default));

        session.Hide();

        MissionDefinition missionDefinition = 0 > session.player.progress.campaignProgress ? selectedCampaign.GetMissionDefinition(selectedCampaign.firstMissionIndex) : null;

        if (missionDefinition == null)
        {
            Vector3 offset = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.transform.position.y)) + (Vector3.left * selectedCampaign.width * 0.5f) + (Vector3.forward * selectedCampaign.height * 0.5f);
            CampaignMap campaignMap = new CampaignMap(selectedCampaign.width, selectedCampaign.height, selectedCampaign.columns, selectedCampaign.rows, offset);
            Layout campaignLayout = new Layout("Campaign", gameObject);

            for (int i = 0; campaignMap.tileMap.count > i; i++)
            {
                int index = i;

                if (selectedCampaign.Has(index))
                {
                    Button button = new Button(selectedCampaign.GetMissionDefinition(index).name, campaignMap.tileMap.tileWidth - 1, campaignMap.tileMap.tileHeight * 0.5f, gameObject, "CampaignMap" + index,
                    fontSize: 20,
                    Enter: (Button b) => b.SetColor(Color.green),
                    Stay: (Button b) =>
                    {
                        if (Input.GetMouseButtonDown(0))
                            missionDefinition = selectedCampaign.GetMissionDefinition(index);
                    },
                    Exit: (Button b) => b.SetColor(Color.white));

                    campaignLayout.Add(button);
                    button.SetPosition(campaignMap.tileMap.PositionOf(index));
                }
            }

            while (missionDefinition == null)
            {
                campaignLayout.Update();
                yield return null;
            }

            campaignLayout.Hide();
            campaignLayout.Destroy();
        }

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.rotation = Quaternion.Euler(90, 0, 0);
        quad.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.transform.position.y));
        quad.transform.localScale = new Vector3(12, 4, 1);

        TextMesh textMesh = new GameObject("StartText").AddComponent<TextMesh>();
        textMesh.transform.localRotation = quad.transform.rotation;
        textMesh.transform.SetPositionAndRotation(quad.transform.position + Vector3.up, quad.transform.rotation);
        textMesh.fontSize = 200;
        textMesh.color = Color.black;
        textMesh.characterSize = 0.15f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = "START";

        float wait = Time.time + 3;

        while (wait > Time.time)
            yield return null;

        quad.SetActive(false);
        textMesh.gameObject.SetActive(false);

        StageDefinition stageDefinition = missionDefinition.stageDefinition;

        session.SetConveyor(new Conveyor(
            speed: 5,
            width: 5,
            height: 15 + (1 * (stageDefinition.laneCount - 1)),
            itemInterval: 3,
            itemLimit: 8,
            itemWidthPadding: 2,
            itemSpacing: 0.1f));

        session.SetStage(new Stage(stageDefinition, session.player, session.conveyor));

        Level level = new Level(missionDefinition.duration);

        for (int i = 0; missionDefinition.waveDefinitions.Count > i; i++)
        {
            Wave wave = new Wave(missionDefinition.waveTimes[ i ] * missionDefinition.duration, session.stage);
            level.Add(wave);

            for (int j = 0; missionDefinition.waveDefinitions[ i ].waveEvents.Count > j; j++)
                switch ((WaveEvent.Type) missionDefinition.waveDefinitions[ i ].waveEvents[ j ].type)
                {
                    case WaveEvent.Type.SpawnEnemy:
                        wave.Add(new SpawnEnemyEvent(Definitions.Enemy(Definitions.Enemies.Default), missionDefinition.waveDefinitions[ i ].waveEvents[ j ]));
                        break;
                }
        }

        session.SetLevel(level);
        session.Show();

        int heroCount = session.player.inventory.heroes.Count;
        int start = heroCount == 1
            ? 2
            : heroCount == 2
                ? 1
                : 0;

        int stride = start == 2
            ? 0
            : 2;

        for ( int i = 0 ; session.player.inventory.heroes.Count > i ; i++ )
            session.stage.AddHero( session.stage.LaneBy( start + ( stride * i ) ) , Definitions.Hero( Definitions.Heroes.Default ) );

        while ( 1 > session.level.progress || session.stage.enemies > 0 || session.stage.items > 0 )
        {
            session.Update( 1 > session.level.progress || session.stage.enemies > 0);
            yield return null;
        }

        quad.SetActive( true );
        textMesh.gameObject.SetActive( true );
        session.stage.DestroyLanes();
        session.level.HideProgress();
        session.coinCounter.Hide();

        if ( session.heldItem != null )
        {
            session.heldItem.conveyorItem.Destroy();
            session.heldItem.Destroy();
        }

        session.Destroy();

        textMesh.text = "STOP";
        wait = Time.time + 3;

        while ( wait > Time.time )
            yield return null;

        quad.SetActive( false );
        textMesh.gameObject.SetActive( false );
        Destroy( textMesh.gameObject );
        Destroy( quad );

        //boss warning?
        //boss battle?

        //end of level fanfare

        session.player.progress.SetCampaignProgress(selectedCampaign.missionDefinitions.IndexOf(missionDefinition));

        Shop shop = new Shop();
        shop.Show( session.player );

        while ( !Input.GetMouseButtonDown( 1 ) )
        {
            shop.Update();
            yield return null;
        }

        shop.Hide();

        PlayerPrefs.SetString(session.player.name, JsonUtility.ToJson(session.player.progress));
        PlayerPrefs.Save();

        StartSession( session.player, selectedCampaign );
    }

    public static Entry instance { get; private set; }
}