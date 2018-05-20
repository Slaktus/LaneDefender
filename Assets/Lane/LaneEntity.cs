using System.Collections;
using UnityEngine;

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

            if ( melee != null )
                move = !melee.MoveNext();

            float x = position.x + ( ( speed * Time.deltaTime ) * ( move ? 1 : 0 ) );
            bool destroy = x > lane.end.x - ( cube.transform.localScale.x * 0.5f ) || 0 >= health;
            position = new Vector3( Mathf.Clamp( x , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

            if ( destroy )
                Destroy();

            yield return null;
        }
    }

    public override void Destroy()
    {
        if ( health == 0 )
        {
            lane.stage.AddCoins( defeatValue );
            lane.Add( new LaneItem( lane , "Wreck" , scale.x , scale.z , position ) );
        }

        base.Destroy();
    }

    private void Damage( int damage = 1 )
    {
        _healthBar.Decrease( damage );

        if ( health > 0 )
            lane.stage.AddCoins( damageValue );
    }

    public void Interaction<T>( T laneObject ) where T : LaneObject
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

            if ( front is Enemy )
            {
                if ( ( back.scale.z * 0.5f > Mathf.Abs( front.position.z - back.position.z ) ) && ( front.Contains( back.frontPoint ) || front.Contains( back.frontPoint + ( Vector3.forward * back.scale.z * 0.5f ) ) || front.Contains( back.frontPoint + ( Vector3.back * back.scale.z * 0.5f ) ) ) )
                {
                    if ( front.melee == null )
                    {
                        back.position = new Vector3( front.back - ( back.scale.x * 0.6f ) , back.position.y , back.position.z );
                        back.pushBack = back.PushBack();
                        back.Damage();
                    }
                    else
                        back.position = new Vector3( front.back - ( back.scale.x * 0.5f ) , back.position.y , back.position.z );
                }
                else
                {
                    bool up = position.z > laneEntity.position.z;
                    position = new Vector3( position.x , position.y , up ? laneEntity.top + ( scale.z * 0.6f ) : laneEntity.bottom - ( scale.z * 0.6f ) );
                    changeLane = ChangeLane( up ? -1 : 1 );
                    Damage();
                }
            }
            else if ( front is Hero )
            {
                if ( back.melee == null && ( back.scale.z * 0.5f > Mathf.Abs( front.position.z - back.position.z ) ) && ( front.Contains( back.frontPoint ) || front.Contains( back.frontPoint + ( Vector3.forward * back.scale.z * 0.5f ) ) || front.Contains( back.frontPoint + ( Vector3.back * back.scale.z * 0.5f ) ) ) )
                {
                    back.position = new Vector3( front.back - ( front.scale.x * 0.6f ) , back.position.y , back.position.z );
                    back.melee = back.Melee( front );
                    //back.pushBack = back.PushBack();
                    //back.Damage();
                }
                else if ( back.melee == null )
                {
                    bool up = position.z > laneEntity.position.z;
                    position = new Vector3( position.x , position.y , up ? laneEntity.top + ( scale.z * 0.6f ) : laneEntity.bottom - ( scale.z * 0.6f ) );
                    changeLane = ChangeLane( up ? -1 : 1 );
                    Damage();
                }
            }
        }
    }

    private IEnumerator Melee( LaneEntity enemy )
    {
        float current = position.x;
        float target = current - 3;
        float targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( current , target , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

        targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( target , current + 1 , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

        if ( overlapping == enemy )
        {
            enemy.Damage();
            enemy.pushAhead = enemy.PushForward();
        }

        targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( current + 1 , current , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

        melee = null;
    }

    private IEnumerator PushForward()
    {
        float current = position.x;
        float target = current + 1;
        float targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( current , target , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

        targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( target , current , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

        pushAhead = null;
    }

    private IEnumerator PushBack()
    {
        float current = position.x;
        float target = current - 3;
        float targetTime = Time.time + 1;

        while ( targetTime > Time.time )
            yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( current , target , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );

        if ( this is Hero )
        {
            targetTime = Time.time + 1;

            while ( targetTime > Time.time )
                yield return position = new Vector3( Mathf.Clamp( Mathf.Lerp( target , current , 1f - ( targetTime - Time.time ) ) , start + ( scale.x * 0.5f ) , end - ( scale.x * 0.5f ) ) , position.y , position.z );
        }

        pushBack = null;
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
    public int health => _healthBar.value;

    public override float front => rect.xMax;
    public override float back => rect.xMin;

    protected override float start => lane.start.x;
    protected override float end => lane.end.x;

    protected override float speed => base.speed - lane.speed;

    private int _value { get; }
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