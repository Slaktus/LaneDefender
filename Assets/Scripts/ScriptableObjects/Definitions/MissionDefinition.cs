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
    public override void Add( ScriptableObject toAdd ) => waveDefinitions.Add( toAdd as WaveDefinition );

    public void Add( ScriptableObject toAdd , float time )
    {
        int i = 0;

        while ( waveTimes.Count > i && time > waveTimes[ i ])
            i++;

        waveDefinitions.Insert(i, toAdd as WaveDefinition);
        waveTimes.Insert(i, time);
    }

    public override void Remove( ScriptableObject toRemove )
    {
        WaveDefinition waveDefinition = toRemove as WaveDefinition;
        waveTimes.RemoveAt( waveDefinitions.IndexOf( waveDefinition ) );
        waveDefinitions.Remove( waveDefinition );
    }

    public float duration;
    public StageDefinition stageDefinition;
    public List<float> waveTimes = new List<float>();
    public List<WaveDefinition> waveDefinitions = new List<WaveDefinition>();
}