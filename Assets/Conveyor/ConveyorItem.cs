using UnityEngine;

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

    public ConveyorItem( Conveyor conveyor , Type type ) : base( "Conveyor" + type.ToString() )
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