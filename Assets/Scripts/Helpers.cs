using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float Normalize( float value , float max , float min = 0 ) => ( value - min ) / ( max - min );
}
