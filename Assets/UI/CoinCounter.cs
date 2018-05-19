using UnityEngine;

public class CoinCounter
{
    public void Show() => _container.SetActive( true );
    public void Hide() => _container.SetActive( false );
    public void Destroy() => GameObject.Destroy( _container );
    public void SetCounterValue( int value ) => textMesh.text = value.ToString();

    public TextMesh textMesh { get; }

    private GameObject _container { get; }
    private GameObject _quad { get; }

    public CoinCounter( int value = 0 )
    {
        _container = new GameObject( "CoinCounter" );

        _quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
        _quad.transform.SetParent( _container.transform );
        _quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        _quad.transform.localScale = new Vector3( 2 , 1 , 1 );

        textMesh = new GameObject( "Counter" ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( _container.transform );
        textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.15f;
        textMesh.color = Color.black;
        textMesh.fontSize = 35;

        textMesh.text = value.ToString();
        _container.transform.position += new Vector3( -1.75f , 0 , 2 );
    }
}