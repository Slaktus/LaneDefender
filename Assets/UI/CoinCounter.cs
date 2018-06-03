using UnityEngine;

public class CoinCounter
{
    public void Show() => _container.SetActive( true );
    public void Hide() => _container.SetActive( false );
    public void Destroy() => GameObject.Destroy( _container );
    public void SetCounterValue( int value ) => label.SetText( value.ToString() );

    public Label label { get; }

    private GameObject _container { get; }
    private GameObject _quad { get; }

    public CoinCounter( int value = 0 )
    {
        _container = new GameObject( "CoinCounter" );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localScale = new Vector3( 2 , 1 , 1 );

        label = new Label( value.ToString() , Color.black , 1 , 1 , _container );
        label.SetLocalRotation( Quaternion.Euler( 90 , 0 , 0 ) );

        _container.transform.position += new Vector3( -1.75f , 0 , 2 );
    }
}