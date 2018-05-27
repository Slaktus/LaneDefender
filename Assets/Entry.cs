using System.Collections;
using UnityEngine;

/// <summary>
/// Entry point sets up and updates session, keeps a naive Singleton-ish reference to self and temporarily holds asset references
/// MonoBehaviour is Unity's scriptable component base class. Its most important for catching events and running co-routines
/// MonoBehaviours are not thread-safe (nor is the UnityEngine API)
/// </summary>
public class Entry : MonoBehaviour
{
    /// <summary>
    /// Placeholder material, temporary scene reference until assets are properly handled
    /// </summary>
    public Material unlitColor;

    /// <summary>
    /// Sets up session and stores static instance to self
    /// Start is a special MonoBehaviour method that gets called on the frame the GameObject the MonoBehaviour is attached to gets added to the scene
    /// In this case, the Entry MonoBehaviour is attached to an existing scene object, which is added when the scene is loaded
    /// </summary>
	void Start()
    {
        instance = this;
        Session( new Player() );
    }

    void Session( Player player ) => StartCoroutine( SessionHandler( new Session( player , width: 25 , height: 15 , spacing: 1 , lanes: 5 ) ) );

    public IEnumerator SessionHandler( Session session )
    {
        session.level.HideProgress();
        session.coinCounter.Hide();
        session.stage.HideLanes();
        session.conveyor.Hide();

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
        session.level.ShowProgress();
        session.coinCounter.Show();
        session.stage.ShowLanes();
        session.conveyor.Show();

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
            session.Update( 1 > session.level.progress );
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

        Destroy( session.ground );
        session.conveyor.Destroy();
        session.coinCounter.Destroy();
        session.level.DestroyProgress();

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
        Session( session.player );
    }

    /*
    /// <summary>
    /// Updates the session
    /// Update is another special MonoBehaviour method. It gets called once a frame.
    /// </summary>
    void Update() => session.Update();
    */
    /// <summary>
    /// Singleton-ish instance reference. Useful for accessing assets
    /// </summary>
    public static Entry instance { get; private set; }
}

