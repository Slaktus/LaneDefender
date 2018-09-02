using System.Collections;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public Material unlitColor;

	void Awake()
    {
        instance = this;
        #if !UNITY_EDITOR
        //Assets.Initialize(this, () => StartSession(new Player()));
        #else
        Assets.Initialize(this, () => StartSession(new Player()));
        //Assets.Initialize(this, () => editor = new Editor(gameObject));
        #endif
    }

    #if UNITY_EDITOR
    Editor editor;

    private void Update() => editor?.Update();
    #endif

    void StartSession( Player player ) => StartCoroutine( SessionHandler( new Session( player ) ) );

    public IEnumerator SessionHandler( Session session )
    {
        if (!Definitions.initialized)
            Definitions.Initialize(Assets.Get(Assets.ObjectDataSets.Default));

        session.Hide();

        TitleScreen titleScreen = new TitleScreen(gameObject);
        titleScreen.ShowCampaigns();
        titleScreen.ShowTitle();

        while (titleScreen.selectedCampaign == null )
        {
            titleScreen.Update();
            yield return null;
        }

        titleScreen.HideCampaigns();
        titleScreen.HideTitle();
        titleScreen.Hide();

        GameObject quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        quad.transform.rotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.position = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f , 0.5f , Camera.main.transform.position.y ) );
        quad.transform.localScale = new Vector3( 12 , 4 , 1 );

        TextMesh textMesh = new GameObject( "StartText" ).AddComponent<TextMesh>();
        textMesh.transform.localRotation = quad.transform.rotation;
        textMesh.transform.SetPositionAndRotation( quad.transform.position + Vector3.up , quad.transform.rotation );
        textMesh.fontSize = 200;
        textMesh.color = Color.black;
        textMesh.characterSize = 0.15f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = "START";

        float wait = Time.time + 3;

        while ( wait > Time.time )
            yield return null;

        quad.SetActive( false );
        textMesh.gameObject.SetActive( false );

        MissionDefinition missionDefinition = titleScreen.selectedCampaign.GetMissionDefinition(titleScreen.selectedCampaign.missionIndices[ 0 ]);
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

        Shop shop = new Shop();
        shop.Show( session.player );

        while ( !Input.GetMouseButtonDown( 1 ) )
        {
            shop.Update();
            yield return null;
        }

        shop.Hide();
        StartSession( session.player );
    }

    public static Entry instance { get; private set; }
}