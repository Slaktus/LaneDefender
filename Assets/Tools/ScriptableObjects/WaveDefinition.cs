using System.Collections.Generic;
using UnityEngine;

public class WaveDefinition : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => waveEvents.Add( toAdd as WaveEventDefinition );
    public override void Remove( ScriptableObject toRemove ) => waveEvents.Remove( toRemove as WaveEventDefinition );

    public List<WaveEventDefinition> waveEvents = new List<WaveEventDefinition>();
}