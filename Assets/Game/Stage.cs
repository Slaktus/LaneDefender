using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Keeps and updates lanes and contains methods for operating on them
/// </summary>
public class Stage
{
    public bool Handle( WaveEvent waveEvent )
    {
        bool handled = false;

        switch ( waveEvent.type )
        {
            case WaveEvent.Type.SpawnEnemy:
                Lane lane = LaneBy( waveEvent.lane );
                handled = true;

                for ( int i = 0 ; lane.objects.Count > i && handled ; i++ )
                    if ( lane.objects[ i ] is LaneEntity && 5 > lane.objects[ i ].back - lane.start.x )
                        handled = false;

                if ( handled )
                    lane.Add( new Enemy( ( waveEvent as SpawnEnemyEvent ).enemyDefinition , new EnemySettings( Color.white , 3 , 8 ) , lane ) );

                return handled;

            default:
                return handled;
        }
    }

    public void AddHero( Lane lane , HeroDefinition heroDefinition ) => lane.Add( new Hero( heroDefinition , new HeroSettings( Color.white , 3 ) , lane ) );
    public void ShowLane( int index ) => LaneBy( index ).Show();
    public void HideLane( int index ) => LaneBy( index ).Hide();

    public void ShowLanes()
    {
        for ( int i = 0 ; lanes > i ; i++ )
            ShowLane( i );
    }

    public void HideLanes()
    {
        for ( int i = 0 ; lanes > i ; i++ )
            HideLane( i );
    }

    public void ClearLane( int index ) => LaneBy( index ).Clear();
    public void ClearLane<T>( int index ) => LaneBy( index ).Clear<T>();
    public void DestroyLane( int index ) => LaneBy( index ).Destroy();

    public void ClearLanes()
    {
        for ( int i = 0 ; lanes > i ; i++ )
            _lanes[ i ].Clear();
    }

    public void DestroyLanes()
    {
        ClearLanes();

        while ( lanes > 0 )
        {
            Lane lane = LaneBy( lanes - 1 );
            _lanes.Remove( lane );
            lane.Destroy();
        }
    }

    /// <summary>
    /// Updates all lanes
    /// Should strictly speaking be an event, but right now this is safe
    /// </summary>
    public void Update() => Updater();

    /// <summary>
    /// Set color of all lanes except optional
    /// </summary>
    /// <param name="color">Color to apply</param>
    /// <param name="except">Lane to except</param>
    public void SetLaneColor( Color color )
    {
        for ( int i = 0 ; _lanes.Count > i ; i++ )
            _lanes[ i ].color = color;
    }

    /// <summary>
    /// Get currently hovered lane
    /// </summary>
    /// <param name="position">World-space mouse position</param>
    /// <returns>Currently hovered lane, or null if none</returns>
    public Lane GetHoveredLane( Vector3 position )
    {
        for ( int i = 0 ; _lanes.Count > i ; i++ )
            if ( _lanes[ i ].Contains( position ) )
                return _lanes[ i ];

        return null;
    }

    public void AddCoins( int value ) => _player.inventory.AddCoins( value );
    public int IndexOf( Lane lane ) => _lanes.IndexOf( lane );
    public Lane LaneBy( int index ) => _lanes[ index ];

    /// <summary>
    /// Set the speed at which items move down the conveyor
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed( float speed ) => this.speed = speed;

    public int enemies
    {
        get
        {
            int count = 0;

            for ( int i = 0 ; lanes > i ; i++ )
                count += _lanes[ i ].Count<Enemy>();

            return count;
        }
    }

    public int items
    {
        get
        {
            int count = 0;

            for ( int i = 0 ; lanes > i ; i++ )
                count += _lanes[ i ].Count<LaneItem>();

            return count;
        }
    }

    public int lanes => _lanes.Count;
    public float laneSpacing { get; private set; }
    public Conveyor conveyor { get; private set; }
    public float speed { get; private set; }

    private Player _player { get; }
    private List<Lane> _lanes { get; }
    private event Action Updater;

    public Stage( float speed , float width , float height , float laneSpacing , int laneCount , Conveyor conveyor , Player player )
    {
        _player = player;
        this.speed = speed;
        this.conveyor = conveyor;
        this.laneSpacing = laneSpacing;
        float laneHeight = height / laneCount;
        _lanes = new List<Lane>( laneCount );

        for ( int i = 0 ; laneCount > i ; i++ )
        {
            Lane lane = new Lane(
                   stage: this ,
                   depth: ( i * ( laneHeight + laneSpacing ) ) + ( laneHeight * 0.5f ) ,
                   width: width ,
                   height: laneHeight ,
                   name: "Lane" + i );

            _lanes.Add( lane );
            Updater += lane.Update;
        }
    }
}
