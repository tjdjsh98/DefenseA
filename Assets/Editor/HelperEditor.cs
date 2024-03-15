using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using static Define;

public class HelperEditor : EditorWindow
{
    const string GIRL_CARD_DATA_PATH = "Data/소녀카드.csv";
    const string CREATURE_CARD_DATA_PATH = "Data/괴물카드.csv";
    const string WALL_CARD_DATA_PATH = "Data/벽카드.csv";
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
            CreateGirlCardData();
            CreateCreatureCardData();
            CreateWallCardData();
        }
    }
    void CreateGirlCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + GIRL_CARD_DATA_PATH);
        if (textAsset == null) return;

        string[] lines = textAsset.text.Split('\n');

        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;
            Debug.Log(words.Length);
            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length-1 , 1);

            GirlCardData data = ScriptableObject.CreateInstance<GirlCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;

            string[] priorCards = words[4].Split("|",options:System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<Define.CardName>();
            foreach (var priorCard in priorCards)
            {
                data.PriorCards.Add(GetCardName(priorCard));
            }
            data.MaxUpgradeCount = words[5].Equals("") ? 0 : int.Parse(words[5]);
            data.IncreaseHp = words[6].Equals("") ? 0 : int.Parse(words[6]);
            data.IncreaseRecoverHpPower = words[7].Equals("") ? 0 : float.Parse(words[7]);
            data.IncreaseDamageReducePercentage = words[8].Equals("") ? 0 : float.Parse(words[8]);

            data.UnlockAbility = GetGirlAbility(words[9]);
            data.DecreaseFireDelayPercentage = words[10].Equals("") ? 0 : float.Parse(words[10]);
            data.IncreaseReloadSpeedPercentage = words[11].Equals("") ? 0 : float.Parse(words[11]);
            data.IncreasePenerstratingPower = words[12].Equals("") ? 0 : int.Parse(words[12]);
            data.IncreaseAttackPoint = ParseInt(words[13]);

            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    void CreateCreatureCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + CREATURE_CARD_DATA_PATH);
        if (textAsset == null) return;  

        string[] lines = textAsset.text.Split('\n');

        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;
            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length-1 , 1);

            CreatureCardData data = ScriptableObject.CreateInstance<CreatureCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;
            string[] priorCards = words[4].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<Define.CardName>();
            foreach (var priorCard in priorCards)
            {
                data.PriorCards.Add(GetCardName(priorCard));
            }
            data.MaxUpgradeCount = words[5].Equals("") ? 0 : int.Parse(words[5]);
            data.IncreaseHp = words[6].Equals("") ? 0 : int.Parse(words[6]);
            data.IncreaseRecoverHpPower = words[7].Equals("") ? 0 : float.Parse(words[7]);
            data.IncreaseDamageReducePercentage = words[8].Equals("") ? 0 : float.Parse(words[8]);
            data.IncreaseAttackPoint = words[9].Equals("") ? 0 : int.Parse(words[9]);
            data.UnlockAbility = GetCreatureAbility(words[10]);


            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    void CreateWallCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + WALL_CARD_DATA_PATH);
        if (textAsset == null) return;

        string[] lines = textAsset.text.Split('\n');

        string[] preHeadWords = lines[0].Split(',');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;
            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length - 1, 1);

            WallCardData data = ScriptableObject.CreateInstance<WallCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;
            string[] priorCards = words[4].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<Define.CardName>();
            foreach (var priorCard in priorCards)
            {
                data.PriorCards.Add(GetCardName(priorCard));
            }
            data.MaxUpgradeCount = words[5].Equals("") ? 0 : int.Parse(words[5]);
            data.IncreaseHp = words[6].Equals("") ? 0 : int.Parse(words[6]);
            data.IncreaseRecoverHpPower = words[7].Equals("") ? 0 : float.Parse(words[7]);
            data.IncreaseDamageReducePercentage = words[8].Equals("") ? 0 : float.Parse(words[8]);
            data.IncreaseAttackPoint = words[9].Equals("") ? 0 : int.Parse(words[9]);
            data.UnlockAbility = GetWallAbility(words[10]);
            data.SizeUpPercentage = ParseFloat(words[11]);

            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
    Define.GirlAbility GetGirlAbility(string name)
    {
        for (int i = 0; i < Define.GIRLABILITY_COUNT; i++)
        {
            if (name.Equals(((Define.GirlAbility)i).ToString()))
            {
                return (Define.GirlAbility)i;
            }
        }
        return GirlAbility.None;
    }
    Define.WallAbility GetWallAbility(string name)
    {
        for (int i = 0; i < Define.WALLABILITY_COUNT; i++)
        {
            if (name.Equals(((Define.WallAbility)i).ToString()))
            {
                return (Define.WallAbility)i;
            }
        }
        return WallAbility.None;
    }
    Define.CreatureAbility GetCreatureAbility(string name)
    {
        for (int i = 0; i < Define.CREATUREABILITY_COUNT; i++)
        {
            if (name.Equals(((Define.CreatureAbility)i).ToString()))
            {
                return (Define.CreatureAbility)i;
            }
        }
        return CreatureAbility.None;
    }
    int ParseInt(string value)
    {
        int result = 0;
        int.TryParse(value, out result);
        return result;
    }

    float ParseFloat(string value)
    {
        float result = 0;
        float.TryParse(value, out result);
        return result;
    }
    bool ParseBoolean(string value)
    {
        if (value.Equals("1"))
            return true;

        return false;
    }

}
