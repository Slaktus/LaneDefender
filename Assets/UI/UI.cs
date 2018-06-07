using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dropdown : Button
{
    public override void Update()
    {
        base.Update();
        bool contains = containsMouse;

        for ( int i = 0 ; layouts.Count > i && !contains ; i++ )
            contains = layouts[ i ].containsMouse;

        if ( !contains )
            Close?.Invoke( this );
    }

    public void AddLayout( Layout layout ) => layouts.Add( layout );
    public void RemoveLayout( Layout layout ) => layouts.Remove( layout );
    public bool HasLayout( Layout layout ) => layouts.Contains( layout );
    public void SetClose( Action<Button> Close ) => this.Close = Close;

    private Action<Button> Close { get; set; }

    private List<Layout> layouts { get; }

    public Dropdown( string name , string label , float width , float height , GameObject parent = null , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null , Action<Button> Close = null , bool hideQuad = false , int fontSize = 35 , float characterSize = 0.15f ) : base( name , label , width , height , parent , Enter , Stay , Exit , hideQuad , fontSize , characterSize )
    {
        SetClose( Close );
        layouts = new List<Layout>();
    }
}

public class Field : Button
{
    public override void Update()
    {
        if ( _handler != null )
            _handler.MoveNext();

        if ( Time.time > _doubleClickTime )
            _click = false;

        base.Update();
    }

    private IEnumerator Handler()
    {
        SetColor( Color.green );
        StartInput?.Invoke( this );
        ShowQuad();

        while ( Input.GetMouseButton( 0 ) )
            yield return null;

        string input = label.text;
        int count = input.Length;
        bool done = false;

        while ( !done )
        {
            string current = Input.inputString;

            if ( Input.GetMouseButton( 1 ) || Input.GetKeyDown( KeyCode.Return ) || Input.GetKeyDown( KeyCode.KeypadEnter ) )
            {
                done = true;
            }
            else if ( count > 0 && Input.GetKeyDown( KeyCode.Backspace ) )
            {
                string changed = input.Remove( ( count-- ) - 1 , 1 );
                input = changed.Trim();
            }
            else if ( !string.IsNullOrEmpty( current ) )
            {
                int parsedInt = -1;
                char[] chars = current.ToCharArray();

                bool isPunctuation = false;

                for ( int i = 0 ; chars.Length > i && !isPunctuation ; i++ )
                    if ( chars[ i ] == '.' )
                        isPunctuation = true;

                bool isNumber = int.TryParse( current , out parsedInt );
                bool valid =  mode == Mode.TextAndNumbers || ( mode == Mode.Text && !isNumber ) || ( mode == Mode.Numbers && ( isNumber || isPunctuation ) );

                if ( valid )
                {
                    input = input + current;
                    count += current.Length;
                }
            }

            label.SetText( input );
            yield return null;
        }

        SetColor( Color.white );
        EndInput?.Invoke( this );
        _handler = null;
        HideQuad();
    }

    public new Label label => base.label;

    private bool _doubleClick
    {
        get
        {
            return _click;
        }
        set
        {
            _doubleClickTime = Time.time + _doubleClickInterval;
            _click = value;
        }
    }

    private static float _doubleClickInterval = 0.2f;
    private Action<Field> StartInput { get; set; }
    private Action<Field> EndInput { get; set; }
    private float _doubleClickTime { get; set; }
    private IEnumerator _handler { get; set; }
    private Mode mode { get; set; } 
    private bool _click;

    public Field( string name , string label , float width , float height , int fontSize = 35 , GameObject parent = null , Mode mode = Mode.TextAndNumbers , Action<Field> StartInput = null , Action<Field> EndInput = null ) : base( name , label , width , height , parent , hideQuad: true , fontSize: fontSize )
    {
        this.StartInput = StartInput;
        this.EndInput = EndInput;
        this.mode = mode;

        SetStay( ( Button button ) =>
        {
            if ( Input.GetMouseButtonDown( 0 ) )
            {
                if ( _doubleClick )
                    _handler = Handler();
                else
                    _doubleClick = true;
            }
        } );
    }

    public enum Mode
    {
        Text,
        Numbers,
        TextAndNumbers,
    }
}

public class Button : Element
{
    public override void Destroy()
    {
        GameObject.Destroy( container );
    }

    public override void Update()
    {
        if ( containsMouse )
        {
            if ( !hovering )
            {
                Enter( this );
                hovering = true;
            }
            else
                Stay( this );

        }
        else if ( hovering )
        {
            Exit( this );
            hovering = false;
        }
    }

    public void ShowQuad() => quad.enabled = true;
    public void HideQuad() => quad.enabled = false;
    public void ShowLabel() => label.Show();
    public void HideLabel() => label.Hide();

    public void Show()
    {
        ShowQuad();
        ShowLabel();
    }

    public void Hide()
    {
        HideQuad();
        HideLabel();
    }

    public override void SetLocalScale( Vector3 localScale ) => quad.transform.localScale = localScale;

    public void SetColor( Color color ) => quad.material.color = color;
    public void SetEnter( Action<Button> Enter ) => this.Enter = Enter == null ? ( Button button ) => { } : Enter;
    public void SetStay( Action<Button> Stay ) => this.Stay = Stay == null ? ( Button button ) => { } : Stay;
    public void SetExit( Action<Button> Exit ) => this.Exit = Exit == null ? ( Button button ) => { } : Exit;

    public Vector2 screenPosition => Camera.main.WorldToScreenPoint( new Vector3( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , Camera.main.transform.position.z ) );
    public Vector3 localPosition => container.transform.localPosition;
    public Vector3 position => container.transform.position;

    public Color color => quad.material.color;
    protected MeshRenderer quad { get; set; }
    protected bool hovering { get; set; }
    protected Label label { get; set; }

    private Action<Button> Enter { get; set; }
    private Action<Button> Stay { get; set; }
    private Action<Button> Exit { get; set; }

    public Button( string name , string label , float width , float height , GameObject parent , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null , bool hideQuad = false , int fontSize = 35 , float characterSize = 0.15f ) : base( name + typeof( Button ).Name , width , height )
    {
        SetEnter( Enter );
        SetStay( Stay );
        SetExit( Exit );

        quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        quad.transform.SetParent( container.transform );
        quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localScale = new Vector3( width , height , 1 );
        quad.transform.name = "ButtonBG";

        if ( parent != null )
        {
            container.transform.SetParent( parent.transform );
            container.transform.localPosition = Vector3.up;
        }

        this.label = new Label( label , Color.black , width , height , container , fontSize: fontSize, characterSize: characterSize );

        if ( hideQuad )
            HideQuad();
    }
}

public class Label : Element
{
    public void Show() => textMesh.gameObject.SetActive( true );
    public void Hide() => textMesh.gameObject.SetActive( false );
    public void SetText( string text ) => textMesh.text = text;
    public void SetColor( Color color ) => textMesh.color = color;
    public void SetLocalRotation( Quaternion localRotation ) => textMesh.transform.localRotation = localRotation;
    public override void SetLocalScale( Vector3 localScale ) => textMesh.transform.localScale = localScale;
    public override void Destroy() => GameObject.Destroy( container );

    public string text => textMesh.text;

    protected TextMesh textMesh { get; }

    public Label( string text , Color color , float width , float height , GameObject parent = null , string name = "Label" , int fontSize = 35 , float characterSize = 0.15f , TextAnchor anchor = TextAnchor.MiddleCenter , TextAlignment alignment = TextAlignment.Center ) : base( name , width , height )
    {
        textMesh = new GameObject( name ).AddComponent<TextMesh>();
        textMesh.transform.SetParent( container.transform );
        textMesh.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        textMesh.characterSize = characterSize;
        textMesh.alignment = alignment;
        textMesh.fontSize = fontSize;
        textMesh.anchor = anchor;
        textMesh.color = color;
        textMesh.text = text;

        if ( parent != null )
            SetParent( parent );
    }
}

public class Layout : Panel
{
    public override bool containsMouse => Contains( mousePos );

    public override bool Contains( Vector3 position ) => Contains( new Vector2( position.x , position.z ) );

    public override bool Contains( Vector2 position )
    {
        bool contains = false;

        for ( int i = 0 ; elements.Count > i && !contains ; i++ )
            contains = elements[ i ].Contains( position );

        return contains && base.Contains( position );
    }

    public override void Update()
    {
        for ( int i = 0 ; elements.Count > i ; i++ )
            elements[ i ].Update();
    }

    public override void Destroy()
    {
        for ( int i = 0 ; elements.Count > i ; i++ )
            elements[ i ].Destroy();

        elements.Clear();
        base.Destroy();
    }

    public void Refresh()
    {
        int count = elements.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );

        for ( int i = 0 ; elements.Count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );

            if ( !( elements[ i ] is Label ) )
                elements[ i ].SetLocalScale( new Vector3( size.x , size.y , 1 ) );

            elements[ i ].SetLocalPosition( localPosition );
            x++;
        }
    }

    public void Add<T>( T element , bool refresh = false ) where T : Element => Add( element as Element , refresh );
    public void Remove<T>( T element , bool refresh = false ) where T : Element => Remove( element as Element , refresh );

    public void Add( Element element , bool refresh = false )
    {
        element.SetParent( container );
        elements.Add( element );

        if ( refresh )
            Refresh();
    }

    public void Remove( Element element , bool refresh = false )
    {
        elements.Remove( element );

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

    public Vector3 position => container.transform.position;
    public float padding { get; protected set; }
    public float spacing { get; protected set; }
    public int rows { get; protected set; }

    protected List<Element> elements { get; set; }

    public Layout( string name , float width , float height , float padding , float spacing , int rows , GameObject parent = null ) : base( name , width , height )
    {
        elements = new List<Element>();
        this.padding = padding;
        this.spacing = spacing;
        this.rows = rows;

        if ( parent != null )
            SetParent( parent );
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
    public void SetViewportPosition( Vector2 viewportPosition ) => SetLocalPosition( Camera.main.ViewportToWorldPoint( new Vector3( viewportPosition.x , viewportPosition.y , Camera.main.transform.position.y ) ) + ( Vector3.right * width * 0.5f ) + ( Vector3.back * height * 0.5f ) );
    public void SetLocalPosition( Vector3 localPosition ) => container.transform.localPosition = localPosition;
    public void SetParent( GameObject parent ) => container.transform.SetParent( parent.transform );
    public void SetPosition( Vector3 position ) => container.transform.position = position;

    public virtual void Update() { }

    public abstract void SetLocalScale( Vector3 localScale );
    public abstract void Destroy();

    public Rect rect => new Rect( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , width , height );
    public virtual bool Contains( Vector3 position ) => Contains( new Vector2( position.x , position.z ) );
    public virtual bool Contains( Vector2 position ) => rect.Contains( position );
    public virtual bool containsMouse => Contains( mousePos );

    protected Vector3 mousePos => Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );
    protected GameObject container { get; set; }
    public float width { get; protected set; }
    public float height { get;protected  set; }

    public Element( string name , float width , float height )
    {
        this.width = width;
        this.height = height;
        container = new GameObject( name );
    }
}