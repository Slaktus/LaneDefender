using UnityEngine;

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
    public void SetPosition( Vector3 position ) => this.position = new Vector3( position.x , conveyorItem.position.y + 1 , position.z );

    public ConveyorItem conveyorItem { get; private set; }

    public HeldItem( ConveyorItem conveyorItem ) : base( "Held" + conveyorItem.type.ToString() )
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