using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : LaneEntity
{
    /// <summary>
    /// Updates the item's position, cleaning it up if it reaches the end of the lane
    /// </summary>
    public override IEnumerator Update()
    {
        bool move = true;

        while ( container != null )
        {
            if ( overlap )
                Interaction( overlapping );

            if ( pushBack != null )
                move = !pushBack.MoveNext();

            if ( changeLane != null )
                move = !changeLane.MoveNext();

            if ( pushAhead != null )
                move = !pushAhead.MoveNext();

            bool destroy = 0 >= health;

            if ( destroy )
                Destroy();

            yield return null;
        }
    }

    public Color color { get { return meshRenderer.material.color; } set { meshRenderer.material.color = value; } }

    public Hero( HeroDefinition heroDefinition , Lane lane ) : base( heroDefinition.name , 0 , heroDefinition.width , heroDefinition.laneHeightPadding , heroDefinition.health , heroDefinition.value , lane )
    {
        position = lane.end + ( Vector3.left * ( ( heroDefinition.width * 0.5f ) + 3 ) );
        color = heroDefinition.color;
    }
}