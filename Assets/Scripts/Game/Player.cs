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
        itemSettings.Add(new ItemSettings(level, GetEffects(item)));
    }

    public static Definitions.Effects[] GetEffects(Definitions.Items item)
    {
        switch (item)
        {
            case Definitions.Items.Damage:
                return new Definitions.Effects[] { Definitions.Effects.Damage, Definitions.Effects.PushBack };

            case Definitions.Items.LaneDown:
                return new Definitions.Effects[] { Definitions.Effects.LaneDown };

            case Definitions.Items.LaneUp:
                return new Definitions.Effects[] { Definitions.Effects.LaneUp };

            case Definitions.Items.Leap:
                return new Definitions.Effects[] { Definitions.Effects.Leap, Definitions.Effects.Damage, Definitions.Effects.PushBack };

            case Definitions.Items.Part:
                return new Definitions.Effects[] { Definitions.Effects.Damage, Definitions.Effects.PushBack };

            case Definitions.Items.Split:
                return new Definitions.Effects[] { Definitions.Effects.Damage, Definitions.Effects.PushBack, Definitions.Effects.Split };

            case Definitions.Items.Wreck:
                return new Definitions.Effects[] { Definitions.Effects.Leap, Definitions.Effects.Damage, Definitions.Effects.PushBack };

            default:
                return new Definitions.Effects[] { };
        }
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
            new ItemSettings( 0 , GetEffects(items[0])) ,
            new ItemSettings( 0 , GetEffects(items[1])) ,
            new ItemSettings( 0 , GetEffects(items[2]))
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
    public Definitions.Effects[] effects { get; private set; }

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

    public ItemSettings( int level , Definitions.Effects[] effects)
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