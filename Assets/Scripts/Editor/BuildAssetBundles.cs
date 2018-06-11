using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem( "Assets/Build Asset Bundles" )]
    static void BuildAssetBundles()
    {
        string source = "Assets/AssetBundleSource";
        string[] extensions = new string[] { "asset" , "asset" , };
        string[] sourceDirectories = new string[] { "Stages" , "Waves" , };

        string assetBundles = Path.Combine( UnityEngine.Application.streamingAssetsPath , "AssetBundles" );
        string pcAssets = Path.Combine( assetBundles , "PC" );
        string switchAssets = Path.Combine( assetBundles , "Switch" );

        if ( !Directory.Exists( pcAssets ) )
            Directory.CreateDirectory( pcAssets );

        if ( !Directory.Exists( switchAssets ) )
            Directory.CreateDirectory( switchAssets );

        AssetBundleBuild[] builds = new AssetBundleBuild[ sourceDirectories.Length ];
        LoadAssets( source , sourceDirectories , extensions , builds );

        BuildPipeline.BuildAssetBundles( pcAssets , builds , BuildAssetBundleOptions.None , BuildTarget.StandaloneWindows );
        BuildPipeline.BuildAssetBundles( switchAssets , builds , BuildAssetBundleOptions.None , BuildTarget.Switch );
        AssetDatabase.Refresh();
    }

    private static void LoadAssets( string dataAssets , string[] dataDirectories , string[] extensions , AssetBundleBuild[] builds )
    {
        for ( int i = 0; dataDirectories.Length > i; i++ )
        {
            string path = dataAssets + "/" + dataDirectories[ i ];
            builds[ i ].assetBundleName = dataDirectories[ i ];

            UnityEngine.Debug.Log( path );

            if ( Directory.Exists( path ) )
            {
                string[] files = Directory.GetFiles( path , "*." + extensions[ i ] , SearchOption.TopDirectoryOnly );

                for ( int j = 0; files.Length > j; j++ )
                {
                    files[ j ] = path + "/" + files[ j ].Split( '\\' )[ 1 ];
                    UnityEngine.Debug.Log( files[ j ] );
                }

                builds[ i ].assetNames = files;
            }
        }
    }
}