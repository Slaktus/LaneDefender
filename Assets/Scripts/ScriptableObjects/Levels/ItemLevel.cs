using System.Collections.Generic;
using UnityEngine;


public class ItemLevel : ScriptableObject
{
    public void Set(int damage) => this.damage = damage;
    public void Add(Definitions.Effects effect) => effects.Add(effect);
    public void Remove(Definitions.Effects effect) => effects.Remove(effect);

    public int damage;
    public List<Definitions.Effects> effects = new List<Definitions.Effects>();
}