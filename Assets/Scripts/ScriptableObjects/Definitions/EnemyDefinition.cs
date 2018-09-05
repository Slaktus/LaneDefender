using System.Collections.Generic;
using UnityEngine;

public class EnemyDefinition : EntityDefinition
{
    #if UNITY_EDITOR
    public void AddLevel()
    {
        EnemyLevel enemyLevel = ScriptableObject.CreateInstance<EnemyLevel>();
        ScriptableObjects.Add(enemyLevel, this);
        levels.Add(enemyLevel);
    }
    #endif //UNITY_EDITOR

    public void RemoveLevel(int index) => levels.RemoveAt(index);

    public int Value(int index) => levels[ index ].value;
    public int Damage(int index) => levels[ index ].damage;
    public int Health(int index) => levels[ index ].health;
    public float Speed(int index) => levels[ index ].speed;

    public List<Definitions.Effects> Effects(int index) => levels[ index ].effects;
    public void Add(int index, Definitions.Effects effect) => levels[ index ].Add(effect);
    public void Remove(int index, Definitions.Effects effect) => levels[ index ].Remove(effect);

    public void SetDamage(int index, int damage) => levels[ index ].SetDamage(damage);
    public void SetHealth(int index, int health) => levels[ index ].SetHealth(health);
    public void SetColor(int index, Color color) => levels[ index ].SetColor(color);
    public void SetValue(int index, int value) => levels[ index ].SetValue(value);
    public void SetSpeed(int index, int speed) => levels[ index ].SetSpeed(speed);
    public void SetWidth(float width) => this.width = width;
    public void SetLaneHeightPadding(float laneHeightPadding) => this.laneHeightPadding = laneHeightPadding;

    public EnemyDefinition Initialize(string name, float width, float laneHeightPadding, Definitions.Enemies type)
    {
        this.name = name;
        this.type = type;
        Initialize(name, width, laneHeightPadding);
        return this;
    }

    public Definitions.Enemies type;
    public List<EnemyLevel> levels = new List<EnemyLevel>();
}
