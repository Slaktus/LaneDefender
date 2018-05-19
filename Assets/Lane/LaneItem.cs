using System.Collections;
using UnityEngine;

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
            position = new Vector3( Mathf.Clamp( x , end + ( scale.x * 0.5f ) , start - ( scale.x * 0.5f ) ) , position.y , position.z );
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

    public LaneItem( HeldItem heldItem , Lane lane ) : base( "Lane" + heldItem.conveyorItem.type.ToString() , lane )
    {
        cube.transform.localScale = new Vector3( heldItem.conveyorItem.width , 1 , heldItem.conveyorItem.height );

        meshRenderer.material.color = Color.white;
        textMesh.text = heldItem.conveyorItem.text;

        position = lane.end + ( Vector3.up * 0.5f ) + ( Vector3.left * cube.transform.localScale.x * 0.5f );
        this.heldItem = heldItem;
    }

    public LaneItem( Lane lane , string name , float width , float height , Vector3 position ) : base( "Lane" + name , lane )
    {
        cube.transform.localScale = new Vector3( width , 1 , height );

        meshRenderer.material.color = Color.grey;
        textMesh.color = Color.white;
        textMesh.text = name;

        this.position = position;
        heldItem = null;
    }
}