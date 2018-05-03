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
	void Start ()
    {
        instance = this;
        session = new Session( width: 25 , height: 15 , spacing: 1 , lanes: 5 );
	}

    /// <summary>
    /// Updates the session
    /// Update is another special MonoBehaviour method. It gets called once a frame.
    /// </summary>
    void Update() => session.Update();

    /// <summary>
    /// Singleton-ish instance reference. Useful for running co-routines and accessing assets
    /// </summary>
    public static Entry instance { get; private set; }

    /// <summary>
    /// The session drives the game
    /// </summary>
    private Session session { get; set; }
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
            hoveredLane = level.GetHoveredLane( mousePosition );

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

            if ( hoveredLane != null && Input.GetMouseButtonDown( 1 ) )
                hoveredLane.AddEntityToLane( "Entity" , 10 , 5 , 1 );

            //Reset lane colors
            level.SetLaneColor( Color.black );

            //Proceed if a lane is hovered and an item is held
            if ( heldItem != null && hoveredLane != null )
            {
                int laneIndex = level.IndexOf( hoveredLane );
                int lane1Up = laneIndex - 1 >= 0 ? laneIndex - 1 : -1;
                int lane1Down = level.lanes > laneIndex + 1 ? laneIndex + 1 : -1;

                hoveredLane.color = Color.yellow;

                switch ( heldItem.conveyorItem.level )
                {
                    case 0:
                        break;

                    case 1:
                        if ( lane1Up >= 0 )
                            level.LaneBy( lane1Up ).color = Color.yellow;

                        if ( lane1Down >= 0 )
                            level.LaneBy( lane1Down ).color = Color.yellow;
                        break;
                }
                
                //Proceed if the left mouse button is not held
                //This will only happen if the left mouse button is released
                if ( !Input.GetMouseButton( 0 ) )
                {
                    //Add the item to the lane and clean up
                    switch ( heldItem.conveyorItem.level )
                    {
                        case 0:
                            hoveredLane.AddItemToLane( heldItem );
                            break;

                        case 1:

                            if ( lane1Up >= 0 )
                                level.LaneBy( lane1Up ).AddItemToLane( heldItem );

                            if ( lane1Down >= 0 )
                                level.LaneBy( lane1Down ).AddItemToLane( heldItem );

                            hoveredLane.AddItemToLane( heldItem );
                            break;

                        case 2:
                            hoveredLane.AddItemToLane( heldItem );
                            break;

                        case 3:
                            hoveredLane.AddItemToLane( heldItem );
                            break;
                    }

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

        //Update the level
        level.Update();

        //Proceed if the item spawn interval has elapsed, and add a new item to the conveyor belt
        if ( Time.time > itemTime )
            itemTime = conveyor.AddItemToConveyor();
    }

    /// <summary>
    /// Level holds the lanes and methods for operating on them
    /// </summary>
    public Level level { get; private set; }

    /// <summary>
    /// Conveyor handles items and methods for operating on them
    /// </summary>
    public Conveyor conveyor { get; private set; }

    /// <summary>
    /// Getter using UnityEngine's Camera class, which features a convenience getter for the first camera in the scene with the tag "Main"
    /// </summary>
    private Camera camera => Camera.main;

    /// <summary>
    /// Ground plane GameObject. Has a BoxCollider attached
    /// </summary>
    private GameObject ground { get; set; }

    /// <summary>
    /// Last time an item was added to the conveyor
    /// </summary>
    private float itemTime { get; set; }

    /// <summary>
    /// Currently held item
    /// </summary>
    private HeldItem heldItem { get; set; }

    public Session ( float width , float height , float spacing , int lanes )
    {
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

        level = new Level(
            speed: 5 ,
            width: width ,
            height: height ,
            laneSpacing: spacing ,
            laneCount: lanes , 
            conveyor: conveyor );

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
public class Level
{
    /// <summary>
    /// Updates all lanes
    /// Should strictly speaking be an event, but right now this is safe
    /// </summary>
    public void Update()
    {
        for ( int i = 0 ; _lanes.Count > i ; i++ )
            _lanes[ i ].Update();
    }

    /// <summary>
    /// Set color of all lanes except optional
    /// </summary>
    /// <param name="color">Color to apply</param>
    /// <param name="except">Lane to except</param>
    public void SetLaneColor( Color color  )
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

    public int IndexOf( Lane lane ) => _lanes.IndexOf(lane );
    public Lane LaneBy( int index ) => _lanes[ index ];

    /// <summary>
    /// Set the speed at which items move down the conveyor
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed( float speed ) => this.speed = speed;

    public int lanes => _lanes.Count;
    public float laneSpacing { get; private set; }
    public Conveyor conveyor { get; private set; }
    public float speed { get; private set; }

    private List<Lane> _lanes { get; set; }

    public Level ( float speed , float width , float height , float laneSpacing , int laneCount , Conveyor conveyor )
    {
        this.speed = speed;
        this.conveyor = conveyor;
        this.laneSpacing = laneSpacing;
        float laneHeight = height / laneCount;
        _lanes = new List<Lane>( laneCount );

        for ( int i = 0 ; laneCount > i ; i++ )
            _lanes.Add( new Lane( 
                level: this , 
                depth: ( i * ( laneHeight + laneSpacing  ) ) + ( laneHeight * 0.5f ) , 
                width: width , 
                height: laneHeight , 
                name: "Lane" + i ) );
    }
}

/// <summary>
/// Lanes hold items and eventually much, much more
/// </summary>
public class Lane
{
    /// <summary>
    /// Updates all the items on this lane
    /// Should strictly speaking be an event, but right now this is safe
    /// </summary>
    public void Update()
    {
        LaneItemUpdater();
        LaneEntityUpdater();
    }

    /// <summary>
    /// Add a held item to the lane
    /// </summary>
    /// <param name="heldItem">Currently held item</param>
    /// <returns>Lane item corresponding to held item</returns>
    public LaneItem AddItemToLane( HeldItem heldItem )
    {
        LaneItem laneItem = new LaneItem( heldItem , this );
        LaneItemUpdater += laneItem.updater.MoveNext;
        laneItems.Add( laneItem );
        return laneItem;
    }

    /// <summary>
    /// Add a held item to the lane
    /// </summary>
    /// <param name="laneItem">Currently held item</param>
    /// <returns>Lane item corresponding to held item</returns>
    public LaneItem AddItemToLane( LaneItem laneItem )
    {
        LaneItemUpdater += laneItem.updater.MoveNext;
        laneItems.Add( laneItem );
        return laneItem;
    }

    /// <summary>
    /// Add an entity to the lane
    /// </summary>
    /// <param name="name">Name of the entity</param>
    /// <param name="speed">Speed of the entity</param>
    /// <param name="width">Width of the entity</param>
    /// <param name="laneHeightPadding">Amount to subtract from lane height to give height</param>
    /// <returns>The added lane entity</returns>
    public LaneEntity AddEntityToLane( string name , float speed , float width , float laneHeightPadding )
    {
        LaneEntity laneEntity = new LaneEntity( name , speed , width , laneHeightPadding , this );
        LaneEntityUpdater += laneEntity.updater.MoveNext;
        laneEntities.Add( laneEntity );
        return laneEntity;
    }

    public LaneEntity AddEntityToLane( LaneEntity laneEntity )
    {
        LaneEntityUpdater += laneEntity.updater.MoveNext;
        laneEntities.Add( laneEntity );
        return laneEntity;
    }

    /// <summary>
    /// Remove an item from the lane
    /// </summary>
    /// <param name="laneItem">The item to remove from the lane</param>
    public void RemoveItemFromLane( LaneItem laneItem )
    {
        LaneItemUpdater -= laneItem.updater.MoveNext;
        laneItems.Remove( laneItem );
    } 

    /// <summary>
    /// Remove an entity from the lane
    /// </summary>
    /// <param name="laneEntity">The entity to remove from the lane</param>
    public void RemoveEntityFromLane( LaneEntity laneEntity )
    {
        LaneEntityUpdater -= laneEntity.updater.MoveNext;
        laneEntities.Remove( laneEntity );
    }

    /// <summary>
    /// Check if the lane's rect contains a world-space position
    /// The position is projected to 2D
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if the rect contains the point, false if not</returns>
    public bool Contains ( Vector3 position ) => _rect.Contains( new Vector2( position.x , position.z ) );

    public Color color { get { return _meshRenderer.material.color; } set { _meshRenderer.material.color = value; } }
    public Vector3 start => new Vector3( _rect.xMin , 0 , _rect.yMin + ( height * 0.5f ) );
    public Vector3 end => new Vector3( _rect.xMax , 0 , _rect.yMin + ( height * 0.5f ) );
    public float width => _rect.width;
    public float height => _rect.height;
    public float speed => level.speed;

    public Level level { get; private set; }
    public List<LaneItem> laneItems { get; private set; }
    public List<LaneEntity> laneEntities { get; private set; }

    /// <summary>
    /// Why a rect? Why not a collider like the ground plane?
    /// Because this has way, way, waaaay fewer moving parts and much fewer things that can go wrong
    /// Since all the mouse positions we want are on the same plane, we can project them to 2D
    /// Rects are fast, and best of all -- they're structs, so we can create and discard them willy-nilly!
    /// </summary>
    private Rect _rect { get; set; }
    private GameObject _quad { get; set; }
    private MeshRenderer _meshRenderer { get; set; }
    private event Func<bool> LaneEntityUpdater;
    private event Func<bool> LaneItemUpdater;

    public Lane( Level level , float depth , float width , float height , string name )
    {
        LaneEntityUpdater += () => false;
        LaneItemUpdater += () => false;

        this.level = level;
        _rect = new Rect( 0 , -depth , width , height );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.position = new Vector3( width * 0.5f , 0 , -depth + ( height * 0.5f ) );
        _quad.transform.localScale = new Vector3( width , height , 0 );
        _quad.transform.name = name;

        _quad.GetComponent<MeshCollider>().enabled = false;
        _meshRenderer = _quad.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;

        laneItems = new List<LaneItem>();
        laneEntities = new List<LaneEntity>();
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

    private Rect _rect { get; set; }
    private int _itemLimit { get; set; }
    private GameObject _quad { get; set; }
    private MeshRenderer _meshRenderer { get; set; }
    private List<ConveyorItem> _conveyorItems { get; set; }

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
public class ConveyorItem
{
    /// <summary>
    /// Remove the item from the conveyor then destroy the scene graph representation
    /// </summary>
    public void Destroy()
    {
        _conveyor.RemoveItemFromConveyor( this );
        GameObject.Destroy( _container );
    }

    /// <summary>
    /// Set the item's held flag
    /// </summary>
    /// <param name="held">The new held flag state</param>
    public void SetHeld( bool held ) => this.held = held;

    /// <summary>
    /// Upgrade the item and update the label
    /// </summary>
    public void Upgrade() => _textMesh.text = type.ToString() + "\n" + ++level;

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
    public Type type { get; private set; }
    public Vector3 position { get { return _container.transform.position; } private set { _container.transform.position = value; } }
    public Color color { get { return _meshRenderer.material.color; } set { _meshRenderer.material.color = value; } }
    public string text => _textMesh.text;

    private float speed => _conveyor.speed;
    private float limit => bottom + ( height * 0.5f ) + ( ( height + itemSpacing ) * index );
    private float itemSpacing => _conveyor.itemSpacing;
    private float bottom => _conveyor.bottom.z;
    private float top => _conveyor.top.z;

    private GameObject _quad { get; set; }
    private TextMesh _textMesh { get; set; }
    private Conveyor _conveyor { get; set; }
    private GameObject _container { get; set; }
    private MeshRenderer _meshRenderer { get; set; }
    private int _maxLevel { get; } = 3;

    public ConveyorItem ( Conveyor conveyor , Type type )
    {
        string name = type.ToString();

        _conveyor = conveyor;
        _container = new GameObject( "Conveyor" + name );
        _container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.identity;
        _quad.transform.localScale = new Vector3( width , height , 1 );

        _meshRenderer = _quad.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;
        _meshRenderer.material.color = Color.white;

        _textMesh = new GameObject( name ).AddComponent<TextMesh>();
        _textMesh.transform.SetParent( _container.transform );
        _textMesh.transform.localRotation = Quaternion.identity;


        _textMesh.text = name + "\n" + level;
        _textMesh.fontSize = 35;
        _textMesh.color = Color.black;
        _textMesh.characterSize = 0.15f;
        _textMesh.anchor = TextAnchor.MiddleCenter;
        _textMesh.alignment = TextAlignment.Center;

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
    }
}

/// <summary>
/// Items picked up from the conveyor belt. Can be dropped on lanes
/// </summary>
public class HeldItem
{
    /// <summary>
    /// Drops the currently held item then destroy the scene graph representation
    /// </summary>
    public void Destroy()
    {
        conveyorItem.SetHeld( false );
        GameObject.Destroy( _container );
    }

    /// <summary>
    /// Set the item's position. Height is relative to the corresponding conveyor item
    /// </summary>
    /// <param name="position">Position to place the item at</param>
    public void SetPosition ( Vector3 position ) => this.position = new Vector3( position.x , conveyorItem.position.y + 1 , position.z );

    public Vector3 position { get { return _container.transform.position; } private set { _container.transform.position = value; } }
    public ConveyorItem conveyorItem { get; private set; }

    private GameObject _quad { get; set; }
    private TextMesh _textMesh { get; set; }
    private GameObject _container { get; set; }
    private MeshRenderer _meshRenderer { get; set; }

    public HeldItem( ConveyorItem conveyorItem )
    {
        conveyorItem.SetHeld( true );
        conveyorItem.color = Color.gray;
        string name = conveyorItem.type.ToString();

        _container = new GameObject( "Held" + name );
        _container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.identity;
        _quad.transform.localScale = new Vector3( conveyorItem.width , conveyorItem.height , 1 );

        _meshRenderer = _quad.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;
        _meshRenderer.material.color = Color.white;

        _textMesh = new GameObject( name ).AddComponent<TextMesh>();
        _textMesh.transform.SetParent( _container.transform );
        _textMesh.transform.localRotation = Quaternion.identity;

        _textMesh.fontSize = 35;
        _textMesh.color = Color.black;
        _textMesh.characterSize = 0.15f;
        _textMesh.anchor = TextAnchor.MiddleCenter;
        _textMesh.alignment = TextAlignment.Center;
        _textMesh.text = conveyorItem.text;

        position = conveyorItem.position + Vector3.forward;
        this.conveyorItem = conveyorItem;
    }
}

/// <summary>
/// Items travelling down lanes. Will eventually interact with many, many other items
/// </summary>
public class LaneItem
{
    /// <summary>
    /// Updates the item's position, cleaning it up if it reaches the end of the lane
    /// </summary>
    public IEnumerator Updater()
    {
        bool move = true;

        while ( true )
        {
            if ( changeLane != null )
                move = !changeLane.MoveNext();

            float x = position.x - ( ( speed * Time.deltaTime ) * ( move ? 1 : 0 ) );
            bool destroy = _lane.start.x + ( _cube.transform.localScale.x * 0.5f ) > x;
            position = new Vector3( Mathf.Clamp( x , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );
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

    /// <summary>
    /// Remove the item from the lane then destroy the scene graph representation
    /// </summary>
    public void Destroy()
    {
        _lane.RemoveItemFromLane( this );
        GameObject.Destroy( _container );
    }

    public void Split()
    {
        HeldItem upHeldItem = new HeldItem( new ConveyorItem( _lane.level.conveyor , ConveyorItem.Type.Part ) );
        LaneItem upItem = new LaneItem( upHeldItem , _lane );
        upHeldItem.conveyorItem.Destroy();
        upHeldItem.Destroy();
        _lane.AddItemToLane( upItem );
        upItem.SetPosition( new Vector3( position.x , upItem.position.y , upItem.position.z ) );
        upItem.changeLane = upItem.ChangeLane( -1 );

        HeldItem downHeldItem = new HeldItem( new ConveyorItem( _lane.level.conveyor , ConveyorItem.Type.Part ) );
        LaneItem downItem = new LaneItem( downHeldItem , _lane );
        downHeldItem.conveyorItem.Destroy();
        downHeldItem.Destroy();
        _lane.AddItemToLane( downItem );
        downItem.SetPosition( new Vector3( position.x , downItem.position.y , downItem.position.z ) );
        downItem.changeLane = downItem.ChangeLane( 1 );
    }

    public void LeapEntity( LaneEntity laneEntity ) => leap = Leap( laneEntity );

    private IEnumerator Leap( LaneEntity laneEntity )
    {
        _cube.transform.localPosition = Vector3.up;

        while ( laneEntity.valid && back.x > laneEntity.back )
            yield return null;

        _cube.transform.localPosition = Vector3.zero;
    }

    public IEnumerator ChangeLane( int change )
    {
        Level level = _lane.level;
        int laneIndex = level.IndexOf( _lane );
        Lane newLane = level.LaneBy( Mathf.Clamp( laneIndex + change , 0 , level.lanes - 1 ) );
        bool outOfBounds = newLane == _lane;

        if ( !outOfBounds )
        {
            _lane.RemoveItemFromLane( this );
            _lane = newLane;
            _lane.AddItemToLane( this );
        }

        return ChangeLane( position.z , outOfBounds ? ( _lane.start.z - _lane.height - _lane.level.laneSpacing ) * Mathf.Sign( change ) : _lane.start.z , outOfBounds );
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

    public void SetPosition( Vector3 position ) => this.position = position;

    public bool overlap => Mathf.Approximately( _cube.transform.localPosition.y , 0 ) && overlapping != null;

    public LaneEntity overlapping
    {
        get
        {
            bool collide = false;

            for ( int i = 0 ; _lane.laneEntities.Count > i && !collide ; i++ )
                if ( _lane.laneEntities[ i ].Contains( front ) || _lane.laneEntities[ i ].Contains( front + ( Vector3.forward * scale.z * 0.5f ) ) || _lane.laneEntities[ i ].Contains( front + ( Vector3.back * scale.z * 0.5f ) ) )
                    return _lane.laneEntities[ i ];

            return null;
        }
    }

    public IEnumerator updater { get; private set; }
    public ConveyorItem.Type type => _heldItem.conveyorItem.type;
    public Vector3 front => new Vector3( rect.xMin , 0 , rect.center.y );
    public Vector3 back => new Vector3( rect.xMax , 0 , rect.center.y );
    public Rect rect => new Rect( position.x - ( scale.x * 0.5f ) , position.z - ( scale.z * 0.5f ) , scale.x , scale.z );
    public Vector3 position { get { return _container.transform.position; } private set { _container.transform.position = value; } }
    public Vector3 scale { get { return _cube.transform.localScale; } private set { _cube.transform.localScale = value; } }

    private float start => _lane.start.x;
    private float end => _lane.end.x;
    private float speed => _lane.speed;

    private Lane _lane { get; set; }
    private GameObject _cube { get; set; }
    private HeldItem _heldItem { get; set; }
    private TextMesh _textMesh { get; set; }
    private GameObject _container { get; set; }
    private MeshRenderer _meshRenderer { get; set; }
    private IEnumerator leap { get; set; }
    private IEnumerator changeLane { get; set; }

    public LaneItem ( HeldItem heldItem , Lane lane )
    {
        updater = Updater();
        string name = heldItem.conveyorItem.type.ToString();
        _container = new GameObject( "Lane" + name );

        _cube = GameObject.CreatePrimitive( PrimitiveType.Cube );
        _cube.transform.SetParent( _container.transform );
        _cube.transform.localRotation = Quaternion.identity;
        _cube.transform.localScale = new Vector3( heldItem.conveyorItem.width , 1 , heldItem.conveyorItem.height );

        _meshRenderer = _cube.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;
        _meshRenderer.material.color = Color.white;

        _textMesh = new GameObject( name ).AddComponent<TextMesh>();
        _textMesh.transform.SetParent( _container.transform );
        _textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        _textMesh.fontSize = 35;
        _textMesh.color = Color.black;
        _textMesh.characterSize = 0.15f;
        _textMesh.anchor = TextAnchor.MiddleCenter;
        _textMesh.alignment = TextAlignment.Center;
        _textMesh.text = heldItem.conveyorItem.text;

        position = lane.end + ( Vector3.up * 0.5f ) + ( Vector3.left * _cube.transform.localScale.x * 0.5f );
        _heldItem = heldItem;
        _lane = lane;
    }
}

public class LaneEntity
{
    /// <summary>
    /// Updates the item's position, cleaning it up if it reaches the end of the lane
    /// </summary>
    public IEnumerator Updater()
    {
        bool move = true;

        while ( _container != null )
        {
            if ( overlap )
                Interaction( overlapping );

            if ( _pushBack != null )
                move = !_pushBack.MoveNext();

            if ( _changeLane != null )
                move = !_changeLane.MoveNext();

            float x = position.x + ( ( speed * Time.deltaTime ) * ( move ? 1 : 0 ) );
            bool destroy = x > _lane.end.x - ( _cube.transform.localScale.x * 0.5f ) || 0 >= _health;
            position = new Vector3( Mathf.Clamp( x , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

            if ( destroy )
                Destroy();

            yield return null;
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

    private IEnumerator ChangeLane( int change )
    {
        Level level = _lane.level;
        int laneIndex = level.IndexOf( _lane );
        Lane newLane = level.LaneBy( Mathf.Clamp( laneIndex + change , 0 , level.lanes - 1 ) );
        bool outOfBounds = newLane == _lane;

        if ( !outOfBounds )
        {
            _lane.RemoveEntityFromLane( this );
            _lane = newLane;
            _lane.AddEntityToLane( this );
        }

        return ChangeLane( position.z , outOfBounds ? ( _lane.start.z - _lane.height - _lane.level.laneSpacing ) * Mathf.Sign( change ) : _lane.start.z , outOfBounds );
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

    public void Interaction ( LaneItem laneItem )
    {
        switch ( laneItem.type )
        {
            case ConveyorItem.Type.Part:
            case ConveyorItem.Type.Damage:
                _healthBar.Decrease();
                _pushBack = PushBack();
                laneItem.Destroy();
                break;

            case ConveyorItem.Type.Split:
                _healthBar.Decrease();
                _pushBack = PushBack();
                laneItem.Split();
                laneItem.Destroy();
                break;

            case ConveyorItem.Type.Leap:
                _healthBar.Decrease();
                _pushBack = PushBack();
                laneItem.LeapEntity( this );
                break;

            case ConveyorItem.Type.LaneDown:
                _changeLane = ChangeLane( 1 );
                laneItem.Destroy();
                break;

            case ConveyorItem.Type.LaneUp:
                _changeLane = ChangeLane( -1 );
                laneItem.Destroy();
                break;
        }
    }

    public void Interaction( LaneEntity laneEntity )
    {
        LaneEntity front = position.x > laneEntity.position.x ? this : laneEntity;
        LaneEntity back = front == this ? laneEntity : this;

        if ( ( back.scale.z * 0.5f > Mathf.Abs( front.position.z - back.position.z ) ) && ( front.Contains( back.frontPoint ) || front.Contains( back.frontPoint + ( Vector3.forward * back.scale.z * 0.5f ) ) || front.Contains( back.frontPoint + ( Vector3.back * back.scale.z * 0.5f ) ) ) )
        {
            back.position = new Vector3( front.back - ( front.scale.x * 0.6f ) , back.position.y , back.position.z );
            back._pushBack = back.PushBack();
            back._healthBar.Decrease();
        }
        else
        {
            bool up = position.z > laneEntity.position.z;
            position = new Vector3( position.x , position.y , up ? laneEntity.top + ( scale.z * 0.6f ) : laneEntity.bottom - ( scale.z * 0.6f ) );
            _changeLane = ChangeLane( up ? -1 : 1 );
            _healthBar.Decrease();
        }
    }

    /// <summary>
    /// Remove the item from the lane then destroy the scene graph representation
    /// </summary>
    public void Destroy()
    {
        _lane.RemoveEntityFromLane( this );
        GameObject.Destroy( _container );
    }

    public bool Contains( Vector3 position ) => rect.Contains( new Vector2( position.x , position.z ) );

    public bool valid => _container != null;
    public Vector3 backPoint => new Vector3( rect.xMin , 0 , rect.center.y );
    public Vector3 frontPoint => new Vector3( rect.xMax , 0 , rect.center.y );
    public Vector3 topPoint => new Vector3( rect.center.x , 0 , rect.yMax );
    public Vector3 bottomPoint => new Vector3( rect.center.x , 0 , rect.yMin );

    public float front => rect.xMax;
    public float back => rect.xMin;
    public float top => rect.yMax;
    public float bottom => rect.yMin;
    public bool overlap => overlapping != null;

    public LaneEntity overlapping
    {
        get
        {
            bool collide = false;

            for ( int i = 0 ; _lane.laneEntities.Count > i && !collide ; i++ )
                if ( _lane.laneEntities[ i ] != this && ( _lane.laneEntities[ i ].Contains( frontPoint ) || _lane.laneEntities[ i ].Contains( frontPoint + ( Vector3.forward * scale.z * 0.5f ) ) || _lane.laneEntities[ i ].Contains( frontPoint + ( Vector3.back * scale.z * 0.5f ) ) || _lane.laneEntities[ i ].Contains( topPoint ) || _lane.laneEntities[ i ].Contains( topPoint + ( Vector3.left * scale.x * 0.5f ) ) || _lane.laneEntities[ i ].Contains( topPoint + ( Vector3.right * scale.x * 0.5f ) ) || _lane.laneEntities[ i ].Contains( bottomPoint ) || _lane.laneEntities[ i ].Contains( bottomPoint + ( Vector3.left * scale.x * 0.5f ) ) || _lane.laneEntities[ i ].Contains( bottomPoint + ( Vector3.right * scale.x * 0.5f ) ) ) )
                    return _lane.laneEntities[ i ];

            return null;
        }
    }

    public Rect rect => new Rect( position.x - ( scale.x * 0.5f ) , position.z - ( scale.z * 0.5f ) , scale.x , scale.z );
    public Vector3 position { get { return _container.transform.position; } private set { _container.transform.position = value; } }
    public Vector3 scale { get { return _cube.transform.localScale; } private set { _cube.transform.localScale = value; } }

    public IEnumerator updater { get; private set; }

    private float speed => _speed - _lane.speed;
    private float start => _lane.start.x;
    private float end => _lane.end.x;

    private int _health => _healthBar.value;
    private float _speed { get; set; }
    private Lane _lane { get; set; }
    private GameObject _cube { get; set; }
    private TextMesh _textMesh { get; set; }
    private GameObject _container { get; set; }
    private MeshRenderer _meshRenderer { get; set; }
    private IEnumerator _pushBack { get; set; }
    private IEnumerator _changeLane { get; set; }
    private HealthBar _healthBar { get; set; }

    public LaneEntity( string name , float speed , float width , float laneHeightPadding , Lane lane )
    {
        updater = Updater();
        _container = new GameObject( "Lane" + name );

        _cube = GameObject.CreatePrimitive( PrimitiveType.Cube );
        _cube.transform.SetParent( _container.transform );
        _cube.transform.localRotation = Quaternion.identity;
        _cube.transform.localScale = new Vector3( width , 1 , lane.height - laneHeightPadding );

        _meshRenderer = _cube.GetComponent<MeshRenderer>();
        _meshRenderer.material = Entry.instance.unlitColor;
        _meshRenderer.material.color = Color.white;

        _textMesh = new GameObject( name ).AddComponent<TextMesh>();
        _textMesh.transform.SetParent( _container.transform );
        _textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        _textMesh.fontSize = 35;
        _textMesh.color = Color.black;
        _textMesh.characterSize = 0.15f;
        _textMesh.anchor = TextAnchor.MiddleCenter;
        _textMesh.alignment = TextAlignment.Center;
        _textMesh.text = name;

        position = lane.start + ( Vector3.up * 0.5f ) + ( Vector3.right * _cube.transform.localScale.x * 0.5f );
        _speed = speed;
        _lane = lane;

        _healthBar = new HealthBar( scale.x , _lane.level.laneSpacing , 0.1f , 0.1f , 1 , 3 );
        _healthBar.SetParent( _container.transform , Vector3.forward * ( ( scale.z + _lane.level.laneSpacing + laneHeightPadding ) * 0.5f ) );
    }
}

public class HealthBar
{
    public void Increase( int value = 1 ) => SetValue( this.value + value );

    public void Decrease( int value = 1 ) => SetValue( this.value - value );

    private void SetValue( int value )
    {
        bool different = Mathf.Clamp( value , 0 , _initialValue ) != this.value;
        this.value = Mathf.Clamp( value , 0 , _initialValue );

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

    private List<MeshRenderer> _segments { get; set; }
    private GameObject _container { get; set; }
    private MeshRenderer _quad { get; set; }
    private float _width { get; set; }
    private float _height { get; set; }
    private int _initialValue { get; set; }

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
        _initialValue = this.value = value;

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