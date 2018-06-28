using UnityEngine;

public class Tile
{
    public Vector2 planarPosition => new Vector2( x * width , -y * height );
    public Vector3 position => new Vector3( x * width , 0 , -y * height );
    public float width => map.width / map.columns;
    public float height => map.height / map.rows;
    public int index => map.IndexOf( this );
    public int x => map.XOf( this );
    public int y => map.YOf( this );

    private TileMap map { get; }

    public Tile( TileMap map )
    {
        this.map = map;
    }
}