using System.Collections.Generic;
using UnityEngine;

public class ItemDefinition : EntityDefinition
{
    #if UNITY_EDITOR
    public void AddLevel()
    {
        ItemLevel itemLevel = ScriptableObject.CreateInstance<ItemLevel>();
        ScriptableObjects.Add(itemLevel, this);
        levels.Add(itemLevel);
    }
    #endif //UNITY_EDITOR

    public void RemoveLevel(int index) => levels.RemoveAt(index);

    public int Value(int index) => levels[ index ].value;
    public int Damage(int index) => levels[ index ].damage;

    public List<Definitions.Effects> Effects(int index) => levels[ index ].effects;
    public void Add(int index, Definitions.Effects effect) => levels[ index ].Add(effect);
    public void Remove(int index, Definitions.Effects effect) => levels[ index ].Remove(effect);

    public void SetDamage(int index, int damage) => levels[ index ].SetDamage(damage);
    public void SetValue(int index, int value) => levels[ index ].SetValue(value);

    public ItemDefinition Initialize(string name, float width, float laneHeightPadding, Definitions.Items type)
    {
        this.name = name;
        this.type = type;
        Initialize(name, width, laneHeightPadding);
        return this;
    }

    public Definitions.Items type;
    public List<ItemLevel> levels = new List<ItemLevel>();
}
