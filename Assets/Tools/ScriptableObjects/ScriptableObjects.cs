using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class ScriptableObjects
{
    public static void Add<T,Y>( T toAdd , Y addTo ) where T : ScriptableObject where Y : DefinitionBase
    {
        addTo.Add( toAdd );
        AssetDatabase.AddObjectToAsset( toAdd , addTo );
        AssetDatabase.SaveAssets();
    }

    public static void Remove<T,Y>( T toRemove , Y removeFrom ) where T : ScriptableObject where Y : DefinitionBase
    {
        removeFrom.Remove( toRemove );
        AssetDatabase.SaveAssets();
    }
}
#endif //UNITY_EDITOR

public class DefinitionBase : ScriptableObject
{
    public virtual void Add( ScriptableObject toAdd ) { }
    public virtual void Remove( ScriptableObject toRemove ) { }
}
