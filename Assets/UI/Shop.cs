using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Metastructure idea
//- Persistent heroes -- start with one, which must be healed and upgraded etc, permadeath
//- Buy, equip and fuse heroes between rounds, game ends when all heroes are dead
//- Get that in place so testing can begin in earnest and we am has gaem

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

    public Shop( Player player ) : this()
    {
        Show( player );
    }
}

public class UpgradeHeroPanel : Layout
{
    public UpgradeHeroPanel( Player player , List<Definitions.Heroes> heroes , float width , float height , float spacing , float padding , int rows ) : base( "UpgradeHero" , width , height , padding , spacing , rows )
    {
        int count = heroes.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;
        int x = 0;
        int y = 0;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );

        for ( int i = 0 ; count > i ; i++ )
        {
            if ( x > perRow - 1 )
            {
                x = 0;
                y++;
            }

            int index = i;
            Add(  new UpgradeElement( heroes[ i ].ToString() , size.x , size.y , Color.yellow , container , 
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
        Refresh();
    }
}

public class UpgradeItemPanel : Layout
{
    public UpgradeItemPanel( Player player , List<Definitions.Items> items , float width , float height , float spacing , float padding , int rows ) : base( "UpgradeItem" , width , height , padding , spacing , rows )
    {
        int count = items.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );

        for ( int i = 0 ; count > i ; i++ )
        {
            int index = i;
            Add( new UpgradeElement( items[ i ].ToString() , size.x , size.y , Color.red , container , 
                ( Button button ) => button.SetColor( Color.green ) ,
                ( Button button ) => 
                {
                    if ( Input.GetMouseButtonDown( 0 ) )
                        player.inventory.Settings( items[ index ] ).Upgrade();
                } ,
                ( Button button ) => button.SetColor( Color.white ) ) );
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.25f , 1 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.back * ( ( height * 0.5f ) + 1 ) );
        Refresh();
    }
}

public class BuyPanel : Layout
{
    public override void Update()
    {
        for ( int i = 0 ; contents.Count > i ; i++ )
            contents[ i ].Update();
    }

    public BuyPanel( Shop shop , Player player , List<Definitions.Heroes> heroes , List<Definitions.Items> items , float width , float height , float spacing , float padding , int rows ) : base ( "Buy" , width , height , padding , spacing , rows )
    {
        int count = heroes.Count + items.Count;
        int perRow = Mathf.CeilToInt( count / rows );
        int remainder = count - ( perRow * rows );
        perRow += remainder;

        Vector2 size = new Vector2( ( width - ( padding * 2 ) - ( spacing * ( perRow - 1 ) ) ) / perRow , ( height - ( padding * 2 ) - ( spacing * ( rows - 1 ) ) ) / rows );

        for ( int i = 0 ; count > i ; i++ )
        {
            int index = i;
            
            Add( i >= heroes.Count 
                ? new BuyElement( items[ index - heroes.Count ].ToString() , size.x , size.y , Color.red , container , 
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
                : new BuyElement( heroes[ index ].ToString() , size.x , size.y , Color.yellow , container ,
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
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f , 0 , Camera.main.transform.position.y ) );
        container.transform.position = worldPos + ( Vector3.forward * ( ( height * 0.5f ) + 1 ) );
        Refresh();
    }
}

public class UpgradeElement : Layout
{
    public UpgradeElement( string name , float width , float height , Color color , GameObject parent , Action<Button> Enter = null , Action<Button> Stay = null , Action<Button> Exit = null ) : base( "Upgrade" + name , width , height , 0 , 0 , 1 )
    {
        Add( new Button( "Upgrade" + name , "Upgrade\n" + name , width - 1 , height - 1 , parent , Enter , Stay , Exit ) );
        container.transform.SetParent( parent.transform );
        quad.material.color = color;
    }
}

public class BuyElement : Layout
{
    public BuyElement( string name , float width , float height , Color color , GameObject parent , Action<Button> Enter , Action<Button> Stay , Action<Button> Exit ) : base( "Buy" + name , width , height , 0 , 0 , 1 )
    {
        Add( new Button( "Buy" + name , "Buy\n" + name , width - 1 , height - 1 , parent , Enter , Stay , Exit ) );
        container.transform.SetParent( parent.transform );
        quad.material.color = color;
    }
}