using UnityEngine;

public class WaveEventDefinition : ScriptableObject
{
    public void SetLane( int lane ) => this.lane = lane;

    public void Initialize( float delay , int lane , WaveEvent.Type type , int subType , float entryPoint = 0 )
    {
        this.entryPoint = entryPoint;
        this.delay = delay;
        this.lane = lane;
        this.type = ( int ) type;
        this.subType = subType;
    }

    public float entryPoint;
    public float delay;
    public int subType;
    public int type;
    public int lane;
}