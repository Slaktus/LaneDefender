using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string name { get; private set; }
    public Inventory inventory { get; }

    public Player()
    {
        name = "Player";
        inventory = new Inventory();
    }

    public Player( Player player )
    {
        name = player.name;
        inventory = new Inventory( player.inventory );
    }
}

public class Inventory
{
    public ItemSettings Settings( Definitions.Items item ) => itemSettings[ items.IndexOf( item ) ];
    public HeroSettings Settings( Definitions.Heroes hero ) => heroSettings[ heroes.IndexOf( hero ) ];
    public void AddCoins( int value ) => coins += value;

    public void AddHero( Definitions.Heroes hero )
    {
        heroes.Add( hero );
        heroSettings.Add( new HeroSettings( Color.white , 3 ) );
    }

    public void AddItem( Definitions.Heroes hero )
    {
        heroes.Add( hero );
        heroSettings.Add( new HeroSettings( Color.white , 3 ) );
    }

    public List<HeroSettings> heroSettings { get; }
    public List<ItemSettings> itemSettings { get; }
    public List<Definitions.Heroes> heroes { get; }
    public List<Definitions.Items> items { get; }
    public int coins { get; private set; }

    public Inventory()
    {
        heroes = new List<Definitions.Heroes>()
        {
            //Definitions.Heroes.Default ,
            //Definitions.Heroes.Default ,
            //Definitions.Heroes.Default ,
        };

        heroSettings = new List<HeroSettings>();

        items = new List<Definitions.Items>()
        {
            Definitions.Items.Damage,
            Definitions.Items.LaneUp,
            Definitions.Items.LaneDown,
        };

        itemSettings = new List<ItemSettings>( items.Count )
        {
            new ItemSettings( 0 ) ,
            new ItemSettings( 0 ) ,
            new ItemSettings( 0 )
        };
    }

    public Inventory( Inventory inventory )
    {
        coins = inventory.coins;
        heroes = new List<Definitions.Heroes>( inventory.heroes );
        items = new List<Definitions.Items>( inventory.items );
    }
}

public class ItemSettings
{
    public void SetLevel( int level ) => this.level = level;

    public int level { get; private set; }
    public int damage
    {
        get
        {
            switch ( level )
            {
                default:
                    return 1;
            }
        }
    }

    public ItemSettings( int level )
    {
        this.level = level;
    }
}

public class HeroSettings
{
    public Color color { get; }
    public int health { get; }

    public HeroSettings( Color color , int health )
    {
        this.color = color;
        this.health = health;
    }
}