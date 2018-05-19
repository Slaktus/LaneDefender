﻿using System.Collections.Generic;

public class Wave
{
    public void Update( float time )
    {
        for ( int i = 0 ; _events.Count > i ; i++ )
            if ( time > _events[ i ].delay )
                _queue.Enqueue( _events[ i ] );

        while ( _queue.Count > 0 )
        {
            WaveEvent waveEvent = _queue.Dequeue();
            bool handled = _stage.Handle( waveEvent );

            if ( handled )
                _events.Remove( waveEvent );
        }
    }

    public void Add( WaveEvent waveEvent ) => _events.Add( waveEvent );

    public int events => _events.Count;
    public float time { get; private set; }

    private List<WaveEvent> _events { get; }
    private Queue<WaveEvent> _queue { get; }
    private Stage _stage { get; }

    public Wave( float time , Stage stage )
    {
        _stage = stage;
        this.time = time;
        _events = new List<WaveEvent>();
        _queue = new Queue<WaveEvent>();
    }
}

public class SpawnEnemyEvent : WaveEvent
{
    public EnemyDefinition enemyDefinition { get; }

    public SpawnEnemyEvent( EnemyDefinition enemyDefinition , float delay , int lane ) : base( delay , lane , Type.SpawnEnemy )
    {
        this.enemyDefinition = enemyDefinition;
    }
}

public abstract class WaveEvent
{
    public float delay { get; }
    public Type type { get; }
    public int lane { get; }

    public WaveEvent( float delay , int lane , Type type )
    {
        this.delay = delay;
        this.lane = lane;
        this.type = type;
    }

    public enum Type
    {
        SpawnEnemy
    }
}
