using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Level
{
    public void Update()
    {
        if ( _waves.Count > 0 && _time > _waves.Peek().time )
        {
            IEnumerator handler = WaveHandler( _waves.Dequeue() );
            _currentHandlers.Add( handler );
            Updater += handler.MoveNext;
        }

        Updater();
        _progress.Update();
        _time += Time.deltaTime;
    }

    public void Add( Wave wave ) => _waves.Enqueue( wave );

    private IEnumerator WaveHandler( Wave wave )
    {
        float time = 0;
        _currentWaves.Add( wave );

        while ( wave.events > 0 )
        {
            wave.Update( time += Time.deltaTime );
            yield return null;
        }

        IEnumerator handler = _currentHandlers[ _currentWaves.IndexOf( wave ) ];
        _currentWaves.Remove( wave );
        Updater -= handler.MoveNext;
    }

    public void ShowProgress() => _progress.Show();
    public void HideProgress() => _progress.Hide();
    public void DestroyProgress() => _progress.Destroy();

    public int waves => _waves.Count + _currentWaves.Count;
    public float duration { get; private set; }
    public float _time { get; private set; }
    public float progress => _progress.progress;

    private LevelProgress _progress { get; }
    private List<IEnumerator> _currentHandlers { get; }
    private List<Wave> _currentWaves { get; }
    private Queue<Wave> _waves { get; }
    private event Func<bool> Updater;

    public Level( float duration , bool showProgress = true )
    {
        Updater += () => false;
        this.duration = duration;
        _waves = new Queue<Wave>();
        _currentWaves = new List<Wave>();
        _currentHandlers = new List<IEnumerator>();
        _progress = new LevelProgress( ( Vector3.forward * 2 ) + ( Vector3.right * 31.175f ) , 5 , 1 , duration );

        if ( !showProgress )
            HideProgress();
    }
}