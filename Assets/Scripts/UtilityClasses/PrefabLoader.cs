#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class PrefabLoader
{
    //So, there's no "load all assets in directory" function in unity. 
    //I guess this is to avoid people using Prefabs as "data blobs". 
    //They'd rather you use ScriptableObjects... which is fine in some cases, 
    //but sometimes the thing you're blobbing is just a bunch of child transforms anyway,
    //so it's huge buckets of sweat to reproduce the scene tools unity already has. 

    //The "AssetsDatabase.LoadAllAssetsAtPath" refers to /compound/ raw assets,
    //like maya files. But it doesn't care about humble prefabs, 
    //which are more like compounds of assets, rather than raw assets.

    //This function collates all the the behaviours you want in the directory you point it at. The path is relative to your Assets.
    //i.e. "Assets/MyDirectory/"
    //It returns the Prefab References, remember! Not instantiated scene objects! 
    //So it's only used for *editor-side* tools. Not run time.
    //Quite useful in conjunction with "PrefabUtility".
    public static List<T> LoadAllPrefabsOfType<T>(string path, bool includeSubDirectories) where T : MonoBehaviour
    {
        if (path != "")
        {
            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
        }

        DirectoryInfo dirInfo = new DirectoryInfo(path);
        FileInfo[] fileInf;
        if (includeSubDirectories)
        {
            fileInf = dirInfo.GetFiles("*.prefab", SearchOption.AllDirectories);
        }
        else
        {
            fileInf = dirInfo.GetFiles("*.prefab");
        }

        //loop through directory loading the game object and checking if it has the component you want
        List<T> prefabComponents = new List<T>();
        foreach (FileInfo fileInfo in fileInf)
        {
            string fullPath = fileInfo.FullName.Replace(@"\", "/");
            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

            if (prefab != null)
            {
                T hasT = prefab.GetComponent<T>();
                if (hasT != null)
                {
                    prefabComponents.Add(hasT);
                }
            }
        }
        return prefabComponents;
    }
}
#endif
