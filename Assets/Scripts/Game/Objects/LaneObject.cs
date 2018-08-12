using System.Collections;
using UnityEngine;

public abstract class LaneObject : BaseObject
{
    public IEnumerator Enter( float entryPoint )
    {
        bool interrupt = false;

        while ( !interrupt && entryPoint - ( scale.x * 0.5f ) > position.x - lane.start.x )
        {
            float x = position.x + ( speed * Time.deltaTime * 3 );
            position = new Vector3( Mathf.Clamp( x , start + ( scale.x * 0.5f ) , start + entryPoint - ( scale.x * 0.5f ) ) , position.y , position.z );
            yield return interrupt = overlap;
        }

        enter = null;
    }

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

        changeLane = null;
    }

    public override void Destroy()
    {
        lane.Remove( this );
        GameObject.Destroy( container );
    }

    public bool overlap => overlapping != null && Mathf.Approximately( body.transform.localPosition.y , 0 );

    public abstract LaneEntity overlapping { get; }

    protected Lane lane { get; set; }

    protected IEnumerator changeLane { get; set; }
    protected IEnumerator pushAhead { get; set; }
    protected IEnumerator pushBack { get; set; }
    protected IEnumerator melee { get; set; }
    protected IEnumerator enter { get; set; }

    protected virtual float speed { get; }

    protected abstract float start { get; }
    protected abstract float end { get; }

    public LaneObject(string name, Lane lane, float speed = 0) : base(name, GameObject.CreatePrimitive(PrimitiveType.Cube))
    {
        this.lane = lane;
        this.speed = speed;
    }
}
