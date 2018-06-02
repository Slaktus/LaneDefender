using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveDefinition : ScriptableObject
{
    public void AddWaveEvent( WaveEventDefinition waveEvent )
    {
        waveEvents.Add( waveEvent );
        AssetDatabase.AddObjectToAsset( waveEvent , this );
        AssetDatabase.SaveAssets();
    }

    public List<WaveEventDefinition> waveEvents = new List<WaveEventDefinition>();
}