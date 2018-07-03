using System.Collections.Generic;

public class Mission
{
    public WaveDefinition Next( float time ) => waveDefinitions.Count > 0 && wavetimes[ 0 ] * duration > time ? waveDefinitions[ 0 ] : null;
    public void SetDuration( float duration ) => _missionDefinition.SetDuration( duration );
    public void AddWaveDefinition( WaveDefinition waveDefinition ) => waveDefinitions.Add( waveDefinition );
    public void SetIndex( int index ) => this.index = index;

    public List<WaveDefinition> waveDefinitions => _missionDefinition.waveDefinitions;
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
