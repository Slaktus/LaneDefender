using System.Collections.Generic;
using UnityEngine;

public class HealthBar
{
    public void Increase( int value = 1 ) => SetValue( this.value + value );

    public void Decrease( int value = 1 ) => SetValue( this.value - value );

    private void SetValue( int value )
    {
        bool different = Mathf.Clamp( value , 0 , initialValue ) != this.value;
        this.value = Mathf.Clamp( value , 0 , initialValue );

        if ( different )
            for ( int i = 0 ; _segments.Count > i ; i++ )
                _segments[ i ].material.color = this.value > i ? Color.red : Color.black;
    }

    public void SetParent( Transform parent , Vector3 localPosition )
    {
        _container.transform.SetParent( parent );
        _container.transform.localPosition = localPosition;
    }

    public int value { get; private set; }
    public int initialValue { get; }

    private List<MeshRenderer> _segments { get; }
    private GameObject _container { get; }
    private MeshRenderer _quad { get; }
    private float _width { get; }
    private float _height { get; }

    public HealthBar( float width , float height , float spacing , float padding , int rows , int value )
    {
        _container = new GameObject( "HealthBar" );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localScale = new Vector3( width , height , 1 );
        _quad.transform.name = "HealthBG";

        _width = width;
        _height = height;
        initialValue = this.value = value;

        int perRow = Mathf.CeilToInt( value / rows );
        int remainder = value - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        _segments = new List<MeshRenderer>( value );

        for ( int i = 0 ; value > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            MeshRenderer segment = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
            segment.transform.SetParent( _container.transform );
            segment.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
            segment.transform.localScale = new Vector3( size.x , size.y , 1 );
            segment.transform.localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            segment.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            segment.material.color = Color.red;
            segment.name = "Segment" + i;
            _segments.Add( segment );
            x++;
        }
    }
}
