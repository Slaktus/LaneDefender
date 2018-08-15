using System.Collections.Generic;

public class ItemDefinition : EntityDefinition
{
    public void AddLevel()
    {
        effects.Add(new List<Definitions.Effects>());
        damage.Add(0);
        levels++;
    }

    public void RemoveLevel(int index)
    {
        damage.RemoveAt(index);
        effects.RemoveAt(index);
        levels--;
    }

    public int Damage(int index) => damage[ index ];
    public List<Definitions.Effects> Effects(int index) => effects[ index ];
    public void Add(int index, Definitions.Effects effect) => effects[ index ].Add(effect);
    public void Remove(int index, Definitions.Effects effect) => effects[ index ].Remove(effect);
    public void Set(int index, int damage) => this.damage[ index ] = damage;

    public ItemDefinition Initialize(string name, float width, float laneHeightPadding, Definitions.Items type)
    {
        levels = 0;
        this.name = name;
        this.type = type;
        Initialize(name, width, laneHeightPadding);
        return this;
    }

    public int levels;
    public Definitions.Items type;
    public List<int> damage = new List<int>();
    public List<List<Definitions.Effects>> effects = new List<List<Definitions.Effects>>();
}