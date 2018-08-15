using UnityEngine;

public class StageDefinition : ScriptableObject
{
    public static StageDefinition Default() => CreateInstance<StageDefinition>().Initialize( 10 , 25 , 15 , 1 , 5 );

    public StageDefinition Initialize( float speed , float width , float height , float laneSpacing , int laneCount )
    {
        this.speed = speed;
        this.width = width;
        this.height = height;
        this.laneSpacing = laneSpacing;
        this.laneCount = laneCount;
        return this;
    }

    public float speed;
    public float width;
    public float height;
    public float laneSpacing;
    public int laneCount;
}