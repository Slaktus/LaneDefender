using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveData : ScriptableObject
{
    public void AddWaveSet()
    {
        WaveSet waveSet = ScriptableObject.CreateInstance<WaveSet>();
        waveSets.Add( waveSet );

        AssetDatabase.AddObjectToAsset( waveSet , this );
        AssetDatabase.SaveAssets();
    }

    public List<WaveSet> waveSets = new List<WaveSet>();
}
