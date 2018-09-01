using UnityEngine;

public class Connection : ScriptableObject
{
    public Connection Initialize (int fromIndex , int toIndex)
    {
        this.fromIndex = fromIndex;
        this.toIndex = toIndex;
        return this;
    }

    public int fromIndex;
    public int toIndex;
}