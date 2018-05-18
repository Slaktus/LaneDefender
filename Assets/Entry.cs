using System;
using System.Collections;
using System.Collections.Generic;
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
        StartCoroutine( SessionHandler( new Session( new Player() , width: 25 , height: 15 , spacing: 1 , lanes: 5 ) ) );
    }

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


        while ( 1 > session.level.progress || session.stage.enemies > 0 )
        {
            session.Update();
            yield return null;
        }

        quad.SetActive( true );
        textMesh.gameObject.SetActive( true );
        session.stage.ClearLanes();
        session.level.HideProgress();
        session.coinCounter.Hide();
        session.stage.HideLanes();

        if ( session.heldItem != null )
        {
            session.heldItem.conveyorItem.Destroy();
            session.heldItem.Destroy();
        }

        session.conveyor.Clear();
        session.conveyor.Hide();

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

/// <summary>
/// Handles the session. Keeps references to most of the game's moving parts, and updates them with the data they need to operate
/// Session is basically an observer -- it handles messaging between game systems based on their state
/// </summary>
public class Session
{
    /// <summary>
    /// Updates the session by fetching data and sharing it with the game handlers
    /// </summary>
    public void Update()
    {
        coinCounter.SetCounterValue( player.coins );

        //While there are strictly speaking better ways to get a world-space mouse position, this one has the absolute minimum number of moving parts
        //First we get a ray. The camera has a convenience method that returns a ray from the center of the camera in the direction of a screen point
        //The mouse position we can poll via the input system is in screen-space coordinates
        Ray mouseRay = camera.ScreenPointToRay( Input.mousePosition );

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
            if ( conveyor.Contains( mousePosition ) )
            {
                //Try to get a hovered conveyor item
                hoveredItem = conveyor.GetHoveredItem( mousePosition );

                //Proceed if an item is hovered and no item is held
                if ( hoveredItem != null && heldItem == null )
                {
                    //Instantiate a new HeldItem if no item is held and the left mouse button is pressed
                    //Otherwise, change the color of the item to indicate hover
                    if ( heldItem == null && Input.GetMouseButtonDown( 0 ) )
                        heldItem = new HeldItem( hoveredItem );
                    else
                        hoveredItem.color = Color.yellow;
                }
            }

            if ( Input.GetMouseButtonDown( 1 ) )
                hoveredLane?.Add( new Enemy( Definitions.Enemy( Definitions.Enemies.Default ) , hoveredLane ) );

            //Reset lane colors
            stage.SetLaneColor( Color.black );

            //Proceed if a lane is hovered and an item is held
            if ( heldItem != null && hoveredLane != null )
            {
                int laneIndex = stage.IndexOf( hoveredLane );
                int lane1Up = laneIndex - 1 >= 0 ? laneIndex - 1 : -1;
                int lane1Down = stage.lanes > laneIndex + 1 ? laneIndex + 1 : -1;

                hoveredLane.color = Color.yellow;

                //Proceed if the left mouse button is not held
                //This will only happen if the left mouse button is released
                if ( !Input.GetMouseButton( 0 ) )
                {
                    hoveredLane.Add( new LaneItem( heldItem , hoveredLane ) );
                    heldItem.conveyorItem.Destroy();
                    heldItem.Destroy();
                    heldItem = null;
                }
            }

            //Proceed if an item is held
            if ( heldItem != null )
            {
                //Position the held item at the world-space mouse position
                heldItem.SetPosition( mousePosition );

                //Proceed if the left mouse button is released or the right mouse button is pressed
                if ( !Input.GetMouseButton( 0 ) || Input.GetMouseButtonDown( 1 ) )
                {
                    //Reset the held conveyor item's color and clean up the held item
                    heldItem.conveyorItem.color = Color.white;
                    heldItem.Destroy();
                    heldItem = null;
                }
            }
        }

        //Reset the color of any item not currently hovered
        conveyor.SetItemColor( Color.white , heldItem != null ? heldItem.conveyorItem : hoveredItem );

        //Update the conveyor
        conveyor.Update();

        //Update the stage
        stage.Update();

        //Update the level
        level.Update();

        //Proceed if the item spawn interval has elapsed, and add a new item to the conveyor belt
        if ( Time.time > itemTime )
            itemTime = conveyor.AddItemToConveyor();
    }

    /// <summary>
    /// Level holds the lanes and methods for operating on them
    /// </summary>
    public Stage stage { get; }

    /// <summary>
    /// Conveyor handles items and methods for operating on them
    /// </summary>
    public Conveyor conveyor { get; }

    public Level level { get; set; }

    public CoinCounter coinCounter { get; }

    /// <summary>
    /// Getter using UnityEngine's Camera class, which features a convenience getter for the first camera in the scene with the tag "Main"
    /// </summary>
    private Camera camera => Camera.main;

    /// <summary>
    /// Ground plane GameObject. Has a BoxCollider attached
    /// </summary>
    private GameObject ground { get; }

    /// <summary>
    /// Last time an item was added to the conveyor
    /// </summary>
    private float itemTime { get; set; }

    /// <summary>
    /// Currently held item
    /// </summary>
    public HeldItem heldItem { get; set; }

    private Player player { get; }

    public Session ( Player player , float width , float height , float spacing , int lanes )
    {
        this.player = player;

        //Cube primitives have a mesh filter, mesh renderer and box collider already attached
        ground = GameObject.CreatePrimitive( PrimitiveType.Cube );

        conveyor = new Conveyor( 
            speed: 5 , 
            width: 5 , 
            height: height + ( spacing * ( lanes - 1 ) ) , 
            itemInterval: 3 ,
            itemLimit: 8 , 
            itemWidthPadding: 2 , 
            itemSpacing: 0.1f );

        stage = new Stage(
            speed: 5 ,
            width: width ,
            height: height ,
            laneSpacing: spacing ,
            laneCount: lanes , 
            conveyor: conveyor ,
            player: player );

        level = new Level( 10 );
        Wave wave = new Wave( 3 , stage );
        EnemyDefinition enemyDefinition = Definitions.Enemy( Definitions.Enemies.Default );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 0 , lane: 0 ) );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 1 , lane: 1 ) );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 1 , lane: 0 ) );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 1 , lane: 3 ) );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 5 , lane: 2 ) );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 0 , lane: 3 ) );
        wave.Add( new SpawnEnemyEvent( enemyDefinition , delay: 2 , lane: 4 ) );
        level.Add( wave );

        coinCounter = new CoinCounter();

        //Project the corners of the screen to the ground plane to find out how large the ground plane needs to be to fill the camera's field of view
        Vector3 bottomLeft = camera.ScreenToWorldPoint( new Vector3( 0 , 0 , camera.transform.position.y ) );
        Vector3 topRight = camera.ScreenToWorldPoint( new Vector3( Screen.width , Screen.height , camera.transform.position.y ) );

        //Transforms give GameObjects' positions, rotations and scales
        ground.transform.localScale = new Vector3( topRight.x - bottomLeft.x , 1 , topRight.z - bottomLeft.z );
        ground.transform.position = new Vector3( width * 0.5f , -1 , ( -height * 0.5f ) - spacing * 0.5f );
        ground.name = "Ground";

        //Disable the ground mesh renderer -- we don't want to see the cube
        //GetComponent lets us fetch references to components attached to GameObjects in a scene
        ground.GetComponent<MeshRenderer>().enabled = false;
    }
}

/// <summary>
/// Keeps and updates lanes and contains methods for operating on them
/// </summary>
public class Stage
{
    public bool Handle( WaveEvent waveEvent )
    {
        bool handled = false;

        switch ( waveEvent.type )
        {
            case WaveEvent.Type.SpawnEnemy:
                Lane lane = LaneBy( waveEvent.lane );
                handled = true;

                for ( int i = 0 ; lane.objects.Count > i && handled ; i++ )
                    if ( lane.objects[ i ] is LaneEntity && 5 > lane.objects[ i ].back - lane.start.x )
                        handled = false;

                if ( handled )
                    lane.Add( new Enemy( ( waveEvent as SpawnEnemyEvent ).enemyDefinition , lane ) );

                return handled;

            default:
                return handled;
        }

    }

    public void ShowLane( int index ) => LaneBy( index ).Show();
    public void HideLane( int index ) => LaneBy( index ).Hide();

    public void ShowLanes()
    {
        for ( int i = 0 ; lanes > i ; i++ )
            ShowLane( i );
    }

    public void HideLanes()
    {
        for ( int i = 0 ; lanes > i ; i++ )
            HideLane( i );
    }

    public void ClearLane( int index ) => LaneBy( index ).Clear();
    public void ClearLane<T>( int index ) => LaneBy( index ).Clear<T>();

    public void ClearLanes()
    {
        for ( int i = 0 ; lanes > i ; i++ )
            _lanes[ i ].Clear();
    }

    /// <summary>
    /// Updates all lanes
    /// Should strictly speaking be an event, but right now this is safe
    /// </summary>
    public void Update() => Updater();

    /// <summary>
    /// Set color of all lanes except optional
    /// </summary>
    /// <param name="color">Color to apply</param>
    /// <param name="except">Lane to except</param>
    public void SetLaneColor( Color color )
    {
        for ( int i = 0 ; _lanes.Count > i ; i++ )
            _lanes[ i ].color = color;
    }

    /// <summary>
    /// Get currently hovered lane
    /// </summary>
    /// <param name="position">World-space mouse position</param>
    /// <returns>Currently hovered lane, or null if none</returns>
    public Lane GetHoveredLane ( Vector3 position )
    {
        for ( int i = 0 ; _lanes.Count > i ; i++ )
            if ( _lanes[ i ].Contains( position ) )
                return _lanes[ i ];

        return null;
    }

    public void AddCoins( int value ) => _player.AddCoins( value );

    public int IndexOf( Lane lane ) => _lanes.IndexOf(lane );
    public Lane LaneBy( int index ) => _lanes[ index ];

    /// <summary>
    /// Set the speed at which items move down the conveyor
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed( float speed ) => this.speed = speed;

    public int enemies
    {
        get
        {
            int count = 0;

            for ( int i = 0 ; lanes > i ; i++ )
                count += _lanes[ i ].Count<Enemy>();

            return count;
        }
    }

    public int lanes => _lanes.Count;
    public float laneSpacing { get; private set; }
    public Conveyor conveyor { get; private set; }
    public float speed { get; private set; }

    private Player _player { get; }
    private List<Lane> _lanes { get; }
    private event Action Updater;

    public Stage ( float speed , float width , float height , float laneSpacing , int laneCount , Conveyor conveyor , Player player )
    {
        _player = player;
        this.speed = speed;
        this.conveyor = conveyor;
        this.laneSpacing = laneSpacing;
        float laneHeight = height / laneCount;
        _lanes = new List<Lane>( laneCount );

        for ( int i = 0 ; laneCount > i ; i++ )
        {
            Lane lane = new Lane(
                   stage: this ,
                   depth: ( i * ( laneHeight + laneSpacing ) ) + ( laneHeight * 0.5f ) ,
                   width: width ,
                   height: laneHeight ,
                   name: "Lane" + i );

            _lanes.Add( lane );
            Updater += lane.Update;
        }
    }
}

/// <summary>
/// Lanes hold items and eventually much, much more
/// </summary>
public class Lane
{
    /// <summary>
    /// Updates all the items on this lane
    /// </summary>
    public void Update() => Updater();

    public T Add<T> ( T laneObject ) where T : LaneObject
    {
        Updater += laneObject.update.MoveNext;
        objects.Add( laneObject );
        return laneObject;
    }

    public void Remove<T> ( T laneObject ) where T : LaneObject
    {
        Updater -= laneObject.update.MoveNext;
        objects.Remove( laneObject );
    }

    public void Show() => _quad.SetActive( true );

    public void Hide() => _quad.SetActive( false );

    /// <summary>
    /// Check if the lane's rect contains a world-space position
    /// The position is projected to 2D
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if the rect contains the point, false if not</returns>
    public bool Contains ( Vector3 position ) => _rect.Contains( new Vector2( position.x , position.z ) );

    public int Count<T>()
    {
        int count = 0;

        for ( int i = 0 ; objects.Count > i ; i++ )
            if ( objects[ i ] is T )
                count++;

        return count;
    }

    public void Clear<T>()
    {
        List<int> toClear = new List<int>( objects.Count );

        for ( int i = 0 ; objects.Count > i ; i++ )
            if ( objects[ i ] is T )
                toClear.Add( i );

        for ( int i = 0 ; toClear.Count > i ; i++ )
            objects[ toClear[ i ] ].Destroy();
    }

    public void Clear()
    {
        while ( objects.Count > 0 )
            objects[ objects.Count - 1 ].Destroy();
    }

    public Color color { get { return _meshRenderer.material.color; } set { _meshRenderer.material.color = value; } }
    public Vector3 start => new Vector3( _rect.xMin , 0 , _rect.yMin + ( height * 0.5f ) );
    public Vector3 end => new Vector3( _rect.xMax , 0 , _rect.yMin + ( height * 0.5f ) );
    public float width => _rect.width;
    public float height => _rect.height;
    public float speed => stage.speed;

    public Stage stage { get; }
    public List<LaneObject> objects { get; }

    /// <summary>
    /// Why a rect? Why not a collider like the ground plane?
    /// Because this has way, way, waaaay fewer moving parts and much fewer things that can go wrong
    /// Since all the mouse positions we want are on the same plane, we can project them to 2D
    /// Rects are fast, and best of all -- they're structs, so we can create and discard them willy-nilly!
    /// </summary>
    private Rect _rect { get; set; }
    private GameObject _quad { get; }
    private MeshRenderer _meshRenderer { get; }
    private event Func<bool> Updater;

    public Lane( Stage stage , float depth , float width , float height , string name )
    {
        Updater += () => false;

        this.stage = stage;
        objects = new List<LaneObject>();
        _rect = new Rect( 0 , -depth , width , height );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.position = new Vector3( width * 0.5f , 0 , -depth + ( height * 0.5f ) );
        _quad.transform.localScale = new Vector3( width , height , 0 );
        _quad.transform.name = name;

        _quad.GetComponent<MeshCollider>().enabled = false;
        _meshRenderer = _quad.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;
    }
}

/// <summary>
/// Conveyor keeps and updates conveyor items, as well as managing item match-three logic
/// </summary>
public class Conveyor
{
    /// <summary>
    /// Updates all the items on the conveyor
    /// Should strictly speaking be an event, but right now this is safe
    /// </summary>
    public void Update()
    {
        //Check all the items on the conveyor for match-three
        for ( int i = 0 ; _conveyorItems.Count > i ; i++ )
        {
            //If the conveyor item is part of a match-three, upgrade it and clean up the two other items
            if ( _conveyorItems[ i ].matchThree )
            {
                ItemAt( i + 2 ).Destroy();
                ItemAt( i + 1 ).Destroy();
                _conveyorItems[ i ].Upgrade();
            }

            //Update the conveyor item
            _conveyorItems[ i ].Update();
        }
    }

    /// <summary>
    /// Add an item to the conveyor. Currently only a placeholder
    /// </summary>
    /// <returns></returns>
    public float AddItemToConveyor()
    {
        if ( _itemLimit > _conveyorItems.Count )
            _conveyorItems.Add( new ConveyorItem( this , ( ConveyorItem.Type ) UnityEngine.Random.Range( 0 , ( int ) ConveyorItem.Type.Count ) ) );

        return Time.time + itemInterval;
    }

    /// <summary>
    /// Returns a reference to a hovered conveyor item
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public ConveyorItem GetHoveredItem( Vector3 position )
    {
        for ( int i = 0 ; _conveyorItems.Count > i ; i++ )
            if ( _conveyorItems[ i ].Contains( position ) )
                return _conveyorItems[ i ];

        return null;
    }

    /// <summary>
    /// Set color of all items except optional
    /// </summary>
    /// <param name="color">Color to apply</param>
    /// <param name="except">Item to except</param>
    public void SetItemColor( Color color , ConveyorItem except = null )
    {
        for ( int i = 0 ; _conveyorItems.Count > i ; i++ )
            if ( _conveyorItems[ i ] != except )
                _conveyorItems[ i ].color = color;
    }

    public void Clear()
    {
        while ( _conveyorItems.Count > 0 )
            _conveyorItems[ _conveyorItems.Count - 1 ].Destroy();
    }

    public void Show() => _quad.SetActive( true );
    public void Hide() => _quad.SetActive( false );

    /// <summary>
    /// Set the speed at which items travel down the conveyor
    /// </summary>
    /// <param name="speed">Speed at which items move down the conveyor</param>
    public void SetSpeed ( float speed ) => this.speed = speed;

    /// <summary>
    /// Set the interval at which items spawn at the top of the conveyor
    /// </summary>
    /// <param name="itemInterval">Interval at which to spawn items</param>
    public void SetItemInterval ( float itemInterval ) => this.itemInterval = itemInterval;

    /// <summary>
    /// Remove an item from the conveyor
    /// </summary>
    /// <param name="item">Item to remove</param>
    public void RemoveItemFromConveyor( ConveyorItem item ) => _conveyorItems.Remove( item );

    /// <summary>
    /// Check if the item's rect contains a world-space position
    /// The position is projected to 2D
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if the rect contains the point, false if not</returns>
    public bool Contains( Vector3 position ) => _rect.Contains( new Vector2( position.x , position.z ) );

    /// <summary>
    /// Get the index of a conveyor item
    /// </summary>
    /// <param name="conveyorItem">The item to return the index of</param>
    /// <returns>The index of the item</returns>
    public int IndexOf( ConveyorItem conveyorItem ) => _conveyorItems.IndexOf( conveyorItem );

    /// <summary>
    /// Get the conveyor item at given index
    /// </summary>
    /// <param name="index">The index of the item to return</param>
    /// <returns>The item at the index</returns>
    public ConveyorItem ItemAt( int index ) => _conveyorItems[ index ];

    public int itemCount => _conveyorItems.Count;
    public Vector3 top => new Vector3( _rect.center.x , 0 , _rect.yMax );
    public Vector3 bottom => new Vector3( _rect.center.x , 0 , _rect.yMin );
    public Color color { get { return _meshRenderer.material.color; } set { _meshRenderer.material.color = value; } }

    public float width => _rect.width;
    public float height => _rect.height;
    public float speed { get; private set; }
    public float itemHeight { get; private set; }
    public float itemSpacing { get; private set; }
    public float itemInterval { get; private set; }
    public float itemWidthPadding { get; private set; }

    private Rect _rect { get; }
    private int _itemLimit { get; }
    private GameObject _quad { get; }
    private MeshRenderer _meshRenderer { get; }
    private List<ConveyorItem> _conveyorItems { get; }

    public Conveyor ( float speed , float width , float height , float itemInterval , int itemLimit , float itemWidthPadding , float itemSpacing )
    {
        this.speed = speed;
        this.itemSpacing = itemSpacing;
        this.itemInterval = itemInterval;
        this.itemWidthPadding = itemWidthPadding;
        itemHeight = ( ( height - ( itemSpacing * ( itemLimit - 1 ) ) ) / itemLimit );
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint( new Vector3( Screen.width , Screen.height * 0.5f , Camera.main.transform.position.y ) ) + ( Vector3.left * width * 0.5f );

        _itemLimit = itemLimit;
        _conveyorItems = new List<ConveyorItem>();
        _rect = new Rect( new Vector2( worldPosition.x - ( width * 0.5f ) , worldPosition.z - ( height * 0.5f ) ) , new Vector2( width , height ) );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localScale = new Vector3( width , height , 0 );
        _quad.transform.position = worldPosition;
        _quad.transform.name = "Conveyor";

        _quad.GetComponent<MeshCollider>().enabled = false;

        _meshRenderer = _quad.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;
    }
}

/// <summary>
/// Items travelling down the conveyor belt. Upgrade by matching three
/// </summary>
public class ConveyorItem : MouseObject
{
    /// <summary>
    /// Remove the item from the conveyor then destroy the scene graph representation
    /// </summary>
    public override void Destroy()
    {
        _conveyor.RemoveItemFromConveyor( this );
        GameObject.Destroy( container );
    }

    /// <summary>
    /// Set the item's held flag
    /// </summary>
    /// <param name="held">The new held flag state</param>
    public void SetHeld( bool held ) => this.held = held;

    /// <summary>
    /// Upgrade the item and update the label
    /// </summary>
    public void Upgrade() => textMesh.text = type.ToString() + "\n" + ++level;

    /// <summary>
    /// Update the item's position
    /// </summary>
    public void Update() => position = new Vector3( position.x , position.y , Mathf.Clamp( position.z - ( speed * Time.deltaTime ) , limit , top - ( height * 0.5f ) ) );

    /// <summary>
    /// Check if the item's rect contains a world-space position
    /// The position is projected to 2D
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if the rect contains the point, false if not</returns>
    public bool Contains( Vector3 position ) => rect.Contains( new Vector2( position.x , position.z ) );

    public int index => _conveyor.IndexOf( this );
    public float height => _conveyor.itemHeight;
    public float width => _conveyor.width - _conveyor.itemWidthPadding;

    /// <summary>
    /// A beautiful example of a throwaway rect. Conveyor items only have colliders when they need to
    /// </summary>
    public Rect rect => new Rect( position.x - ( width * 0.5f ) , position.z - ( height * 0.5f ) , width , height );

    /// <summary>
    /// Checks the two items above (if they exist) and returns whether they qualify for a match-three
    /// </summary>
    public bool matchThree => canUpgrade && _conveyor.itemCount > index + 2 && _conveyor.ItemAt( index + 1 ).canUpgrade && _conveyor.ItemAt( index + 2 ).canUpgrade && type == _conveyor.ItemAt( index + 1 ).type && _conveyor.ItemAt( index + 1 ).type == _conveyor.ItemAt( index + 2 ).type && level == _conveyor.ItemAt( index + 1 ).level && _conveyor.ItemAt( index + 1 ).level == _conveyor.ItemAt( index + 2 ).level;

    /// <summary>
    /// Returns whether the item has settled and is below max level
    /// </summary>
    public bool canUpgrade => settled && _maxLevel > level;

    /// <summary>
    /// Returns whether the item has settled at the bottom of the conveyor
    /// Float comparisons considered harmful, so use UnityEngine's approximate comparison
    /// </summary>
    public bool settled => !held && Mathf.Approximately( position.z , limit );

    public bool held { get; private set; }
    public int level { get; private set; }
    public Type type { get; }

    private float speed => _conveyor.speed;
    private float limit => bottom + ( height * 0.5f ) + ( ( height + itemSpacing ) * index );
    private float itemSpacing => _conveyor.itemSpacing;
    private float bottom => _conveyor.bottom.z;
    private float top => _conveyor.top.z;

    private Conveyor _conveyor { get; }
    private int _maxLevel { get; } = 3;

    public ConveyorItem ( Conveyor conveyor , Type type ) : base( "Conveyor" + type.ToString() )
    {
        _conveyor = conveyor;
        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3( width , height , 1 );

        meshRenderer.material.color = Color.white;

        textMesh.transform.localRotation = Quaternion.identity;
        textMesh.text = type.ToString() + "\n" + level;

        position = conveyor.top - ( Vector3.forward * height * 0.5f ) + Vector3.up;
        this.type = type;
    }

    /// <summary>
    /// All items the conveyor can handle. For mapping
    /// </summary>
    public enum Type
    {
        LaneUp = 0,
        LaneDown = 1,
        Damage = 2,
        Split = 3,
        Leap = 4,
        Count = 5,
        Part = 6,
        Wreck = 7
    }
}

/// <summary>
/// Items picked up from the conveyor belt. Can be dropped on lanes
/// </summary>
public class HeldItem : MouseObject
{
    /// <summary>
    /// Drops the currently held item then destroy the scene graph representation
    /// </summary>
    public override void Destroy()
    {
        conveyorItem.SetHeld( false );
        GameObject.Destroy( container );
    }

    /// <summary>
    /// Set the item's position. Height is relative to the corresponding conveyor item
    /// </summary>
    /// <param name="position">Position to place the item at</param>
    public void SetPosition ( Vector3 position ) => this.position = new Vector3( position.x , conveyorItem.position.y + 1 , position.z );

    public ConveyorItem conveyorItem { get; private set; }

    public HeldItem( ConveyorItem conveyorItem ) : base ( "Held" + conveyorItem.type.ToString() )
    {
        conveyorItem.SetHeld( true );
        conveyorItem.color = Color.gray;

        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3( conveyorItem.width , conveyorItem.height , 1 );

        meshRenderer.material.color = Color.white;

        textMesh.transform.localRotation = Quaternion.identity;
        textMesh.text = conveyorItem.text;

        position = conveyorItem.position + Vector3.forward;
        this.conveyorItem = conveyorItem;
    }
}

/// <summary>
/// Items travelling down lanes. Will eventually interact with many, many other items
/// </summary>
public class LaneItem : LaneObject
{
    /// <summary>
    /// Updates the item's position, cleaning it up if it reaches the end of the lane
    /// </summary>
    public override IEnumerator Update()
    {
        bool move = true;

        while ( true )
        {
            if ( changeLane != null )
                move = !changeLane.MoveNext();

            float x = position.x - ( ( speed * Time.deltaTime ) * ( move ? 1 : 0 ) );
            bool destroy = end + ( cube.transform.localScale.x * 0.5f ) > x;
            position = new Vector3( Mathf.Clamp( x , end + ( scale.x * 0.5f ) , start - ( scale.x * 0.5f )  ) , position.y , position.z );
            Debug.DrawLine( new Vector3( rect.xMin , 0 , rect.yMin ) , new Vector3( rect.xMax , 0 , rect.yMax ) , Color.yellow );

            if ( leap != null )
                leap.MoveNext();

            if ( overlap )
                overlapping.Interaction( this );

            if ( destroy )
                Destroy();

            yield return null;
        }
    }

    public void Split()
    {
        HeldItem upHeldItem = new HeldItem( new ConveyorItem( lane.stage.conveyor , ConveyorItem.Type.Part ) );
        LaneItem upItem = new LaneItem( upHeldItem , lane );
        upHeldItem.conveyorItem.Destroy();
        upHeldItem.Destroy();
        lane.Add( upItem );
        upItem.SetPosition( new Vector3( position.x , upItem.position.y , upItem.position.z ) );
        upItem.changeLane = upItem.ChangeLane( -1 );

        HeldItem downHeldItem = new HeldItem( new ConveyorItem( lane.stage.conveyor , ConveyorItem.Type.Part ) );
        LaneItem downItem = new LaneItem( downHeldItem , lane );
        downHeldItem.conveyorItem.Destroy();
        downHeldItem.Destroy();
        lane.Add( downItem );
        downItem.SetPosition( new Vector3( position.x , downItem.position.y , downItem.position.z ) );
        downItem.changeLane = downItem.ChangeLane( 1 );
    }

    public void LeapEntity( LaneEntity laneEntity ) => leap = Leap( laneEntity );

    public void SetPosition( Vector3 position ) => this.position = position;

    private IEnumerator Leap( LaneEntity laneEntity )
    {
        cube.transform.localPosition = Vector3.up;

        while ( laneEntity.valid && back > laneEntity.back )
            yield return null;

        cube.transform.localPosition = Vector3.zero;
    }

    public override LaneEntity overlapping
    {
        get
        {
            bool collide = false;

            for ( int i = 0 ; lane.objects.Count > i && !collide ; i++ )
                if ( lane.objects[ i ] is LaneEntity && ( lane.objects[ i ].Contains( frontPoint ) || lane.objects[ i ].Contains( frontPoint + ( Vector3.forward * scale.z * 0.5f ) ) || lane.objects[ i ].Contains( frontPoint + ( Vector3.back * scale.z * 0.5f ) ) ) )
                    return lane.objects[ i ] as LaneEntity;

            return null;
        }
    }

    public ConveyorItem.Type type => heldItem != null ? heldItem.conveyorItem.type : ConveyorItem.Type.Wreck;
    public int damage => heldItem != null ? heldItem.conveyorItem.level + 1 : 1;
    public HeldItem heldItem { get; }

    public override float front => rect.xMin;
    public override float back => rect.xMax;

    protected override float start => lane.end.x;
    protected override float end => lane.start.x;
    protected override float speed => lane.speed;

    private IEnumerator leap { get; set; }

    public LaneItem ( HeldItem heldItem , Lane lane ) : base( "Lane" + heldItem.conveyorItem.type.ToString() , lane)
    {
        cube.transform.localScale = new Vector3( heldItem.conveyorItem.width , 1 , heldItem.conveyorItem.height );

        meshRenderer.material.color = Color.white;
        textMesh.text = heldItem.conveyorItem.text;

        position = lane.end + ( Vector3.up * 0.5f ) + ( Vector3.left * cube.transform.localScale.x * 0.5f );
        this.heldItem = heldItem;
    }

    public LaneItem ( Lane lane , string name , float width , float height , Vector3 position ) : base( "Lane" + name , lane )
    {
        cube.transform.localScale = new Vector3( width , 1 , height );

        meshRenderer.material.color = Color.grey;
        textMesh.color = Color.white;
        textMesh.text = name;

        this.position = position;
        heldItem = null;
    }
}

public class Enemy : LaneEntity
{
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }

    public Enemy( EnemyDefinition enemyDefinition , Lane lane ) : base ( enemyDefinition.name , enemyDefinition.speed , enemyDefinition.width , enemyDefinition.laneHeightPadding , enemyDefinition.health , enemyDefinition.value , lane )
    {
        color = enemyDefinition.color;
    }
}

public class LaneEntity : LaneObject
{
    /// <summary>
    /// Updates the item's position, cleaning it up if it reaches the end of the lane
    /// </summary>
    public override IEnumerator Update()
    {
        bool move = true;

        while ( container != null )
        {
            if ( overlap )
                Interaction( overlapping );

            if ( pushBack != null )
                move = !pushBack.MoveNext();

            if ( changeLane != null )
                move = !changeLane.MoveNext();

            float x = position.x + ( ( speed * Time.deltaTime ) * ( move ? 1 : 0 ) );
            bool destroy = x > lane.end.x - ( cube.transform.localScale.x * 0.5f ) || 0 >= _health;
            position = new Vector3( Mathf.Clamp( x , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

            if ( destroy )
                Destroy();

            yield return null;
        }
    }

    public override void Destroy()
    {
        if ( _health == 0 )
        {
            lane.stage.AddCoins( defeatValue );
            lane.Add( new LaneItem( lane , "Wreck" , scale.x , scale.z , position ) );
        }

        base.Destroy();
    }

    private void Damage ( int damage = 1 )
    {
        _healthBar.Decrease( damage );

        if ( _health > 0 )
            lane.stage.AddCoins( damageValue );
    }

    public void Interaction<T> ( T laneObject ) where T : LaneObject
    {
        if ( laneObject is LaneItem )
        {
            LaneItem laneItem = laneObject as LaneItem;

            switch ( laneItem.type )
            {
                case ConveyorItem.Type.Part:
                case ConveyorItem.Type.Damage:
                    Damage( laneItem.damage );
                    pushBack = PushBack();
                    laneItem.Destroy();
                    break;

                case ConveyorItem.Type.Split:
                    Damage( laneItem.damage );
                    pushBack = PushBack();
                    laneItem.Split();
                    laneItem.Destroy();
                    break;

                case ConveyorItem.Type.Leap:
                case ConveyorItem.Type.Wreck:
                    Damage( laneItem.damage );
                    pushBack = PushBack();
                    laneItem.LeapEntity( this );
                    break;

                case ConveyorItem.Type.LaneDown:
                    changeLane = ChangeLane( laneItem.heldItem.conveyorItem.level + 1 );
                    laneItem.Destroy();
                    break;

                case ConveyorItem.Type.LaneUp:
                    changeLane = ChangeLane( -laneItem.heldItem.conveyorItem.level - 1 );
                    laneItem.Destroy();
                    break;
            }
        }
        else
        {
            LaneEntity laneEntity = laneObject as LaneEntity;
            LaneEntity front = position.x > laneEntity.position.x ? this : laneEntity;
            LaneEntity back = front == this ? laneEntity : this;

            if ( ( back.scale.z * 0.5f > Mathf.Abs( front.position.z - back.position.z ) ) && ( front.Contains( back.frontPoint ) || front.Contains( back.frontPoint + ( Vector3.forward * back.scale.z * 0.5f ) ) || front.Contains( back.frontPoint + ( Vector3.back * back.scale.z * 0.5f ) ) ) )
            {
                back.position = new Vector3( front.back - ( front.scale.x * 0.6f ) , back.position.y , back.position.z );
                back.pushBack = back.PushBack();
                back.Damage();
            }
            else
            {
                bool up = position.z > laneEntity.position.z;
                position = new Vector3( position.x , position.y , up ? laneEntity.top + ( scale.z * 0.6f ) : laneEntity.bottom - ( scale.z * 0.6f ) );
                changeLane = ChangeLane( up ? -1 : 1 );
                Damage();
            }
        }
    }

    private IEnumerator PushBack()
    {
        float current = position.x;
        float target = current - 3;
        float targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( current , target , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );
    }

    public override LaneEntity overlapping
    {
        get
        {
            bool collide = false;

            for ( int i = 0 ; lane.objects.Count > i && !collide ; i++ )
                if ( lane.objects[ i ] is LaneEntity && lane.objects[ i ] != this && ( lane.objects[ i ].Contains( frontPoint ) || lane.objects[ i ].Contains( frontPoint + ( Vector3.forward * scale.z * 0.5f ) ) || lane.objects[ i ].Contains( frontPoint + ( Vector3.back * scale.z * 0.5f ) ) || lane.objects[ i ].Contains( topPoint ) || lane.objects[ i ].Contains( topPoint + ( Vector3.left * scale.x * 0.5f ) ) || lane.objects[ i ].Contains( topPoint + ( Vector3.right * scale.x * 0.5f ) ) || lane.objects[ i ].Contains( bottomPoint ) || lane.objects[ i ].Contains( bottomPoint + ( Vector3.left * scale.x * 0.5f ) ) || lane.objects[ i ].Contains( bottomPoint + ( Vector3.right * scale.x * 0.5f ) ) ) )
                    return lane.objects[ i ] as LaneEntity;

            return null;
        }
    }

    public int damageValue => Mathf.CeilToInt( defeatValue / ( _healthBar.initialValue - 1 ) );
    public int defeatValue => Mathf.CeilToInt( ( _value * 0.5f ) );

    public override float front => rect.xMax;
    public override float back => rect.xMin;

    protected override float start => lane.start.x;
    protected override float end => lane.end.x;

    protected override float speed => base.speed - lane.speed;

    private int _value { get; }
    private int _health => _healthBar.value;
    private HealthBar _healthBar { get; }

    public LaneEntity( string name , float speed , float width , float laneHeightPadding , int health , int value , Lane lane ) : base( "Lane" + name , lane , speed )
    {
        cube.transform.localScale = new Vector3( width , 1 , lane.height - laneHeightPadding );

        position = lane.start + ( Vector3.up * 0.5f ) + ( Vector3.right * cube.transform.localScale.x * 0.5f );
        meshRenderer.material.color = Color.white;
        textMesh.text = name;

        _value = value;
        _healthBar = new HealthBar( scale.x , base.lane.stage.laneSpacing , 0.1f , 0.1f , 1 , health );
        _healthBar.SetParent( container.transform , Vector3.forward * ( ( scale.z + base.lane.stage.laneSpacing + laneHeightPadding ) * 0.5f ) );
    }
}

public class HealthBar
{
    public void Increase( int value = 1 ) => SetValue( this.value + value );

    public void Decrease( int value = 1 ) => SetValue( this.value - value );

    private void SetValue( int value )
    {
        bool different = Mathf.Clamp( value , 0 , initialValue ) != this.value;
        this.value = Mathf.Clamp( value , 0 , initialValue );

        if ( different )
            for ( int i = 0 ; _segments.Count > i ; i++ )
                _segments[ i ].material.color = this.value > i ? Color.red : Color.black;
    }

    public void SetParent( Transform parent , Vector3 localPosition )
    {
        _container.transform.SetParent( parent );
        _container.transform.localPosition = localPosition;
    }

    public int value { get; private set; }
    public int initialValue { get; }

    private List<MeshRenderer> _segments { get; }
    private GameObject _container { get; }
    private MeshRenderer _quad { get; }
    private float _width { get; }
    private float _height { get; }

    public HealthBar( float width , float height , float spacing , float padding , int rows , int value )
    {
        _container = new GameObject( "HealthBar" );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localScale = new Vector3( width , height , 1 );
        _quad.transform.name = "HealthBG";

        _width = width;
        _height = height;
        initialValue = this.value = value;

        int perRow = Mathf.CeilToInt( value / rows );
        int remainder = value - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) )  ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) )  ) / rows );
        _segments = new List<MeshRenderer>( value );

        for ( int i = 0 ; value > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            MeshRenderer segment = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
            segment.transform.SetParent( _container.transform );
            segment.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
            segment.transform.localScale = new Vector3( size.x , size.y , 1 );
            segment.transform.localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            segment.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            segment.material.color = Color.red;
            segment.name = "Segment" + i;
            _segments.Add( segment );
            x++;
        }
    }
}

public abstract class LaneObject
{
    public abstract IEnumerator Update();

    public IEnumerator ChangeLane( int change )
    {
        Stage stage = lane.stage;
        int laneIndex = stage.IndexOf( lane );
        Lane newLane = stage.LaneBy( Mathf.Clamp( laneIndex + change , 0 , stage.lanes - 1 ) );
        bool outOfBounds = newLane == lane;

        if ( !outOfBounds )
        {
            lane.Remove( this );
            lane = newLane;
            lane.Add( this );
        }

        return ChangeLane( position.z , outOfBounds ? ( lane.start.z - lane.height - lane.stage.laneSpacing ) * Mathf.Sign( change ) : lane.start.z , outOfBounds );
    }

    private IEnumerator ChangeLane( float current , float target , bool outOfBounds )
    {
        float targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( position.x , position.y , Mathf.Lerp( current , target , 1f - ( targetTime - Time.time ) ) );

        position = new Vector3( position.x , position.y , target );

        if ( outOfBounds )
        {
            IEnumerator changeLane = ChangeLane( target , current , false );

            while ( changeLane.MoveNext() )
                yield return changeLane.Current;
        }
    }

    /// <summary>
    /// Remove the item from the lane then destroy the scene graph representation
    /// </summary>
    public virtual void Destroy()
    {
        lane.Remove( this );
        GameObject.Destroy( container );
    }

    public bool Contains( Vector3 position ) => rect.Contains( new Vector2( position.x , position.z ) );

    public IEnumerator update { get; private set; }

    public float top => rect.yMax;
    public float bottom => rect.yMin;
    public bool valid => container != null;
    public Vector3 topPoint => new Vector3( rect.center.x , 0 , top );
    public Vector3 backPoint => new Vector3( back , 0 , rect.center.y );
    public Vector3 frontPoint => new Vector3( front , 0 , rect.center.y );
    public Vector3 bottomPoint => new Vector3( rect.center.x , 0 , bottom );
    public bool overlap => overlapping != null && Mathf.Approximately( cube.transform.localPosition.y , 0 );
    public Rect rect => new Rect( position.x - ( scale.x * 0.5f ) , position.z - ( scale.z * 0.5f ) , scale.x , scale.z );
    public Vector3 scale { get { return cube.transform.localScale; } protected set { cube.transform.localScale = value; } }
    public Vector3 position { get { return container.transform.position; } protected set { container.transform.position = value; } }

    public abstract LaneEntity overlapping { get; }
    public abstract float front { get; }
    public abstract float back { get; }

    protected Lane lane { get; set; }
    protected GameObject cube { get; }
    protected TextMesh textMesh { get; }
    protected GameObject container { get; }
    protected MeshRenderer meshRenderer { get; }
    protected IEnumerator changeLane { get; set; }
    protected IEnumerator pushBack { get; set; }

    protected virtual float speed { get; }

    protected abstract float start { get; }
    protected abstract float end { get; }

    public LaneObject( string containerName , Lane lane , float speed = 0 )
    {
        update = Update();
        this.lane = lane;
        this.speed = speed;
        container = new GameObject( containerName );

        cube = GameObject.CreatePrimitive( PrimitiveType.Cube );
        cube.transform.SetParent( container.transform );
        cube.transform.localRotation = Quaternion.identity;

        meshRenderer = cube.GetComponent<MeshRenderer>();
        meshRenderer.material = Entry.instance.unlitColor;

        textMesh = new GameObject( containerName + "Label" ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( container.transform );
        textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        textMesh.fontSize = 35;
        textMesh.color = Color.black;
        textMesh.characterSize = 0.15f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }
}

public abstract class MouseObject
{
    public abstract void Destroy();

    public string text => textMesh.text;
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }
    public Vector3 position { get { return container.transform.position; } protected set { container.transform.position = value; } }

    protected GameObject quad { get; }
    protected TextMesh textMesh { get; }
    protected GameObject container { get; }
    protected MeshRenderer meshRenderer { get; }

    public MouseObject( string name )
    {
        container = new GameObject( "Held" + name );
        quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        quad.transform.SetParent( container.transform );
        meshRenderer = quad.GetComponent<MeshRenderer>();
        meshRenderer.material = Entry.instance.unlitColor;
        textMesh = new GameObject( name ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( container.transform );
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.15f;
        textMesh.color = Color.black;
        textMesh.fontSize = 35;
    }
}

public class Level
{
    public void Update()
    {
        if ( _waves.Count > 0 && time > _waves.Peek().time )
        {
            time = 0;
            IEnumerator handler = WaveHandler( _waves.Dequeue() );
            _currentHandlers.Add( handler );
            Updater += handler.MoveNext;
        }

        Updater();
        _progress.Update();
        time += Time.deltaTime;
    }

    public void Add( Wave wave ) => _waves.Enqueue( wave );

    private IEnumerator WaveHandler( Wave wave )
    {
        float time = 0;
        _currentWaves.Add( wave );

        while ( wave.events > 0 )
        {
            wave.Update( time += Time.deltaTime );
            yield return null;
        }

        IEnumerator handler = _currentHandlers[ _currentWaves.IndexOf( wave ) ];
        _currentWaves.Remove( wave );
        Updater -= handler.MoveNext;
    }

    public void ShowProgress() => _progress.Show();
    public void HideProgress() => _progress.Hide();

    public int waves => _waves.Count + _currentWaves.Count;
    public float duration { get; private set; }
    public float time { get; private set; }
    public float progress => _progress.progress;

    private LevelProgress _progress { get; }
    private List<IEnumerator> _currentHandlers { get; }
    private List<Wave> _currentWaves { get; }
    private Queue<Wave> _waves { get; }
    private event Func<bool> Updater;

    public Level( float duration )
    {
        Updater += () => false;
        this.duration = duration;
        _waves = new Queue<Wave>();
        _currentWaves = new List<Wave>();
        _currentHandlers = new List<IEnumerator>();
        _progress = new LevelProgress( this , ( Vector3.forward * 2 ) + ( Vector3.right * 31.175f ) , 5 , 1 , duration );
    }
}

public class Wave
{
    public void Update( float time )
    {
        for ( int i = 0 ; _events.Count > i ; i++ )
            if ( time > _events[ i ].delay )
                _queue.Enqueue( _events[ i ] );

        while ( _queue.Count > 0 )
        {
            WaveEvent waveEvent = _queue.Dequeue();
            bool handled = _stage.Handle( waveEvent );

            if ( handled )
                _events.Remove( waveEvent );
        }
    }

    public void Add( WaveEvent waveEvent ) => _events.Add( waveEvent );

    public int events => _events.Count;
    public float time { get; private set; }

    private List<WaveEvent> _events { get; }
    private Queue<WaveEvent> _queue { get; }
    private Stage _stage { get; }

    public Wave( float time , Stage stage )
    {
        _stage = stage;
        this.time = time;
        _events = new List<WaveEvent>();
        _queue = new Queue<WaveEvent>();
    }
}

public class SpawnEnemyEvent : WaveEvent
{
    public EnemyDefinition enemyDefinition { get; }

    public SpawnEnemyEvent ( EnemyDefinition enemyDefinition , float delay , int lane ) : base ( delay , lane , Type.SpawnEnemy )
    {
        this.enemyDefinition = enemyDefinition;
    }
}

public abstract class WaveEvent
{
    public float delay { get; }
    public Type type { get; }
    public int lane { get; }

    public WaveEvent ( float delay , int lane , Type type )
    {
        this.delay = delay;
        this.lane = lane;
        this.type = type;
    }

    public enum Type
    {
        SpawnEnemy
    }
}

public class EnemyDefinition : EntityDefinition
{
    public Color color { get; }
    public float speed { get; }
    public int health { get; }

    public EnemyDefinition( string name , Color color , float width , float laneHeightPadding , float speed , int health , int value ) : base( name , width , laneHeightPadding , value )
    {
        this.color = color;
        this.speed = speed;
        this.health = health;
    }
}

public abstract class EntityDefinition
{
    public int value { get; }
    public string name { get; }
    public float width { get; }
    public float laneHeightPadding { get; }

    public EntityDefinition( string name , float width , float laneHeightPadding , int value )
    {
        this.name = name;
        this.width = width;
        this.value = value;
        this.laneHeightPadding = laneHeightPadding;
    }
}

public static class Definitions
{
    public static EnemyDefinition Enemy ( Enemies enemy ) => enemyDefinitions[ ( int ) enemy ];

    private static List<EnemyDefinition> enemyDefinitions { get; }

    static Definitions()
    {
        enemyDefinitions = new List<EnemyDefinition>( ( int ) Enemies.Count )
        {
            new EnemyDefinition( "Enemy" , Color.white , 5 , 1 , 10 , 3 , 6 )
        };
    }

    public enum Enemies
    {
        Default = 0,
        Count = 1
    }
}

public class Player
{
    public void AddCoins( int value ) => coins += value;

    public string name { get; private set; }
    public int coins { get; private set; }

    public Player()
    {
        name = "Player";
        coins = 0;
    }

    public Player( Player player )
    {
        name = player.name;
        coins = player.coins;
    }
}

public class CoinCounter
{
    public void Show() => _container.SetActive( true );
    public void Hide() => _container.SetActive( false );
    public void SetCounterValue( int value ) => textMesh.text = value.ToString();

    public TextMesh textMesh { get; }

    private GameObject _container { get; }
    private GameObject _quad { get; }

    public CoinCounter( int value = 0 )
    {
        _container = new GameObject( "CoinCounter" );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localScale = new Vector3( 2 , 1 , 1 );

        textMesh = new GameObject( "Counter" ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( _container.transform );
        textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.15f;
        textMesh.color = Color.black;
        textMesh.fontSize = 35;

        textMesh.text = value.ToString();
        _container.transform.position += new Vector3( -1.75f , 0 , 2 );
    }
}

public class LevelProgress
{
    public void Update()
    {
        _indicator.transform.localPosition = new Vector3( Mathf.Lerp( _start , _end , progress ) , _indicator.transform.localPosition.y , _indicator.transform.localPosition.z );
    }

    public void Show() => _container.SetActive( true );
    public void Hide() => _container.SetActive( false );

    public float progress => Mathf.Clamp( _level.time , 0 , _duration ) / _duration;

    private float _start => ( _height * 0.5f ) - ( _width * 0.5f );
    private float _end => ( _width * 0.5f ) - ( _height * 0.5f );

    private float _duration { get; }
    private float _width { get; }
    private float _height { get; }
    private Level _level { get; }
    private GameObject _bar { get; set; }
    private GameObject _indicator { get; set; }
    private GameObject _container { get; set; }

    public LevelProgress( Level level , Vector3 position , float width , float height , float duration )
    {
        _duration = duration;
        _height = height;
        _width = width;
        _level = level;

        _container = new GameObject( "LevelProgress" );
        _container.transform.position = position;

        _bar = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _bar.transform.SetParent( _container.transform );
        _bar.transform.localPosition = Vector3.zero;
        _bar.transform.localScale = new Vector3( width , height , 1 );
        _bar.GetComponent<MeshRenderer>().material.color = Color.black;


        _indicator = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _indicator.transform.SetParent( _container.transform );
        _indicator.transform.localPosition = ( Vector3.left * width * 0.5f ) + ( Vector3.right * height * 0.5f ) + ( Vector3.back * 0.1f );
        _indicator.transform.localScale = new Vector3( height , height , 1 );

        _container.transform.transform.rotation = Quaternion.Euler( 90 , 0 , 0 );
    }
}