using System.Collections.Generic;
using UnityEngine;

public class WaveData : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => waveSets.Add( toAdd as WaveSet );
    public override void Remove( ScriptableObject toRemove ) => waveSets.Add( toRemove as WaveSet );
    
    public List<WaveSet> waveSets = new List<WaveSet>();
}
