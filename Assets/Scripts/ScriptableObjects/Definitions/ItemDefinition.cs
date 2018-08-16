﻿using System.Collections.Generic;
using UnityEngine;

public class ItemDefinition : EntityDefinition
{
    public void AddLevel()
    {
        ItemLevel itemLevel = ScriptableObject.CreateInstance<ItemLevel>();
        ScriptableObjects.Add(itemLevel, this);
        levels.Add(itemLevel);
    }

    public void RemoveLevel(int index) => levels.RemoveAt(index);

    public int Damage(int index) => levels[ index ].damage;
    public List<Definitions.Effects> Effects(int index) => levels[ index ].effects;
    public void Add(int index, Definitions.Effects effect) => levels[ index ].Add(effect);
    public void Remove(int index, Definitions.Effects effect) => levels[ index ].Remove(effect);
    public void Set(int index, int damage) => levels[ index ].damage = damage;

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