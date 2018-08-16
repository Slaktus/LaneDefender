using System.Collections.Generic;
using UnityEngine;

public class HeroDefinition : EntityDefinition
{
    public void AddLevel()
    {
        HeroLevel heroLevel = ScriptableObject.CreateInstance<HeroLevel>();
        ScriptableObjects.Add(heroLevel, this);
        levels.Add(heroLevel);
    }

    public void RemoveLevel(int index) => levels.RemoveAt(index);

    public int Damage(int index) => levels[ index ].damage;
    public int Health(int index) => levels[ index ].health;
    public int Value(int index) => levels[ index ].value;

    public List<Definitions.Effects> Effects(int index) => levels[ index ].effects;
    public void Add(int index, Definitions.Effects effect) => levels[ index ].Add(effect);
    public void Remove(int index, Definitions.Effects effect) => levels[ index ].Remove(effect);

    public void SetHealth (int index, int health) => levels[ index ].SetHealth( health );
    public void SetDamage(int index, int damage) => levels[ index ].SetDamage( damage );
    public void SetValue(int index, int value) => levels[ index ].SetValue(value);

    public HeroDefinition Initialize(string name, float width, float laneHeightPadding, Definitions.Heroes type)
    {
        this.name = name;
        this.type = type;
        Initialize(name, width, laneHeightPadding);
        return this;
    }

    public Definitions.Heroes type;
    public List<HeroLevel> levels = new List<HeroLevel>();
}
