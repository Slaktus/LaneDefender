using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LaneItem : LaneObject
{
    public override void Update()
    {
        bool move = true;

        if ( changeLane != null )
            move = !changeLane.MoveNext();

        float x = position.x - ( ( speed * Time.deltaTime ) * ( move ? 1 : 0 ) );
        bool destroy = end + ( body.transform.localScale.x * 0.5f ) > x;
        position = new Vector3( Mathf.Clamp( x , end + ( scale.x * 0.5f ) , start - ( scale.x * 0.5f ) ) , position.y , position.z );
        Debug.DrawLine( new Vector3( rect.xMin , 0 , rect.yMin ) , new Vector3( rect.xMax , 0 , rect.yMax ) , Color.yellow );

        if ( leap != null )
            leap.MoveNext();

        if ( overlap )
            overlapping.Interaction( this );

        if ( destroy )
            Destroy();
    }

    public void Split()
    {
        HeldItem upHeldItem = new HeldItem( new ConveyorItem( lane.stage.conveyor , Definitions.Item( Definitions.Items.Part ) , heldItem.conveyorItem.settings ) );
        LaneItem upItem = new LaneItem( upHeldItem , lane );
        upHeldItem.conveyorItem.Destroy();
        upHeldItem.Destroy();
        lane.Add( upItem );
        upItem.SetPosition( new Vector3( position.x , upItem.position.y , upItem.position.z ) );
        upItem.changeLane = upItem.ChangeLane( -1 );

        HeldItem downHeldItem = new HeldItem( new ConveyorItem( lane.stage.conveyor , Definitions.Item( Definitions.Items.Part ) , heldItem.conveyorItem.settings ) );
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
        body.transform.localPosition = Vector3.up;

        while ( laneEntity.valid && back > laneEntity.back )
            yield return null;

        body.transform.localPosition = Vector3.zero;
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

    public Definitions.Items type => heldItem != null ? heldItem.conveyorItem.type : Definitions.Items.Wreck;
    public int damage => heldItem != null ? heldItem.conveyorItem.damage : 1;
    public List<Definitions.Effects> effects { get; }
    public HeldItem heldItem { get; }

    public override float front => rect.xMin;
    public override float back => rect.xMax;

    protected override float start => lane.end.x;
    protected override float end => lane.start.x;
    protected override float speed => lane.speed;

    private IEnumerator leap { get; set; }

    public LaneItem( HeldItem heldItem , Lane lane ) : base( "Lane" + heldItem.conveyorItem.type.ToString() , lane )
    {
        effects = heldItem.conveyorItem.settings.effects;
        body.transform.localScale = new Vector3( heldItem.conveyorItem.width , 1 , heldItem.conveyorItem.height );

        meshRenderer.material.color = Color.white;
        label.SetText( heldItem.conveyorItem.text );

        position = lane.end + ( Vector3.up * 0.5f ) + ( Vector3.left * body.transform.localScale.x * 0.5f );
        this.heldItem = heldItem;
    }

    public LaneItem(List<Definitions.Effects> effects, Lane lane , string name , float width , float height , Vector3 position ) : base( "Lane" + name , lane )
    {
        this.effects = effects;
        body.transform.localScale = new Vector3( width , 1 , height );

        meshRenderer.material.color = Color.grey;
        label.SetColor( Color.white );
        label.SetText( name );

        this.position = position;
        heldItem = null;
    }
}