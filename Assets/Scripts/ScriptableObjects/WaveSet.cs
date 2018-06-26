using System.Collections.Generic;
using UnityEngine;

public class WaveSet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => waveDefinitions.Add( toAdd as WaveDefinition);
    public override void Remove( ScriptableObject toRemove ) => waveDefinitions.Remove( toRemove as WaveDefinition);
    public void SetTime( float time ) => this.time = time;

    public float time;
    public List<WaveDefinition> waveDefinitions = new List<WaveDefinition>();
}
