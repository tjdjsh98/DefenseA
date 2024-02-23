using MoreMountains.Feedbacks;
using PlasticGui.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.VersionControl;
using UnityEngine;

public class HelperEditor : EditorWindow
{
    const string DAUGHTER_CARD_DATA_PATH = "Data/카드정보.csv";
    const string FATHER_CARD_DATA_PATH = "Data/아빠카드.csv";
    const string CARD_FOLDER_DATA_PATH = "Resources/Datas/Card/";
    [MenuItem("CustomWindow/HelperWindow", false, 0)]
    static void Init()
    {
        // 생성되어있는 윈도우를 가져온다. 없으면 새로 생성한다. 싱글턴 구조인듯하다.
        HelperEditor window = (HelperEditor)EditorWindow.GetWindow(typeof(HelperEditor));
        window.Show();
    }
    void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("카드데이터 생성")))
        {
            CreateDaughterCardData();
            CreateFatherCardData();

        }
    }
    void CreateDaughterCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + DAUGHTER_CARD_DATA_PATH);

        string[] lines = textAsset.text.Split('\n');

        foreach (string line in lines)
        {
            string[] words = line.Split(',');
            DaughterCardData data = ScriptableObject.CreateInstance<DaughterCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;
            data.MaxUpgradeCount = words[5].Equals("") ? 0 : int.Parse(words[5]);
            data.IncreaseHp = words[6].Equals("") ? 0 : int.Parse(words[6]);
            data.IncreaseRecoverHpPower = words[7].Equals("") ? 0 : float.Parse(words[7]);
            data.IncreaseDamageReducePercentage = words[8].Equals("") ? 0 : float.Parse(words[8]);
            data.UnlockLastShot = words[9].Equals("1") ? true : false;
            data.UnlockFastReload = words[10].Equals("1") ? true : false;
            data.UnlockAutoReload = words[11].Equals("1") ? true : false;
            data.DecreaseFireDelayPercentage = words[12].Equals("") ? 0 : float.Parse(words[12]);
            data.IncreaseReloadSpeedPercentage = words[13].Equals("") ? 0 : float.Parse(words[13]);
            data.IncreaseControlReboundPowerPercentage = words[14].Equals("") ? 0 : float.Parse(words[14]);
            data.IncreasePenerstratePower = words[15].Equals("") ? 0 : int.Parse(words[15]);
            data.IncreaseAttackPower = words[16].Equals("") ? 0 : int.Parse(words[16]);

            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    void CreateFatherCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + FATHER_CARD_DATA_PATH);

        string[] lines = textAsset.text.Split('\n');

        foreach (string line in lines)
        {
            string[] words = line.Split(',');
            FatherCardData data = ScriptableObject.CreateInstance<FatherCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;
            data.MaxUpgradeCount = words[5].Equals("") ? 0 : int.Parse(words[5]);
            data.IncreaseHp = words[6].Equals("") ? 0 : int.Parse(words[6]);
            data.IncreaseRecoverHpPower = words[7].Equals("") ? 0 : float.Parse(words[7]);
            data.IncreaseDamageReducePercentage = words[8].Equals("") ? 0 : float.Parse(words[8]);
            data.IncreaseAttackPower= words[9].Equals("") ? 0 : int.Parse(words[9]);
            data.IncreaseNormalAttackSpeedPercentage = words[10].Equals("") ? 0 : float.Parse(words[10]);
            data.UnlockShockwave = words[11].Equals("1") ? true : false;
            data.IncreaseShockwaveDamagePercentage = words[12].Equals("") ? 0 : float.Parse(words[12]);
            data.IncreaseShockwaveRangePercentage = words[13].Equals("") ? 0 : float.Parse(words[13]);
            data.IncreaseShockwaveAttackSpeedPercentage = words[14].Equals("") ? 0 : float.Parse(words[14]);
            data.IncreaseShockwaveCount = words[15].Equals("") ? 0 : int.Parse(words[15]);
            data.UnlockStempGround = words[16].Equals("1") ? true : false;
            data.IncreaseStempGroundDamagePercentage = words[17].Equals("") ? 0 : float.Parse(words[17]);
            data.IncreaseStempGroundRangePercentage = words[18].Equals("") ? 0 : float.Parse(words[18]);

            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    void CreateCardData()
    {
        for (int i = 0; i < Define.CARD_COUNT; i++)
        {
            Define.CardName cardName = (Define.CardName)i;

            Object tempObj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/" + DAUGHTER_CARD_DATA_PATH + cardName.ToString() + ".prefab");
            if (tempObj == null)
            {
                CardData tempData = ScriptableObject.CreateInstance<CardData>();
                GameObject temp = Instantiate(tempObj) as GameObject; //인스턴스 만들기
                temp.name = cardName.ToString();
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(temp, "Assets/" + DAUGHTER_CARD_DATA_PATH + temp.name + ".prefab", out bool isSuccess); //저장

                if (temp)
                    DestroyImmediate(temp);
                if (isSuccess)
                    AssetDatabase.DeleteAsset("Assets/" + DAUGHTER_CARD_DATA_PATH + cardName.ToString() + ".prefab");
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

    Define.CardName GetCardName(string cardName)
    {
        for (int i = 0; i < Define.CARD_COUNT; i++)
        {
            if (cardName.Equals(((Define.CardName)i).ToString()))
            {
                return (Define.CardName)i;
            }
        }
        return Define.CardName.None;
    }
}
