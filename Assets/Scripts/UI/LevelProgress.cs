using UnityEngine;

public class LevelProgress
{
    public void Update()
    {
        _time += Time.deltaTime;
        _indicator.transform.localPosition = new Vector3( Mathf.Lerp( _start , _end , progress ) , _indicator.transform.localPosition.y , _indicator.transform.localPosition.z );
    }

    public void Show() => _container.SetActive( true );
    public void Hide() => _container.SetActive( false );
    public void Destroy() => GameObject.Destroy( _container );

    public float progress => Mathf.Clamp( _time , 0 , _duration ) / _duration;

    private float _start => ( _height * 0.5f ) - ( _width * 0.5f );
    private float _end => ( _width * 0.5f ) - ( _height * 0.5f );

    private float _time { get; set; }
    private float _duration { get; }
    private float _width { get; }
    private float _height { get; }
    private GameObject _bar { get; set; }
    private GameObject _indicator { get; set; }
    private GameObject _container { get; set; }

    public LevelProgress( Vector3 position , float width , float height , float duration )
    {
        _duration = duration;
        _height = height;
        _width = width;

        _container = new GameObject( "LevelProgress" );
        _container.transform.position = position;

        _bar = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _bar.transform.SetParent( _container.transform );
        _bar.transform.localPosition = Vector3.zero;
        _bar.transform.localScale = new Vector3( width , height , 1 );
        _bar.GetComponent<MeshRenderer>().material.color = Color.black;


        _indicator = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _indicator.transform.SetParent( _container.transform );
        _indicator.transform.localPosition = ( Vector3.left * width * 0.5f ) + ( Vector3.right * height * 0.5f ) + ( Vector3.back * 0.1f );
        _indicator.transform.localScale = new Vector3( height , height , 1 );

        _container.transform.transform.rotation = Quaternion.Euler( 90 , 0 , 0 );
    }
}