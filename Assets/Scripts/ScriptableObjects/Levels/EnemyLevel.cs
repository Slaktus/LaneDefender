using System.Collections.Generic;
using UnityEngine;

public class EnemyLevel : ScriptableObject
{
    public void SetValue(int value) => this.value = value;
    public void SetSpeed(float speed) => this.speed = speed;
    public void SetHealth(int health) => this.health = health;
    public void SetDamage(int damage) => this.damage = damage;
    public void Add(Definitions.Effects effect) => effects.Add(effect);
    public void Remove(Definitions.Effects effect) => effects.Remove(effect);

    public int value;
    public int health;
    public int damage;
    public float speed;
    public float width;
    public List<Definitions.Effects> effects = new List<Definitions.Effects>();
}