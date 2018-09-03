using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string name;
    public Inventory inventory;
    public Progress progress;

    public Player()
    {
        name = "Player";
        progress = new Progress();
        inventory = new Inventory();
    }

    public Player( Player player )
    {
        name = player.name;
        inventory = new Inventory( player.inventory );
        progress = new Progress(player.progress);
    }

    public Player(Progress progress)
    {
        name = "Player";
        this.progress = progress;
        inventory = new Inventory();
    }
}

[System.Serializable]
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
        itemSettings.Add(new ItemSettings(level));
    }

    public List<HeroSettings> heroSettings;
    public List<ItemSettings> itemSettings;
    public List<Definitions.Heroes> heroes;
    public List<Definitions.Items> items;
    public int coins;

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
            new ItemSettings(0),
            new ItemSettings(0),
            new ItemSettings(0)
        };
    }

    public Inventory( Inventory inventory )
    {
        coins = inventory.coins;
        heroes = new List<Definitions.Heroes>( inventory.heroes );
        items = new List<Definitions.Items>( inventory.items );
    }
}

[System.Serializable]
public class Progress
{
    public void AddCampaignProgress() => campaignProgress.Add(new CampaignProgress());
    public void AddCompleted(int campaign, int mission) => campaignProgress[ campaign ].AddCompleted(mission);
    public bool HasCompleted(int campaign, int mission) => campaignProgress[ campaign ].HasCompleted(mission);
    public bool IsNewGame(int campaign) => campaignProgress[ campaign ].completed.Count == 0;
    public bool HasCampaignProgress(int index) => campaignProgress.Count > index;

    public List<CampaignProgress> campaignProgress;

    public Progress()
    {
        campaignProgress = new List<CampaignProgress>();
    }

    public Progress( Progress progress)
    {
        campaignProgress = progress.campaignProgress;
    }
}

[System.Serializable]
public class CampaignProgress
{
    public bool HasCompleted(int index) => completed.Contains(index);
    public void AddCompleted(int index) => completed.Add(index);

    public List<int> completed;

    public CampaignProgress()
    {
        completed = new List<int>();
    }
}

[System.Serializable]
public class ItemSettings
{
    public void Upgrade() => level++;

    public int level;

    public ItemSettings( int level )
    {
        this.level = level;
    }
}

[System.Serializable]
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

    public Color color;
    public int level;

    public HeroSettings( Color color , int level = 0 )
    {
        this.color = color;
        this.level = level;
    }
}