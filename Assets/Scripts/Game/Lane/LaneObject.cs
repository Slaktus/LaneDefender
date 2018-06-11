using System.Collections;
using UnityEngine;

public abstract class LaneObject
{
    public abstract IEnumerator Update();

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
    protected Label label { get; }
    protected GameObject container { get; }
    protected MeshRenderer meshRenderer { get; }
    protected IEnumerator changeLane { get; set; }
    protected IEnumerator pushAhead { get; set; }
    protected IEnumerator pushBack { get; set; }
    protected IEnumerator melee { get; set; }
    protected IEnumerator enter { get; set; }

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

        label = new Label( string.Empty , Color.black , 1 , 1 , container );
        label.SetLocalRotation( Quaternion.Euler( 90 , 0 , 0 ) );
    }
}
