public class HeroDefinition : EntityDefinition
{
    public HeroDefinition Initialize( string name , float width , float laneHeightPadding , Definitions.Heroes type )
    {
        this.type = type;
        Initialize( name , width , laneHeightPadding );
        return this;
    }

    public Definitions.Heroes type;
}
