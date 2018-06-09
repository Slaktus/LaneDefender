public class StageDefinition : DefinitionBase
{
    public void Initialize( float speed , float width , float height , float laneSpacing , int laneCount )
    {
        this.speed = speed;
        this.width = width;
        this.height = height;
        this.laneSpacing = laneSpacing;
        this.laneCount = laneCount;
    }

    float speed;
    float width;
    float height;
    float laneSpacing;
    int laneCount;
}