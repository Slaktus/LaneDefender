public class WaveEventDefinition : DefinitionBase
{
    public void SetLane( int lane ) => this.lane = lane;

    public void Initialize( float delay , int lane , WaveEvent.Type type , float entryPoint = 0 )
    {
        this.entryPoint = entryPoint;
        this.delay = delay;
        this.lane = lane;
        this.type = ( int ) type;
    }

    public float entryPoint;
    public float delay;
    public int type;
    public int lane;
}