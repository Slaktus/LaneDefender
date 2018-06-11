public class EnemyDefinition : EntityDefinition
{
    public EnemyDefinition Initialize( string name , float width , float laneHeightPadding , int value , Definitions.Enemies type )
    {
        this.type = type;
        this.value = value;
        Initialize( name , width , laneHeightPadding );
        return this;
    }

    public Definitions.Enemies type;
    public int value;
}
