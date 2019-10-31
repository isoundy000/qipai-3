using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class MyMenu
{

    //[MenuItem("Assets/Build AssetBundles")]
    //static void BuildAllAssetBundles()
    //{
    //    BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.Android);
    //}

    [MenuItem("MyMenu/Delete playerPrefs")]
    static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("MyMenu/Img2Prefab__SexImg")]
    static private void MakeAtlas_sex()
    {
        string startPath = Application.dataPath + "/Images/SexImg";
        string endPath = "Assets/Resources/SexImg";
        DoIt(startPath, endPath);
    }


    [MenuItem("MyMenu/Img2Prefab__HeadImg")]
    static private void MakeAtlas_head()
    {
        string startPath = Application.dataPath + "/Images/HeadImg";
        string endPath = "Assets/Resources/HeadImg";
        DoIt(startPath, endPath);
    }

    [MenuItem("MyMenu/Img2Prefab__ItemImg")]
    static private void MakeAtlas_item()
    {
        string startPath = Application.dataPath + "/Images/ItemImg";
        string endPath = "Assets/Resources/ItemImg";
        DoIt(startPath, endPath);
    }


    static private void DoIt(string startPath, string endPath)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(startPath);
        foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
        {
            string allPath = pngFile.FullName;
            string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            GameObject go = new GameObject(sprite.name);
            go.AddComponent<SpriteRenderer>().sprite = sprite;

            string prefabPath = endPath + "/" + sprite.name + ".prefab";
            PrefabUtility.CreatePrefab(prefabPath, go);
            GameObject.DestroyImmediate(go);
        }
    }
}
