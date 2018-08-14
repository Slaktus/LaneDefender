using System.Collections.Generic;
using UnityEngine;

public class ItemSet : DefinitionBase
{
    public override void Add( ScriptableObject toAdd ) => itemDefinitions.Add( toAdd as ItemDefinition );
    public override void Remove( ScriptableObject toRemove ) => itemDefinitions.Remove( toRemove as ItemDefinition);

    public List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();
}