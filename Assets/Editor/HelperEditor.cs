using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class HelperEditor : EditorWindow
{
    const string CARD_DATA_PATH = "Resources/Datas/Card/";
    [MenuItem("CustomWindow/HelperWindow")]
    void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("카드데이터 생성")))
        {
            CreateCardData();
        }
    }


    void CreateCardData()
    {
        for (int i = 0; i < Define.CARD_COUNT; i++)
        {
            Define.CardName cardName = (Define.CardName)i;

            Object tempObj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/" + CARD_DATA_PATH + cardName.ToString() + ".prefab");
            if (tempObj == null)
            {
                CardData tempData  = ScriptableObject.CreateInstance<CardData>();
                GameObject temp = Instantiate(tempObj) as GameObject; //인스턴스 만들기
                temp.name = cardName.ToString();
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(temp, "Assets/" + CARD_DATA_PATH + temp.name + ".prefab", out bool isSuccess); //저장

                if (temp)
                    DestroyImmediate(temp);
                if (isSuccess)
                    AssetDatabase.DeleteAsset("Assets/" + CARD_DATA_PATH + cardName.ToString() + ".prefab");
            }
        }
    }
    public static T[] GetAssetsAtPath<T>(string path) where T : Object
    {
        List<T> returnList = new List<T>();

        //get the contents of the folder's full path (excluding any meta files) sorted alphabetically
        IEnumerable<string> fullpaths = Directory.GetFiles(FullPathForRelativePath(path)).Where(x => !x.EndsWith(".meta")).OrderBy(s => s);
        //loop through the folder contents
        foreach (string fullpath in fullpaths)
        {
            //determine a path starting with Assets
            string assetPath = fullpath.Replace(Application.dataPath, "Assets");
            //load the asset at this relative path
            Object obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            //and add it to the list if it is of type T
            if (obj is T) { returnList.Add(obj as T); }
        }

        return returnList.ToArray();
    }

    private static string FullPathForRelativePath(string path)
    {
        throw new System.NotImplementedException();
    }
}
