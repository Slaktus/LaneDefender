using System.Collections.Generic;
using UnityEngine;

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
                _conveyorItems[ i ].PowerUp();
            }

            //Update the conveyor item
            _conveyorItems[ i ].Update();
        }
    }

    /// <summary>
    /// Add an item to the conveyor
    /// </summary>
    /// <returns></returns>
    public float AddItemToConveyor( Inventory inventory )
    {
        if ( _itemLimit > _conveyorItems.Count )
        {
            Definitions.Items item = inventory.items[ Random.Range( 0 , inventory.items.Count ) ];
            _conveyorItems.Add( new ConveyorItem( this , Definitions.Item( item ) , inventory.Settings( item ) ) );
        }

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

    public void Destroy()
    {
        Clear();
        GameObject.Destroy( _quad );
    }

    /// <summary>
    /// Set the speed at which items travel down the conveyor
    /// </summary>
    /// <param name="speed">Speed at which items move down the conveyor</param>
    public void SetSpeed( float speed ) => this.speed = speed;

    /// <summary>
    /// Set the interval at which items spawn at the top of the conveyor
    /// </summary>
    /// <param name="itemInterval">Interval at which to spawn items</param>
    public void SetItemInterval( float itemInterval ) => this.itemInterval = itemInterval;

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
    public bool showing => _quad.activeSelf;
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

    public Conveyor( float speed , float width , float height , float itemInterval , int itemLimit , float itemWidthPadding , float itemSpacing , bool hide = false )
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

        if ( hide )
            Hide();
    }
}