public class EntityDefinition : DefinitionBase
{
    protected void Initialize( string name , float width , float laneHeightPadding )
    {
        this.name = name;
        this.width = width;
        this.laneHeightPadding = laneHeightPadding;
    }

    public float width;
    public float laneHeightPadding;
}
