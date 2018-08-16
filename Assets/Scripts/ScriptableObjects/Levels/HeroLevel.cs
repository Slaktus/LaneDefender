using System.Collections.Generic;
using UnityEngine;

public class HeroLevel : ScriptableObject
{
    public void SetHealth(int health) => this.health = health;
    public void SetDamage(int damage) => this.damage = damage;
    public void Add(Definitions.Effects effect) => effects.Add(effect);
    public void Remove(Definitions.Effects effect) => effects.Remove(effect);

    public int health;
    public int damage;
    public List<Definitions.Effects> effects = new List<Definitions.Effects>();
}