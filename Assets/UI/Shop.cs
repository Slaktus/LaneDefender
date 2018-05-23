using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Shop
{
    public void Update()
    {
        //_buyPanel.Update();
        //_upgradeHeroPanel.Update();
        //_upgradeItemPanel.Update();
    }

    public void Show( Player player )
    {
        List<HeroDefinition> heroesToBuy = new List<HeroDefinition>( availableHeroes.Count - player.inventory.heroes.Count );
        List<HeroDefinition> heroesToUpgrade = new List<HeroDefinition>( player.inventory.heroes.Count );

        for ( int i = 0 ; availableHeroes.Count > i ; i++ )
            ( player.inventory.heroes.Contains( availableHeroes[ i ].type ) ? heroesToUpgrade : heroesToBuy ).Add( availableHeroes[ i ] );

        List<ItemDefinition> itemsToBuy = new List<ItemDefinition>( availableItems.Count - player.inventory.items.Count );
        List<ItemDefinition> itemsToUpgrade = new List<ItemDefinition>( player.inventory.items.Count );

        for ( int i = 0 ; availableItems.Count > i ; i++ )
            ( player.inventory.items.Contains( availableItems[ i ].type ) ? itemsToUpgrade : itemsToBuy ).Add( availableItems[ i ] );

        _buyPanel = new BuyPanel( heroesToBuy , itemsToBuy , 20 , 5 , 0.1f , 0.5f , 1 );
        _upgradeItemPanel = new UpgradeItemPanel( itemsToUpgrade , 10 , 10 , 0.1f , 0.5f , itemsToUpgrade.Count > 0 ? itemsToUpgrade.Count : 1 );
        _upgradeHeroPanel = new UpgradeHeroPanel( heroesToUpgrade , 10 , 10 , 0.1f , 0.5f , itemsToUpgrade.Count > 0 ? itemsToUpgrade.Count : 1 );
    }

    public void Hide()
    {
        _buyPanel.Destroy();
        _upgradeItemPanel.Destroy();
        _upgradeHeroPanel.Destroy();
    }

    public List<HeroDefinition> availableHeroes { get; }
    public List<ItemDefinition> availableItems { get; }

    private BuyPanel _buyPanel { get; set; }
    private UpgradeItemPanel _upgradeItemPanel { get; set; }
    private UpgradeHeroPanel _upgradeHeroPanel { get; set; }

    public Shop()
    {
        availableHeroes = new List<HeroDefinition>()
        {
            Definitions.Hero( Definitions.Heroes.Default )
        };

        availableItems = new List<ItemDefinition>()
        {
            Definitions.Item( Definitions.Items.Damage ) ,
            Definitions.Item( Definitions.Items.LaneDown ) ,
            Definitions.Item( Definitions.Items.LaneUp ) ,
            Definitions.Item( Definitions.Items.Split ) ,
            Definitions.Item( Definitions.Items.Leap )
        };
    }
}

public class UpgradeHeroPanel : Panel
{
    public void Update()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
            ( contents[ i ] as BuyItemElement ).button.Update();
    }

    public UpgradeHeroPanel( List<HeroDefinition> heroes , float width , float height , float spacing , float padding , int rows ) : base( "UpgradeHero" , width , height )
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

            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents.Add( new UpgradeHeroElement( localPosition , size.x , size.y , container ) );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.75f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
    }
}

public class UpgradeItemPanel : Panel
{
    public void Update()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
            ( contents[ i ] as UpgradeItemElement ).button.Update();
    }

    public UpgradeItemPanel( List<ItemDefinition> items , float width , float height , float spacing , float padding , int rows ) : base( "UpgradeItem" , width , height )
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

            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents.Add( new UpgradeItemElement( localPosition , size.x , size.y , container ) );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.25f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
    }
}

public class BuyPanel : Panel
{
    public void Update()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
        {
            if ( contents[ i ] is BuyItemElement )
                ( contents[ i ] as BuyItemElement ).button.Update();
            else
                ( contents[ i ] as BuyHeroElement ).button.Update();
        }
    }

    public BuyPanel( List<HeroDefinition> heroes , List<ItemDefinition> items , float width , float height , float spacing , float padding , int rows ) : base ( "Buy" , width , height )
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
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            Vector3 localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            contents.Add( i >= heroes.Count ? new BuyItemElement( localPosition , size.x , size.y , container ) as Panel : new BuyHeroElement( localPosition , size.x , size.y , container ) as Panel );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f , 0 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.forward * ( ( height * 0.5f ) + 1 ) );
    }
}

public class UpgradeItemElement : ButtonPanel
{
    public UpgradeItemElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.red;
    }
}

public class UpgradeHeroElement : ButtonPanel
{
    public UpgradeHeroElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.yellow;
    }
}

public class BuyItemElement : ButtonPanel
{
    public BuyItemElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.red;
    }
}

public class BuyHeroElement : ButtonPanel
{
    public BuyHeroElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        quad.material.color = Color.yellow;
    }
}

public class ButtonPanel : Panel
{
    public override void Destroy()
    {
        button.Destroy();
        base.Destroy();
    }

    public Button button { get; private set; }

    public ButtonPanel ( float width , float height ) : base ( "BuyHero" , width , height )
    {
        button = new Button( "BuyHero" , width - 1 , height - 1 , container );
    }
}

public class Panel : Element
{
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

    public void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

        if ( Contains( mousePos ) )
        {
            if ( !_hovering )
            {
                Enter();
                _hovering = true;
            }
            else
                Stay();

        }
        else if ( _hovering )
        {
            Exit();
            _hovering = false;
        }
    }

    public void SetEnter( Action Enter ) => this.Enter = Enter == null ? () => { } : Enter;
    public void SetStay( Action Stay ) => this.Stay = Stay == null ? () => { } : Stay;
    public void SetExit( Action Exit ) => this.Exit = Exit == null ? () => { } : Exit;

    private Action Enter { get; set; }
    private Action Stay { get; set; }
    private Action Exit { get; set; }

    protected bool Contains( Vector3 position ) => rect.Contains( new Vector2( position.x , position.z ) );

    public Rect rect => new Rect( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , width , height );
    public Vector2 screenPosition => Camera.main.WorldToScreenPoint( new Vector3( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , Camera.main.transform.position.z ) );
    protected MeshRenderer quad { get; set; }
    protected TextMesh textMesh { get; set; }

    private bool _hovering { get; set; }

    public Button( string label , float width , float height , GameObject parent , Action Enter = null , Action Stay = null , Action Exit = null ) : base( label + typeof( Button ).Name , width , height )
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