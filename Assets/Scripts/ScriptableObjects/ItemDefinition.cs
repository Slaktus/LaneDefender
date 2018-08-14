using System.Collections.Generic;

public class ItemDefinition : EntityDefinition
{
    public ItemDefinition Initialize(string name, float width, float laneHeightPadding, Definitions.Items type, List<Definitions.Effects> effects)
    {
        this.name = name;
        this.type = type;
        this.effects = effects;
        Initialize(name, width, laneHeightPadding);
        return this;
    }

    public Definitions.Items type;
    public List<Definitions.Effects> effects;
}