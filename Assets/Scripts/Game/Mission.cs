using System.Collections.Generic;

public class Mission
{
    public WaveSet Next( float time ) => waveSets.Count > 0 && wavetimes[ 0 ] * duration > time ? waveSets[ 0 ] : null;
    public void SetDuration( float duration ) => _missionDefinition.SetDuration( duration );
    public void AddWaveSet( WaveSet waveSet ) => waveSets.Add( waveSet );
    public void SetIndex( int index ) => this.index = index;

    public List<WaveSet> waveSets => _missionDefinition.waveSets;
    public List<float> wavetimes => _missionDefinition.waveTimes;
    public float duration => _missionDefinition.duration;
    public int index { get; private set; }

    private MissionDefinition _missionDefinition { get; }

    public Mission( int index , MissionDefinition missionDefinition )
    {
        this.index = index;
        _missionDefinition = missionDefinition;
    }
}
