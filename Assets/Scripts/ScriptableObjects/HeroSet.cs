using System.Collections.Generic;
using UnityEngine;

public class HeroSet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => heroDefinitions.Add( toAdd as HeroDefinition );
    public override void Remove( ScriptableObject toRemove ) => heroDefinitions.Remove( toRemove as HeroDefinition );

    public List<HeroDefinition> heroDefinitions = new List<HeroDefinition>();
}