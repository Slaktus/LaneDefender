using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveSet : ScriptableObject
{
    public void AddWaveDefinition()
    {
        WaveDefinition waveDefinition = ScriptableObject.CreateInstance<WaveDefinition>();
        waveDefinitions.Add( waveDefinition );

        AssetDatabase.AddObjectToAsset( waveDefinition , this );
        AssetDatabase.SaveAssets();
    }

    public List<WaveDefinition> waveDefinitions = new List<WaveDefinition>();
}
