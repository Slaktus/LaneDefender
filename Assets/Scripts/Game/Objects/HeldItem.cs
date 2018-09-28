using UnityEngine;

public class HeldItem : BaseObject
{
    public override void Destroy()
    {
        conveyorItem.SetHeld( false );
        conveyorItem.Destroy();
        GameObject.Destroy( container );
    }

    public void SetPosition( Vector3 position ) => this.position = new Vector3( position.x , conveyorItem.position.y + 1 , position.z );

    public ConveyorItem conveyorItem { get; private set; }

    public HeldItem(ConveyorItem conveyorItem) : base("Held" + conveyorItem.type.ToString(), GameObject.CreatePrimitive(PrimitiveType.Quad))
    {
        conveyorItem.SetHeld( true );
        conveyorItem.color = Color.gray;

        container.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3( conveyorItem.width , conveyorItem.height , 1 );

        meshRenderer.material.color = Color.white;

        label.SetLocalRotation( Quaternion.identity );
        label.SetText( conveyorItem.text );

        position = conveyorItem.position + Vector3.forward;
        this.conveyorItem = conveyorItem;
    }
}