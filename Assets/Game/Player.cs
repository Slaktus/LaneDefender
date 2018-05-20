﻿using System.Collections.Generic;

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
    public void AddCoins( int value ) => coins += value;

    public List<Definitions.Heroes> heroes { get; }
    public List<ConveyorItem.Type> items { get; }
    public int coins { get; private set; }

    public Inventory()
    {
        heroes = new List<Definitions.Heroes>()
        {
            Definitions.Heroes.Default ,
            //Definitions.Heroes.Default ,
            //Definitions.Heroes.Default ,
        };

        items = new List<ConveyorItem.Type>()
        {
            ConveyorItem.Type.Leap,
            ConveyorItem.Type.Damage,
            ConveyorItem.Type.LaneUp,
            ConveyorItem.Type.LaneDown,
        };
    }

    public Inventory( Inventory inventory )
    {
        coins = inventory.coins;
        heroes = new List<Definitions.Heroes>( inventory.heroes );
        items = new List<ConveyorItem.Type>( inventory.items );
    }
}