using System.Collections.Generic;
using UnityEngine;

public class TileMap
{
    public int XOf( Tile tile ) => tile.index % columns;
    public int YOf( Tile tile ) => Mathf.FloorToInt( tile.index / columns );
    public Vector3 PositionOf( Tile tile ) => PositionOf( _tiles.IndexOf( tile ) );
    public Vector3 PositionOf( int index ) => _tiles[ index ].position + _offset;
    public int IndexOf( Tile tile ) => _tiles.IndexOf( tile );
    public Tile TileAt( int index ) => _tiles[ index ];

    public float tileWidth => _tiles[ 0 ].width;
    public float tileHeight => _tiles[ 0 ].height;
    public int count => _tiles.Count;
    public float height { get; }
    public float width { get; }
    public int columns { get; }
    public int rows { get; }

    private List<Tile> _tiles { get; }
    private Vector3 _offset { get; }

    public TileMap( float width , float height , int columns , int rows , Vector3 offset )
    {
        _offset = offset;
        this.rows = rows;
        this.width = width;
        this.height = height;
        this.columns = columns;
        _tiles = new List<Tile>( columns * rows );

        for ( int i = 0 ; _tiles.Capacity > i ; i++ )
            _tiles.Add( new Tile( this ) );
    }
}