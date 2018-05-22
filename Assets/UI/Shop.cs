using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop
{
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

        BuyPanel buyPanel = new BuyPanel( heroesToBuy , itemsToBuy , 20 , 5 , 0.1f , 0.5f , 1 );
        UpgradeItemPanel upgradeItemPanel = new UpgradeItemPanel( itemsToUpgrade , 10 , 10 , 0.1f , 0.5f , 2 );
        UpgradeHeroPanel upgradeHeroPanel = new UpgradeHeroPanel( heroesToUpgrade , 10 , 10 , 0.1f , 0.5f , 2 );
    }

    public void Hide() { }

    public List<HeroDefinition> availableHeroes { get; }
    public List<ItemDefinition> availableItems { get; }

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
    public UpgradeHeroPanel( List<HeroDefinition> heroes , float width , float height , float spacing , float padding , int rows ) : base( width , height )
    {
        int count = heroes.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        contents = new List<MeshRenderer>( count );

        for ( int i = 0 ; count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            MeshRenderer segment = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();

            segment.transform.SetParent( container.transform );
            segment.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
            segment.transform.localScale = new Vector3( size.x , size.y , 1 );
            segment.transform.localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            segment.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            segment.material.color = Color.red;
            segment.name = "Segment" + i;
            contents.Add( segment );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.75f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
    }
}

public class UpgradeItemPanel : Panel
{
    public UpgradeItemPanel( List<ItemDefinition> items , float width , float height , float spacing , float padding , int rows ) : base( width , height )
    {
        int count = items.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        contents = new List<MeshRenderer>( count );

        for ( int i = 0 ; count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            MeshRenderer segment = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();

            segment.transform.SetParent( container.transform );
            segment.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
            segment.transform.localScale = new Vector3( size.x , size.y , 1 );
            segment.transform.localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            segment.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            segment.material.color = Color.red;
            segment.name = "Segment" + i;
            contents.Add( segment );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.25f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
    }
}

public class BuyPanel : Panel
{
    public BuyPanel( List<HeroDefinition> heroes , List<ItemDefinition> items , float width , float height , float spacing , float padding , int rows ) : base ( width , height )
    {
        int count = heroes.Count + items.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );
        contents = new List<MeshRenderer>( count );

        for ( int i = 0 ; count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            MeshRenderer segment = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();

            segment.transform.SetParent( container.transform );
            segment.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
            segment.transform.localScale = new Vector3( size.x , size.y , 1 );
            segment.transform.localPosition = new Vector3( ( -width * 0.5f ) + ( size.x * x ) + ( size.x * 0.5f ) + ( spacing * x ) + padding , 1 , ( height * 0.5f ) - ( size.y * y ) - ( size.y * 0.5f ) - ( spacing * y ) - padding );
            segment.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            segment.material.color = Color.red;
            segment.name = "Segment" + i;
            contents.Add( segment );
            x++;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f , 0 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.forward * ( ( height * 0.5f ) + 1 ) );
    }
}

public class Panel
{
    protected List<MeshRenderer> contents { get; set; }
    protected GameObject container { get; set; }
    protected MeshRenderer quad { get; set; }
    protected float width { get; set; }
    protected float height { get; set; }

    public Panel( float width , float height )
    {
        this.width = width;
        this.height = height;
        container = new GameObject( "Panel" );

        quad = GameObject.CreatePrimitive( PrimitiveType.Quad ).GetComponent<MeshRenderer>();
        quad.transform.SetParent( container.transform );
        quad.transform.localRotation = Quaternion.Euler( 90 , 0 , 0 );
        quad.transform.localScale = new Vector3( width , height , 1 );
        quad.transform.name = "PanelBG";
    }
}