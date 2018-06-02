﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEventDefinition : ScriptableObject
{
    public void Initialize( float delay , int lane , WaveEvent.Type type , float entryPoint = 0 )
    {
        this.entryPoint = entryPoint;
        this.delay = delay;
        this.lane = lane;
        this.type = ( int ) type;
    }

    public int type { get; private set; }
    public float entryPoint { get; private set; }
    public float delay { get; private set; }
    public int lane { get; private set; }
}