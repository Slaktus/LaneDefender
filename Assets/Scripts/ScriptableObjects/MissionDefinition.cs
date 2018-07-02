using System.Collections.Generic;
using UnityEngine;

public class MissionDefinition : DefinitionBase
{
    public static MissionDefinition Default() => CreateInstance<MissionDefinition>().Initialize( "MissionDefinition" , 120 , null );

    public MissionDefinition Initialize( string name , float duration , StageDefinition stageDefinition )
    {
        this.name = name;
        this.duration = duration;
        this.stageDefinition = stageDefinition;
        return this;
    }

    public void SetDuration( float duration ) => this.duration = duration;
    public override void Add( ScriptableObject toAdd ) => waveSets.Add( toAdd as WaveSet );

    public void Add( ScriptableObject toAdd , float time )
    {
        Add( toAdd );
        waveTimes.Add( time );
    }

    public override void Remove( ScriptableObject toRemove )
    {
        WaveSet waveSet = toRemove as WaveSet;
        waveTimes.RemoveAt( waveSets.IndexOf( waveSet ) );
        waveSets.Remove( waveSet );
    }

    public float duration;
    public StageDefinition stageDefinition;
    public List<float> waveTimes = new List<float>();
    public List<WaveSet> waveSets = new List<WaveSet>();
}