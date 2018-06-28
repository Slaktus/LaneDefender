using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class ScriptableObjects
{
    public static T Create<T>( string path ) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset( asset , path );
        return asset;
    }

    public static void Save() => AssetDatabase.SaveAssets();

    public static void Add<T,Y>( T toAdd , Y addTo ) where T : ScriptableObject where Y : DefinitionBase
    {
        addTo.Add( toAdd );
        AssetDatabase.AddObjectToAsset( toAdd , addTo );
    }

    public static void Remove<T,Y>( T toRemove , Y removeFrom ) where T : ScriptableObject where Y : DefinitionBase
    {
        removeFrom.Remove( toRemove );
    }
}
#endif //UNITY_EDITOR

public abstract class DefinitionBase : ScriptableObject
{
    public abstract void Add( ScriptableObject toAdd );
    public abstract void Remove( ScriptableObject toRemove );
}
