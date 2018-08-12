using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : LaneEntity
{
    public override void Update()
    {
        if ( container != null )
        {
            if ( overlap )
                Interaction( overlapping );

            if ( pushBack != null )
                pushBack.MoveNext();

            if ( changeLane != null )
                changeLane.MoveNext();

            if ( pushAhead != null )
                pushAhead.MoveNext();

            bool destroy = 0 >= health;

            if ( destroy )
                Destroy();
        }
    }
    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }

    public Hero( HeroDefinition definition , HeroSettings settings , Lane lane ) : base( definition.name , 0 , definition.width , definition.laneHeightPadding , settings.health , 0 , lane )
    {
        position = lane.end + ( Vector3.left * ( ( definition.width * 0.5f ) + 3 ) );
        color = settings.color;
    }
}