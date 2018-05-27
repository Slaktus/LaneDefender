using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Shop
{
    public void Update()
    {
        _buyPanel.Update();
        _upgradeHeroPanel.Update();
        _upgradeItemPanel.Update();
    }

    public void Show( Player player )
    {
        List<Definitions.Heroes> heroesToBuy = new List<Definitions.Heroes>( availableHeroes.Count - player.inventory.heroes.Count );
        List<Definitions.Heroes> heroesToUpgrade = new List<Definitions.Heroes>( player.inventory.heroes.Count );

        for ( int i = 0 ; availableHeroes.Count > i ; i++ )
            ( player.inventory.heroes.Contains( availableHeroes[ i ] ) ? heroesToUpgrade : heroesToBuy ).Add( availableHeroes[ i ] );

        List<Definitions.Items> itemsToBuy = new List<Definitions.Items>( availableItems.Count - player.inventory.items.Count );
        List<Definitions.Items> itemsToUpgrade = new List<Definitions.Items>( player.inventory.items.Count );

        for ( int i = 0 ; availableItems.Count > i ; i++ )
            ( player.inventory.items.Contains( availableItems[ i ] ) ? itemsToUpgrade : itemsToBuy ).Add( availableItems[ i ] );

        _buyPanel = new BuyPanel( this , player , heroesToBuy , itemsToBuy , 10 * ( heroesToBuy.Count + itemsToBuy.Count ) , 4 , 0.1f , 0.5f , 1 );
        _upgradeItemPanel = new UpgradeItemPanel( player , itemsToUpgrade , 10 , 3 * itemsToUpgrade.Count , 0.1f , 0.5f , itemsToUpgrade.Count > 0 ? itemsToUpgrade.Count : 1 );
        _upgradeHeroPanel = new UpgradeHeroPanel( player , heroesToUpgrade , 10 , 3 * heroesToUpgrade.Count , 0.1f , 0.5f , heroesToUpgrade.Count > 0 ? heroesToUpgrade.Count : 1 );
    }

    public void Hide()
    {
        _buyPanel.Destroy();
        _upgradeItemPanel.Destroy();
        _upgradeHeroPanel.Destroy();
    }

    public void Refresh( Player player )
    {
        Hide();
        Show( player );
    }

    public List<Definitions.Heroes> availableHeroes { get; }
    public List<Definitions.Items> availableItems { get; }

    private BuyPanel _buyPanel { get; set; }
    private UpgradeItemPanel _upgradeItemPanel { get; set; }
    private UpgradeHeroPanel _upgradeHeroPanel { get; set; }

    public Shop()
    {
        availableHeroes = new List<Definitions.Heroes>()
        {
            Definitions.Heroes.Default
        };

        availableItems = new List<Definitions.Items>()
        {
            Definitions.Items.Damage ,
            Definitions.Items.LaneDown ,
            Definitions.Items.LaneUp ,
            Definitions.Items.Split ,
            Definitions.Items.Leap
        };
    }
}

public class UpgradeHeroPanel : Panel
{
    public UpgradeHeroPanel( Player player , List<Definitions.Heroes> heroes , float width , float height , float spacing , float padding , int rows ) : base( "UpgradeHero" , width , height )
    {
        int count = heroes.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        contents = new List<Element>( count );

        for ( int i = 0 ; count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            int index = i;
            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents.Add( new UpgradeHeroElement( localPosition , size.x , size.y , container , 
                ( Button button ) => button.SetColor( Color.green ) ,
                ( Button button ) => 
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                        player.inventory.Settings( heroes[ index ] ).Upgrade();
                } ,
                ( Button button ) => button.SetColor( Color.white ) ) );

            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.75f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
    }
}

public class UpgradeItemPanel : Panel
{
    public UpgradeItemPanel( Player player , List<Definitions.Items> items , float width , float height , float spacing , float padding , int rows ) : base( "UpgradeItem" , width , height )
    {
        int count = items.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        contents = new List<Element>( count );

        for ( int i = 0 ; count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            int index = i;
            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents.Add( new UpgradeItemElement( localPosition , size.x , size.y , container , 
                ( Button button ) => button.SetColor( Color.green ) ,
                ( Button button ) => 
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                        player.inventory.Settings( items[ index ] ).Upgrade();
                } ,
                ( Button button ) => button.SetColor( Color.white ) ) );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.25f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
    }
}

public class BuyPanel : Panel
{
    public override void Update()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
            contents[ i ].Update();
    }

    public BuyPanel( Shop shop , Player player , List<Definitions.Heroes> heroes , List<Definitions.Items> items , float width , float height , float spacing , float padding , int rows ) : base ( "Buy" , width , height )
    {
        int count = heroes.Count + items.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        contents = new List<Element>( count );

        for ( int i = 0 ; count > i ; i++ )
        {
            int index = i;

            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }
            
            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents.Add( i >= heroes.Count 
                ? new BuyItemElement( localPosition , size.x , size.y , container , 
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) => 
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            player.inventory.AddItem( items[ index - heroes.Count ] );
                            shop.Refresh( player );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) as Panel 
                : new BuyHeroElement( localPosition , size.x , size.y , container ,
                    Enter: ( Button button ) => button.SetColor( Color.green ) ,
                    Stay: ( Button button ) =>
                    {
                        if ( Input.GetMouseButtonDown( 0 ) )
                        {
                            player.inventory.AddHero( heroes[ index ] );
                            shop.Refresh( player );
                        }
                    } ,
                    Exit: ( Button button ) => button.SetColor( Color.white ) ) as Panel );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f , 0 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.forward * ( ( height * 0.5f ) + 1 ) );
    }
}

public class UpgradeItemElement : ButtonPanel
{
    public UpgradeItemElement( Vector3 localPosition , float width , float height , GameObject parent , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null ) : base( "UpgradeItem" , "Upgrade\nItem" , width , height , Enter , Stay , Exit )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.red;
    }
}

public class UpgradeHeroElement : ButtonPanel
{
    public UpgradeHeroElement( Vector3 localPosition , float width , float height , GameObject parent , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null ) : base( "UpgradeHero" , "Upgrade\nHero" , width , height , Enter , Stay , Exit )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.yellow;
    }
}

public class BuyItemElement : ButtonPanel
{
    public BuyItemElement( Vector3 localPosition , float width , float height , GameObject parent , Action<Button> Enter , Action<Button> Stay , Action<Button> Exit ) : base( "BuyItem" , "Buy\nItem" , width , height , Enter , Stay , Exit )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.red;
    }
}

public class BuyHeroElement : ButtonPanel
{
    public BuyHeroElement( Vector3 localPosition , float width , float height , GameObject parent , Action<Button> Enter , Action<Button> Stay , Action<Button> Exit ) : base( "BuyHero" , "Buy\nHero" , width , height , Enter , Stay , Exit )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.yellow;
    }
}

public class ButtonPanel : Panel
{
    public override void Update()
    {
        button.Update();
        base.Update();
    }

    public override void Destroy()
    {
        button.Destroy();
        base.Destroy();
    }

    public Button button { get; private set; }

    public ButtonPanel ( string name , string label , float width , float height , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null ) : base ( name , width , height )
    {
        button = new Button( name , label , width - 1 , height - 1 , container , Enter , Stay , Exit );
    }
}

public class Panel : Element
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
        GameObject.Destroy( container );
    }

    protected List<Element> contents { get; set; }
    protected MeshRenderer quad { get; set; }

    public Panel( string name , float width , float height ) : base( name + typeof( Panel ).Name , width , height )
    {
        this.width = width;
        this.height = height;
        contents = new List<Element>();

        quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        quad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        quad.receiveShadows = false;
        quad.transform.SetParent( container.transform );
        quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localScale = new Vector3( width , height , 1 );
        quad.transform.name = name + "PanelBG";
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

public abstract class Element
{
    public virtual void Update() { }
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