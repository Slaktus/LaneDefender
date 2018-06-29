using System.Collections.Generic;
using UnityEngine;

public class MissionDefinition : DefinitionBase
{
    public static MissionDefinition Default() => CreateInstance<MissionDefinition>().Initialize( "MissionDefinition" , 120 , StageDefinition.Default() );

    public MissionDefinition Initialize( string name , float duration , StageDefinition stageDefinition )
    {
        this.name = name;
        this.duration = duration;
        this.stageDefinition = stageDefinition;
        return this;
    }

    public void SetDuration( float duration ) => this.duration = duration;
    public override void Add( ScriptableObject toAdd ) => waveSets.Add( toAdd as WaveSet );
    public override void Remove( ScriptableObject toRemove ) => waveSets.Remove( toRemove as WaveSet );

    public float duration;
    public StageDefinition stageDefinition;
    public List<WaveSet> waveSets = new List<WaveSet>();
}