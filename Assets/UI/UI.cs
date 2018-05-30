using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Button : Element
{
    public override void Destroy()
    {
        GameObject.Destroy( container );
    }

    public override void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

        if ( Contains( mousePos ) )
        {
            if ( !_hovering )
            {
                Enter( this );
                _hovering = true;
            }
            else
                Stay( this );

        }
        else if ( _hovering )
        {
            Exit( this );
            _hovering = false;
        }
    }

    public override void SetLocalScale( Vector3 localScale ) => quad.transform.localScale = localScale;

    public void SetColor( Color color ) => quad.material.color = color;
    public void SetEnter( Action<Button> Enter ) => this.Enter = Enter == null ? ( Button button ) => { } : Enter;
    public void SetStay( Action<Button> Stay ) => this.Stay = Stay == null ? ( Button button ) => { } : Stay;
    public void SetExit( Action<Button> Exit ) => this.Exit = Exit == null ? ( Button button ) => { } : Exit;

    private Action<Button> Enter { get; set; }
    private Action<Button> Stay { get; set; }
    private Action<Button> Exit { get; set; }

    protected bool Contains( Vector3 position ) => rect.Contains( new Vector2( position.x , position.z ) );

    public Rect rect => new Rect( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , width , height );
    public Vector2 screenPosition => Camera.main.WorldToScreenPoint( new Vector3( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , Camera.main.transform.position.z ) );
    protected MeshRenderer quad { get; set; }
    protected TextMesh textMesh { get; set; }

    private bool _hovering { get; set; }

    public Button( string name , string label , float width , float height , GameObject parent , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null ) : base( name + typeof( Button ).Name , width , height )
    {
        SetEnter( Enter );
        SetStay( Stay );
        SetExit( Exit );

        quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        quad.transform.SetParent( container.transform );
        quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localScale = new Vector3( width , height , 1 );
        quad.transform.name = "ButtonBG";

        textMesh = new GameObject( "ButtonLabel" ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( container.transform );
        textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );

        textMesh.fontSize = 35;
        textMesh.color = Color.black;
        textMesh.characterSize = 0.15f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = label;

        container.transform.SetParent( parent.transform );
        container.transform.localPosition = Vector3.up;
    }
}

public class Layout : Panel
{
    public override void Update()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
            contents[ i ].Update();
    }

    public override void Destroy()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
            contents[ i ].Destroy();

        contents.Clear();
        base.Destroy();
    }

    public void Refresh()
    {
        int count = contents.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );

        for ( int i = 0 ; contents.Count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents[ i ].SetLocalScale( new Vector3( size.x , size.y , 1 ) );
            contents[ i ].SetLocalPosition( localPosition );
            x++;
        }
    }

    public void Add<T>( T element , bool refresh = false ) where T : Element => Add( element as Element , refresh );
    public void Remove<T>( T element , bool refresh = false ) where T : Element => Remove( element as Element , refresh );

    public void Add( Element element , bool refresh = false )
    {
        element.SetParent( container );
        contents.Add( element );

        if ( refresh )
            Refresh();
    }

    public void Remove( Element element , bool refresh = false )
    {
        contents.Remove( element );

        if ( refresh )
            Refresh();
    }

    public void Add<T>( List<T> elements , bool refresh = false ) where T : Element
    {
        for ( int i = 0 ; elements.Count > i ; i++ )
            Add( elements[ i ] );

        if ( refresh )
            Refresh();
    }

    public void Remove<T>( List<T> elements , bool refresh = false ) where T : Element
    {
        for ( int i = 0 ; elements.Count > i ; i++ )
            Remove( elements[ i ] );

        if ( refresh )
            Refresh();
    }

    public float padding { get; protected set; }
    public float spacing { get; protected set; }
    public int rows { get; protected set; }

    protected List<Element> contents { get; set; }

    public Layout( string name , float width , float height , float padding , float spacing , int rows ) : base( name , width , height )
    {
        contents = new List<Element>();
        this.padding = padding;
        this.spacing = spacing;
        this.rows = rows;
    }
}

public class Panel : Element
{
    public override void SetLocalScale( Vector3 localScale ) => quad.transform.localScale = localScale;
    public override void Destroy() => GameObject.Destroy( container );

    protected MeshRenderer quad { get; set; }

    public Panel( string name , float width , float height ) : base( name + typeof( Panel ).Name , width , height )
    {
        quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        quad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        quad.receiveShadows = false;

        quad.transform.SetParent( container.transform );
        quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localScale = new Vector3( width , height , 1 );
        quad.transform.name = name + "PanelBG";
    }
}

public abstract class Element
{
    public void SetLocalPosition( Vector3 localPosition ) => container.transform.localPosition = localPosition;
    public void SetParent( GameObject parent ) => container.transform.SetParent( parent.transform );

    public virtual void Update() { }

    public abstract void SetLocalScale( Vector3 localScale );
    public abstract void Destroy();

    protected GameObject container { get; set; }
    protected float width { get; set; }
    protected float height { get; set; }

    public Element( string name , float width , float height )
    {
        this.width = width;
        this.height = height;
        container = new GameObject( name );
    }
}