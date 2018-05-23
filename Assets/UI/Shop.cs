using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Hide() { }

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
        {
            BuyItemElement element = contents[ i ] as BuyItemElement;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

            if ( element.button.Contains( mousePos ) )
                Debug.Log( "UpgradeHero" );
        }
    }

    public UpgradeHeroPanel( List<HeroDefinition> heroes , float width , float height , float spacing , float padding , int rows ) : base( width , height )
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
        {
            UpgradeItemElement element = contents[ i ] as UpgradeItemElement;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

            if ( element.button.Contains( mousePos ) )
                Debug.Log( "UpgradeItem" );
        }
    }

    public UpgradeItemPanel( List<ItemDefinition> items , float width , float height , float spacing , float padding , int rows ) : base( width , height )
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
            {
                BuyItemElement element = contents[ i ] as BuyItemElement;
                Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

                if ( element.button.Contains( mousePos ) )
                    Debug.Log( "BuyItemElement" );
            }
            else
            {
                BuyHeroElement element = contents[ i ] as BuyHeroElement;
                Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );

                if ( element.button.Contains( mousePos ) )
                    Debug.Log( "BuyHeroElement" );
            }
        }
    }

    public BuyPanel( List<HeroDefinition> heroes , List<ItemDefinition> items , float width , float height , float spacing , float padding , int rows ) : base ( width , height )
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

public class UpgradeItemElement : Panel
{
    public Button button { get; private set; }

    public UpgradeItemElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        button = new Button( "Upgrade\nItem" , width - 1 , height - 1 , container );
        quad.material.color = Color.red;
    }
}

public class UpgradeHeroElement : Panel
{
    public Button button { get; private set; }

    public UpgradeHeroElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        button = new Button( "Upgrade\nItem" , width - 1 , height - 1 , container );
        quad.material.color = Color.yellow;
    }
}

public class BuyItemElement : Panel
{
    public Button button { get; private set; }

    public BuyItemElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        button = new Button( "Buy Item" , width - 1 , height - 1 , container );
        quad.material.color = Color.red;
    }
}

public class BuyHeroElement : Panel
{
    public Button button { get; private set; }

    public BuyHeroElement( Vector3 localPosition , float width , float height , GameObject parent ) : base( width , height )
    {
        container.transform.SetParent( parent.transform );
        container.transform.localPosition = localPosition;
        button = new Button( "Buy Hero" , width - 1 , height - 1 , container );
        quad.material.color = Color.yellow;
    }
}

public class Panel : Element
{
    protected List<Element> contents { get; set; }
    protected MeshRenderer quad { get; set; }

    public Panel( float width , float height ) : base( typeof( Button ).Name , width , height )
    {
        this.width = width;
        this.height = height;
        container = new GameObject( "Panel" );

        quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        quad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        quad.receiveShadows = false;
        quad.transform.SetParent( container.transform );
        quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localScale = new Vector3( width , height , 1 );
        quad.transform.name = "PanelBG";
    }
}

public class Button : Element
{
    public bool Contains( Vector3 position ) => rect.Contains( new Vector2( position.x , position.z ) );

    public Rect rect => new Rect( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , width , height );
    public Vector2 screenPosition => Camera.main.WorldToScreenPoint( new Vector3( container.transform.position.x - ( width * 0.5f ) , container.transform.position.z - ( height * 0.5f ) , Camera.main.transform.position.z ) );
    protected MeshRenderer quad { get; set; }
    protected TextMesh textMesh { get; set; }

    public Button( string label , float width , float height , GameObject parent ) : base( typeof( Button ).Name , width , height )
    {
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