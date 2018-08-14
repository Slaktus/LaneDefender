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
    public HeroSettings Settings( Definitions.Heroes hero ) => heroSettings[ heroes.IndexOf( hero ) ];
    public ItemSettings Settings(Definitions.Items item) => itemSettings[ items.IndexOf(item) ];
    public void AddCoins( int value ) => coins += value;

    public void AddHero( Definitions.Heroes hero )
    {
        heroes.Add( hero );
        heroSettings.Add( new HeroSettings( Color.white , 3 ) );
    }

    public void AddItem( Definitions.Items item , int level = 0 )
    {
        items.Add( item );
        itemSettings.Add(new ItemSettings(level, Definitions.GetEffects(item)));
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

        itemSettings = new List<ItemSettings>(items.Count)
        {
            new ItemSettings( 0 , Definitions.GetEffects(items[0])) ,
            new ItemSettings( 0 , Definitions.GetEffects(items[1])) ,
            new ItemSettings( 0 , Definitions.GetEffects(items[2]))
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
    public void Upgrade() => level++;

    public int level { get; private set; }
    public List<Definitions.Effects> effects { get; private set; }

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

    public ItemSettings( int level , List<Definitions.Effects> effects)
    {
        this.effects = effects;
        this.level = level;
    }
}

public class HeroSettings
{
    public void Upgrade() => level++;

    public int health
    {
        get
        {
            switch ( level )
            {
                default:
                    return 3;
            }
        }
    }

    public Color color { get; }
    public int level { get; private set; }

    public HeroSettings( Color color , int level = 0 )
    {
        this.color = color;
        this.level = level;
    }
}